// -----------------------------------------------------------------------
// <copyright file="GrpcBuilderExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting;

using Microsoft.Extensions.DependencyInjection;

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
    /// <param name="desiredScheme">The desired scheme.</param>
    /// <param name="endpointName">The end point name.</param>
    /// <returns>A resource builder with the health check annotation added.</returns>
    public static IResourceBuilder<T> WithGrpcHealthCheck<T>(this IResourceBuilder<T> builder, string desiredScheme, string endpointName)
        where T : IResourceWithEndpoints
    {
        var endpoint = builder.Resource.GetEndpoint(endpointName);

        var healthCheckKey = $"{builder.Resource.Name}_check";
        _ = builder.ApplicationBuilder.Eventing.Subscribe<AfterEndpointsAllocatedEvent>((_, _) => endpoint switch
        {
            { Exists: false } => throw new DistributedApplicationException($"The endpoint '{endpointName}' does not exist on the resource '{builder.Resource.Name}'."),
            { Scheme: { } scheme } when string.Equals(scheme, desiredScheme, StringComparison.Ordinal) => Task.CompletedTask,
            _ => throw new DistributedApplicationException($"The endpoint '{endpointName}' on resource '{builder.Resource.Name}' was not using the '{desiredScheme}' scheme."),
        });

        Uri? uri = null;
        _ = builder.ApplicationBuilder.Eventing.Subscribe<BeforeResourceStartedEvent>(builder.Resource, (_, _) =>
        {
            uri = new Uri(endpoint.Url, UriKind.Absolute);
            return Task.CompletedTask;
        });

        _ = builder.ApplicationBuilder.Services
            .AddHealthChecks()
            .Add(new Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckRegistration(
                healthCheckKey,
                _ => uri switch
                {
                    null => throw new DistributedApplicationException("The URI for the health check is not set. Ensure that the resource has been allocated before the health check is executed."),
                    _ => new GrpcHealthCheck(global::Grpc.Net.Client.GrpcChannel.ForAddress(uri)),
                },
                failureStatus: null,
                tags: null));

        _ = builder.WithHealthCheck(healthCheckKey);

        return builder;
    }

    /// <summary>
    /// Adds a <c>grpcui</c> platform to the application model.
    /// </summary>
    /// <typeparam name="T">The type of resource.</typeparam>
    /// <param name="builder">The resource builder.</param>
    /// <param name="containerName">The name of the container (Optional).</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<T> WithGrpcUI<T>(this IResourceBuilder<T> builder, string? containerName = default)
        where T : IResourceWithEndpoints => builder.WithGrpcUI(default(Action<IResourceBuilder<T>, IResourceBuilder<GrpcUIContainerResource>>), containerName);

    /// <summary>
    /// Adds a <c>grpcui</c> platform to the application model.
    /// </summary>
    /// <typeparam name="T">The type of resource.</typeparam>
    /// <param name="builder">The resource builder.</param>
    /// <param name="configureContainer">Callback to configure GrpcUI container resource.</param>
    /// <param name="containerName">The name of the container (Optional).</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<T> WithGrpcUI<T>(this IResourceBuilder<T> builder, Action<IResourceBuilder<GrpcUIContainerResource>>? configureContainer = null, string? containerName = default)
        where T : IResourceWithEndpoints => builder.WithGrpcUI((_, c) => configureContainer?.Invoke(c), containerName);

    /// <summary>
    /// Adds a <c>grpcui</c> platform to the application model.
    /// </summary>
    /// <typeparam name="T">The type of resource.</typeparam>
    /// <param name="builder">The resource builder.</param>
    /// <param name="configureExecutable">Callback to configure GrpcUI executable resource.</param>
    /// <param name="executableName">The name of the executable (Optional).</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<T> WithGrpcUI<T>(this IResourceBuilder<T> builder, Action<IResourceBuilder<GrpcUIExecutableResource>>? configureExecutable = null, string? executableName = default)
        where T : IResourceWithEndpoints => builder.WithGrpcUI((_, c) => configureExecutable?.Invoke(c), executableName);

    /// <summary>
    /// Adds a <c>grpcui</c> platform to the application model.
    /// </summary>
    /// <typeparam name="T">The type of resource.</typeparam>
    /// <param name="builder">The resource builder.</param>
    /// <param name="configureContainer">Callback to configure GrpcUI container resource.</param>
    /// <param name="containerName">The name of the container (Optional).</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<T> WithGrpcUI<T>(this IResourceBuilder<T> builder, Action<IResourceBuilder<T>, IResourceBuilder<GrpcUIContainerResource>>? configureContainer = null, string? containerName = default)
        where T : IResourceWithEndpoints => WithGrpcUI(
            builder,
            (applicationBuilder, name) => applicationBuilder
                .AddResource(new GrpcUIContainerResource(name))
                .WithImage(Grpc.GrpcUIContainerImageTags.Image, Grpc.GrpcUIContainerImageTags.Tag)
                .WithImageRegistry(Grpc.GrpcUIContainerImageTags.Registry),
            configureContainer,
            containerName);

    /// <summary>
    /// Adds a <c>grpcui</c> platform to the application model.
    /// </summary>
    /// <typeparam name="T">The type of resource.</typeparam>
    /// <param name="builder">The resource builder.</param>
    /// <param name="configureExecutable">Callback to configure GrpcUI executable resource.</param>
    /// <param name="executableName">The name of the executable (Optional).</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    [System.Runtime.CompilerServices.OverloadResolutionPriority(1)]
    public static IResourceBuilder<T> WithGrpcUI<T>(this IResourceBuilder<T> builder, Action<IResourceBuilder<T>, IResourceBuilder<GrpcUIExecutableResource>>? configureExecutable = null, string? executableName = default)
        where T : IResourceWithEndpoints => WithGrpcUI(
            builder,
            (applicationBuilder, name) => applicationBuilder.AddResource(new GrpcUIExecutableResource(name)),
            configureExecutable,
            executableName);

    private static IResourceBuilder<TResource> WithGrpcUI<TResource, TGrpcResource>(
        IResourceBuilder<TResource> builder,
        Func<IDistributedApplicationBuilder, string, IResourceBuilder<TGrpcResource>> factory,
        Action<IResourceBuilder<TResource>, IResourceBuilder<TGrpcResource>>? configureResource,
        string? resourceName)
        where TResource : IResourceWithEndpoints
        where TGrpcResource : IResourceWithEndpoints, IResourceWithArgs
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

        resourceName ??= $"{builder.Resource.Name}-grpcui";

        var resource = factory(builder.ApplicationBuilder, resourceName).ExcludeFromManifest();

        _ = resource.ApplicationBuilder.Eventing.Subscribe<AfterEndpointsAllocatedEvent>((_, _) =>
        {
            SetArguments(builder, resource, endpointType);
            return Task.CompletedTask;
        });

        _ = resource.WithEndpoint(targetPort: GetTargetPort(resource.Resource), scheme: endpointType);

        configureResource?.Invoke(builder, resource);

        return builder;

        static int? GetTargetPort(IResource resource)
        {
            const int DefaultPort = 8080;

            return resource is ContainerResource
                ? DefaultPort
                : FindFreePort();

            static int? FindFreePort()
            {
                using var socket = new System.Net.Sockets.Socket(
                    System.Net.Sockets.AddressFamily.InterNetwork,
                    System.Net.Sockets.SocketType.Stream,
                    System.Net.Sockets.ProtocolType.Tcp);
                try
                {
                    socket.Bind(new System.Net.IPEndPoint(System.Net.IPAddress.Any, 0));
                    if (socket.LocalEndPoint is System.Net.IPEndPoint localEndPoint)
                    {
                        return localEndPoint.Port;
                    }
                }
                finally
                {
                    socket.Close();
                }

                return null;
            }
        }

        static void SetArguments(IResourceBuilder<TResource> builder, IResourceBuilder<TGrpcResource> resource, string endpointType)
        {
            _ = resource.WithArgs(context =>
            {
                foreach (var arg in GetArgs(builder, resource, endpointType))
                {
                    context.Args.Add(arg);
                }
            });

            static IEnumerable<string> GetArgs(IResourceBuilder<TResource> builder, IResourceBuilder<TGrpcResource> resource, string endpointType)
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
                    var hostName = containerResource
                        .GetEndpoints()
                        .Select(ep => ep.ContainerHost)
                        .FirstOrDefault(x => x is not null) ?? "host.docker.internal";

                    return endpoint.Host
                        .Replace("localhost", hostName, StringComparison.OrdinalIgnoreCase)
                        .Replace("127.0.0.1", hostName, StringComparison.Ordinal)
                        .Replace("[::1]", hostName, StringComparison.Ordinal);
                }
            }
        }
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