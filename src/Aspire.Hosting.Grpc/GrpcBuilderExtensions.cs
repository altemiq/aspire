// -----------------------------------------------------------------------
// <copyright file="GrpcBuilderExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting;

using Aspire.Hosting.ApplicationModel;

/// <summary>
/// <see cref="Grpc"/> health checks.
/// </summary>
public static class GrpcBuilderExtensions
{
    /// <summary>
    /// Adds a gRPC health check to the resource.
    /// </summary>
    /// <typeparam name="T">The type of endpoint resource.</typeparam>
    /// <param name="builder">The resource builder.</param>
    /// <param name="endpoint">The end point name.</param>
    /// <returns>A resource builder with the health check annotation added.</returns>
    public static IResourceBuilder<T> WithGrpcHealthCheck<T>(this IResourceBuilder<T> builder, string endpoint)
        where T : IResourceWithEndpoints => builder.WithAnnotation(HealthCheckAnnotation.Create(builder.Resource, (Uri uri) => new GrpcHealthCheck(global::Grpc.Net.Client.GrpcChannel.ForAddress(uri)), endpoint));

    /// <summary>
    /// Adds a grpcui platform to the application model.
    /// </summary>
    /// <typeparam name="T">The type of resource.</typeparam>
    /// <param name="builder">The resource builder.</param>
    /// <param name="configureContainer">Callback to configure GrpcUI container resource.</param>
    /// <param name="containerName">The name of the container (Optional).</param>
    /// <param name="wait">Set to <see langword="true"/> to wait for <paramref name="builder"/>; otherwise <see langword="false"/>.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<T> WithGrpcUI<T>(this IResourceBuilder<T> builder, Action<IResourceBuilder<GrpcUIContainerResource>>? configureContainer = null, string? containerName = default, bool wait = true)
        where T : IResourceWithEndpoints
    {
        // get the end point type
        var endpointType = "tcp";
        if (builder.Resource.TryGetEndpoints(out var endpoints))
        {
            if (endpoints.Any(ea => string.Equals(ea.UriScheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)))
            {
                endpointType = Uri.UriSchemeHttps;
            }
            else if (endpoints.Any(ea => string.Equals(ea.UriScheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)))
            {
                endpointType = Uri.UriSchemeHttp;
            }
        }

        const int Port = 8080;

        containerName ??= $"{builder.Resource.Name}-grpcui";

        var resource = builder.ApplicationBuilder
            .AddResource(new GrpcUIContainerResource(containerName))
            .WithImage(Grpc.GrpcUIContainerImageTags.Image, Grpc.GrpcUIContainerImageTags.Tag)
            .WithImageRegistry(Grpc.GrpcUIContainerImageTags.Registry)
            .Wait(builder, wait)
            .ExcludeFromManifest();

        resource.WithArgs(context =>
        {
            foreach (var arg in GetArgs(builder, resource, endpointType))
            {
                context.Args.Add(arg);
            }
        });

        if (string.Equals(endpointType, Uri.UriSchemeHttps, StringComparison.Ordinal))
        {
            resource.WithHttpsEndpoint(targetPort: Port);
        }
        else if (string.Equals(endpointType, Uri.UriSchemeHttp, StringComparison.Ordinal))
        {
            resource.WithHttpEndpoint(targetPort: Port);
        }

        configureContainer?.Invoke(resource);

        return builder;

        static IEnumerable<string> GetArgs<TResource>(IResourceBuilder<T> builder, IResourceBuilder<TResource> resource, string endpointType)
            where TResource : IResourceWithEndpoints
        {
            const int Timeout = 3600;

            // get the port
            var endpoint = resource.GetEndpoint(endpointType);
            var port = endpoint.TargetPort ?? endpoint.Port;

            yield return string.Create(System.Globalization.CultureInfo.InvariantCulture, $"-port={port}");
            yield return $"-connect-fail-fast={bool.FalseString}";
            yield return $"-connect-timeout={Timeout}";
            yield return "-vv";

            endpoint = builder.GetEndpoint(endpointType);
            if (string.Equals(endpoint.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase))
            {
                yield return $"-plaintext={bool.TrueString}";
            }

            var host = resource.Resource is ContainerResource containerResource
                ? GetHost(endpoint, containerResource)
                : endpoint.Host;

            yield return string.Create(System.Globalization.CultureInfo.InvariantCulture, $"{host}:{endpoint.Port}");

            static string GetHost(EndpointReference endpoint, ContainerResource containerResource)
            {
                var hostName = containerResource.GetEndpoints().Select(ep => ep.ContainerHost).FirstOrDefault(x => x is not null) ?? "host.docker.internal";

                return endpoint.Host
                    .Replace("localhost", hostName, StringComparison.OrdinalIgnoreCase)
                    .Replace("127.0.0.1", hostName, StringComparison.Ordinal)
                    .Replace("[::1]", hostName, StringComparison.Ordinal);
            }
        }
    }

    private static IResourceBuilder<T1> Wait<T1, T2>(this IResourceBuilder<T1> builder, IResourceBuilder<T2> source, bool wait)
        where T1 : IResource
        where T2 : IResourceWithEndpoints
    {
        if (wait)
        {
            builder.WaitFor(source);
        }

        return builder;
    }

    private sealed class GrpcHealthCheck(global::Grpc.Net.Client.GrpcChannel channel) : Microsoft.Extensions.Diagnostics.HealthChecks.IHealthCheck, IDisposable
    {
        private readonly global::Grpc.Health.V1.Health.HealthClient client = new(channel);

        public async Task<Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult> CheckHealthAsync(Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckContext context, CancellationToken cancellationToken = default) =>
            await this.client.CheckAsync(new global::Grpc.Health.V1.HealthCheckRequest(), cancellationToken: cancellationToken).ConfigureAwait(false) switch
            {
                { Status: global::Grpc.Health.V1.HealthCheckResponse.Types.ServingStatus.Serving } => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy(),
                { Status: global::Grpc.Health.V1.HealthCheckResponse.Types.ServingStatus.NotServing } => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy(),
                { Status: global::Grpc.Health.V1.HealthCheckResponse.Types.ServingStatus.ServiceUnknown } => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Degraded(),
                _ => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy(),
            };

        public void Dispose() => channel.Dispose();
    }
}