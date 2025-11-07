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
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Public API")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Public API")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "This is required")]
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
        where T : IResourceWithEndpoints => WithGrpcHealthCheck(builder, desiredScheme, () => builder.GetEndpoint(endpointName));

    /// <summary>
    /// Adds a gRPC health check to the resource.
    /// </summary>
    /// <typeparam name="T">The type of endpoint resource.</typeparam>
    /// <param name="builder">The resource builder.</param>
    /// <param name="scheme">The desired scheme.</param>
    /// <param name="endpointSelector">The endpoint selector.</param>
    /// <returns>A resource builder with the health check annotation added.</returns>
    public static IResourceBuilder<T> WithGrpcHealthCheck<T>(this IResourceBuilder<T> builder, string scheme, Func<EndpointReference> endpointSelector)
        where T : IResourceWithEndpoints
    {
        var endpoint = endpointSelector() ?? throw new DistributedApplicationException($"Could not create health check for resource '{builder.Resource.Name}' as the endpoint selector returned null.");

        var endpointName = endpoint.EndpointName;

        if (!string.Equals(endpoint.Scheme, scheme, StringComparison.Ordinal))
        {
            throw new DistributedApplicationException($"The endpoint '{endpointName}' on resource '{builder.Resource.Name}' was not using the '{scheme}' scheme.");
        }

        _ = builder.ApplicationBuilder.Eventing.Subscribe<ResourceEndpointsAllocatedEvent>(
            builder.Resource,
            (_, _) =>
            {
                if (endpoint.Exists)
                {
                    return Task.CompletedTask;
                }

                throw new DistributedApplicationException($"The endpoint '{endpointName}' does not exist on the resource '{builder.Resource.Name}'.");
            });

        Uri? uri = null;
        _ = builder.ApplicationBuilder.Eventing.Subscribe<BeforeResourceStartedEvent>(builder.Resource, (_, _) =>
        {
            uri = new(endpoint.Url, UriKind.Absolute);
            return Task.CompletedTask;
        });

        var healthCheckKey = $"{builder.Resource.Name}_check";
        _ = builder.ApplicationBuilder.Services
            .AddHealthChecks()
            .Add(new(
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

    /// <summary>
    /// Add the import path for the resource.
    /// </summary>
    /// <typeparam name="T">The type of gRPC resource.</typeparam>
    /// <param name="builder">The resource builder.</param>
    /// <param name="path">The path.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<T> WithImportPath<T>(this IResourceBuilder<T> builder, string path)
        where T : IGrpcUIResource => builder.WithAnnotation(new GrpcImportPathAnnotation(path));

    /// <summary>
    /// Add the base path for the resource.
    /// </summary>
    /// <typeparam name="T">The type of gRPC resource.</typeparam>
    /// <param name="builder">The resource builder.</param>
    /// <param name="path">The path.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<T> WithBasePath<T>(this IResourceBuilder<T> builder, string path)
        where T : IGrpcUIResource => builder.WithAnnotation(new GrpcBasePathAnnotation(path), ResourceAnnotationMutationBehavior.Replace);

    private static IResourceBuilder<TResource> WithGrpcUI<TResource, TGrpcResource>(
        IResourceBuilder<TResource> builder,
        Func<IDistributedApplicationBuilder, string, IResourceBuilder<TGrpcResource>> factory,
        Action<IResourceBuilder<TResource>, IResourceBuilder<TGrpcResource>>? configureResource,
        string? resourceName)
        where TResource : IResourceWithEndpoints
        where TGrpcResource : IGrpcUIResource
    {
        // get the end point type
        var endpointType = "tcp";
        if (builder.Resource.TryGetEndpoints(out var endpoints))
        {
            var endpointsArray = endpoints.ToArray();
            if (endpointsArray.Any(ea => string.Equals(ea.UriScheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)))
            {
                endpointType = Uri.UriSchemeHttps;
            }
            else if (endpointsArray.Any(ea => string.Equals(ea.UriScheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)))
            {
                endpointType = Uri.UriSchemeHttp;
            }
        }

        resourceName ??= $"{builder.Resource.Name}-grpcui";

        var resource = factory(builder.ApplicationBuilder, resourceName).ExcludeFromManifest();

        _ = resource.ApplicationBuilder.Eventing.Subscribe<ResourceEndpointsAllocatedEvent>(resource.Resource, (evt, cancellationToken) =>
        {
            var context = evt.Services.GetRequiredService<DistributedApplicationExecutionContext>();
            SetArguments(context, builder, resource, endpointType);
            return Task.CompletedTask;
        });

        _ = resource.WithEndpoint(targetPort: GetTargetPort(resource.Resource), scheme: endpointType);

        _ = resource.WithUrlForEndpoint(
            endpointType,
            c =>
            {
                if (resource.Resource.TryGetLastAnnotation<GrpcBasePathAnnotation>(out var basePathAnnotation))
                {
                    var uriBuilder = new UriBuilder(c.Url) { Path = basePathAnnotation.Path };
                    c.Url = uriBuilder.ToString();
                }
            });

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

        static void SetArguments(DistributedApplicationExecutionContext context, IResourceBuilder<TResource> source, IResourceBuilder<TGrpcResource> grpc, string endpointType)
        {
            _ = grpc.WithArgs(async ctx =>
            {
                await foreach (var arg in GetArgsAsync(context, source, grpc, endpointType, ctx.CancellationToken).ConfigureAwait(false))
                {
                    ctx.Args.Add(arg);
                }
            });

            static async IAsyncEnumerable<string> GetArgsAsync(
                DistributedApplicationExecutionContext context,
                IResourceBuilder<TResource> source,
                IResourceBuilder<TGrpcResource> grpc,
                string endpointType,
                [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
            {
                const int Timeout = 3600;

                // get the port
                var grpcEndpoint = grpc.GetEndpoint(endpointType);
                var port = grpcEndpoint.TargetPort ?? grpcEndpoint.Port;

                yield return string.Create(System.Globalization.CultureInfo.InvariantCulture, $"-port={port}");
                yield return $"-connect-fail-fast={bool.FalseString}";
                yield return $"-connect-timeout={Timeout}";
                yield return "-vv";

                if (grpc.Resource.TryGetAnnotationsOfType<GrpcImportPathAnnotation>(out var importPathAnnotations))
                {
                    foreach (var importPathAnnotation in importPathAnnotations)
                    {
                        yield return $"-import-path=\"{Path.GetFullPath(importPathAnnotation.Path.Replace('\\', Path.DirectorySeparatorChar))}\"";
                    }
                }

                if (grpc.Resource.TryGetLastAnnotation<GrpcBasePathAnnotation>(out var basePathAnnotation))
                {
                    yield return $"-base-path={basePathAnnotation.Path}";
                }

                var sourceEndpoint = source.GetEndpoint(endpointType);
                if (string.Equals(sourceEndpoint.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase))
                {
                    yield return $"-plaintext={bool.TrueString}";
                }

                var host = await GetHostUriAsync(context, sourceEndpoint, grpc.Resource, cancellationToken).ConfigureAwait(false);

                yield return host.GetComponents(UriComponents.HostAndPort, UriFormat.UriEscaped);
                static async ValueTask<Uri> GetHostUriAsync(DistributedApplicationExecutionContext context, EndpointReference endpoint, IResource? containerResource, CancellationToken cancellationToken = default)
                {
                    IValueProvider hostUrl = new HostUrl(endpoint.Url);
                    var valueProviderContext = new ValueProviderContext
                    {
                        ExecutionContext = context,
                        Network = containerResource is ContainerResource
                            ? KnownNetworkIdentifiers.DefaultAspireContainerNetwork
                            : default,
                        Caller = containerResource,
                    };

                    if (await hostUrl.GetValueAsync(valueProviderContext, cancellationToken).ConfigureAwait(false) is { } value
                        && Uri.TryCreate(value, UriKind.Absolute, out var uri))
                    {
                        return uri;
                    }

                    return new(endpoint.Url);
                }
            }
        }
    }

    private sealed class GrpcHealthCheck(global::Grpc.Net.Client.GrpcChannel channel) : Microsoft.Extensions.Diagnostics.HealthChecks.IHealthCheck, IDisposable
    {
        private readonly global::Grpc.Health.V1.Health.HealthClient client = new(channel);

        public async Task<Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult> CheckHealthAsync(Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckContext context, CancellationToken cancellationToken = default) =>
            await this.client.CheckAsync(new(), cancellationToken: cancellationToken).ConfigureAwait(false) switch
            {
                { Status: global::Grpc.Health.V1.HealthCheckResponse.Types.ServingStatus.Serving } => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy(),
                { Status: global::Grpc.Health.V1.HealthCheckResponse.Types.ServingStatus.NotServing } => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy(),
                { Status: global::Grpc.Health.V1.HealthCheckResponse.Types.ServingStatus.ServiceUnknown } => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Degraded(),
                _ => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy(),
            };

        public void Dispose() => channel.Dispose();
    }

    private sealed record GrpcImportPathAnnotation(string Path) : IResourceAnnotation;

    private sealed record GrpcBasePathAnnotation(string Path) : IResourceAnnotation;
}