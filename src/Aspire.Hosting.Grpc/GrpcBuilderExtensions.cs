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
    /// <param name="wait">Set to <see langword="true"/> to wait for <paramref name="builder"/>; otherwise <see langword="false"/>.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<T> WithGrpcUI<T>(this IResourceBuilder<T> builder, bool wait = true)
        where T : IResourceWithEndpoints
    {
        const int Port = 8080;
        const int Timeout = 3600;

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

        if (builder.ApplicationBuilder.Configuration.IsPodman())
        {
            var port = FindPort();
            var grpcUI = new ExecutableResource($"{builder.Resource.Name}-grpcui", Path.Combine(FindToolsDirectory("grpcui"), "grpcui"), string.Empty);
            _ = builder.ApplicationBuilder
                .AddResource(grpcUI)
                .WithArgs(GetDefaultArgs(port))
                .WithArgs(context =>
                {
                    foreach (var args in GetArgs(builder, grpcUI, endpointType))
                    {
                        context.Args.Add(args);
                    }
                })
                .WithHttpEndpoint(targetPort: port)
                .Wait(builder, wait)
                .ExcludeFromManifest();

            static int FindPort()
            {
                using var client = new System.Net.Sockets.UdpClient(0, System.Net.Sockets.AddressFamily.InterNetwork);
                if (client.Client.LocalEndPoint is System.Net.IPEndPoint endPoint)
                {
                    return endPoint.Port;
                }

                throw new InvalidOperationException();
            }
        }
        else
        {
            var grpcUI = new ContainerResource($"{builder.Resource.Name}-grpcui");
            _ = builder.ApplicationBuilder
                .AddResource(grpcUI)
                .WithImage(Grpc.GrpcUIContainerImageTags.Image, Grpc.GrpcUIContainerImageTags.Tag)
                .WithImageRegistry(Grpc.GrpcUIContainerImageTags.Registry)
                .WithArgs(GetDefaultArgs(Port))
                .WithArgs(context =>
                {
                    foreach (var arg in GetArgs(builder, grpcUI, endpointType))
                    {
                        context.Args.Add(arg);
                    }
                })
                .WithHttpEndpoint(targetPort: Port)
                .Wait(builder, wait)
                .ExcludeFromManifest();
        }

        static string[] GetDefaultArgs(int port)
        {
            return [string.Create(System.Globalization.CultureInfo.InvariantCulture, $"-port={port}"), $"-connect-fail-fast={bool.FalseString}", $"-connect-timeout={Timeout}", "-vv"];
        }

        static IEnumerable<string> GetArgs<TResource>(IResourceBuilder<T> builder, TResource resource, string endpointType)
            where TResource : IResource
        {
            var endpoint = builder.GetEndpoint(endpointType);
            if (string.Equals(endpoint.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase))
            {
                yield return $"-plaintext={bool.TrueString}";
            }

            var host = resource is ContainerResource containerResource
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

        static string FindToolsDirectory(string tool)
        {
            var currentPath = Path.GetDirectoryName(typeof(GrpcBuilderExtensions).Assembly.Location) ?? Environment.CurrentDirectory;
            while (Directory.Exists(currentPath))
            {
                var toolsPath = Path.Combine(currentPath, "tools");
                if (Directory.Exists(toolsPath))
                {
                    var toolPath = Path.Combine(toolsPath, tool);
                    if (Directory.Exists(toolPath))
                    {
                        return toolPath;
                    }
                }

                currentPath = Path.GetDirectoryName(currentPath);
            }

            throw new DirectoryNotFoundException();
        }

        return builder;
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