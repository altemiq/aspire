// -----------------------------------------------------------------------
// <copyright file="LocalStackBuilderExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting;

using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides extensions for <c>localstack</c>.
/// </summary>
public static class LocalStackBuilderExtensions
{
    private const string LocalStackConfigSection = "LocalStack";
    private const string DataLocation = "/var/lib/localstack/state";

    /// <summary>
    /// Adds a named volume for the data folder to a MinIO container resource.
    /// </summary>
    /// <param name="builder">The resource builder.</param>
    /// <param name="name">The name of the volume. Defaults to an auto-generated name based on the application and resource names.</param>
    /// <param name="isReadOnly">A flag that indicates if this is a read-only volume.</param>
    /// <returns>The <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<LocalStackServerResource> WithDataVolume(this IResourceBuilder<LocalStackServerResource> builder, string? name = null, bool isReadOnly = false)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder
            .WithVolume(name ?? VolumeNameGenerator.Generate(builder, "state"), DataLocation, isReadOnly)
            .WithEnvironment("LOCALSTACK_PERSISTENCE", "1");
    }

    /// <summary>
    /// Adds a bind mount for the data folder to a MinIO container resource.
    /// </summary>
    /// <param name="builder">The resource builder.</param>
    /// <param name="source">The source directory on the host to mount into the container.</param>
    /// <param name="isReadOnly">A flag that indicates if this is a read-only mount.</param>
    /// <returns>The <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<LocalStackServerResource> WithDataBindMount(this IResourceBuilder<LocalStackServerResource> builder, string source, bool isReadOnly = false)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(source);

        return builder
            .WithBindMount(source, DataLocation, isReadOnly)
            .WithEnvironment("LOCALSTACK_PERSISTENCE", "1");
    }

    /// <summary>
    /// Injects service discovery information as environment variables from the project resource into the destination resource, using the source resource's name as the service name.
    /// Each endpoint defined on the project resource will be injected using the format <c>services__{sourceResourceName}__{endpointIndex}={endpointNameQualifiedUriString}</c>.
    /// </summary>
    /// <typeparam name="TDestination">The destination resource.</typeparam>
    /// <param name="builder">The resource where the service discovery information will be injected.</param>
    /// <param name="source">The resource from which to extract service discovery information.</param>
    /// <param name="endpointName">The endpoint name.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<TDestination> WithReference<TDestination>(this IResourceBuilder<TDestination> builder, IResourceBuilder<LocalStackServerResource> source, string? endpointName = default)
        where TDestination : IResourceWithEnvironment
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(source);

        _ = Aspire.Hosting.ResourceBuilderExtensions.WithReference(builder, source);

        _ = builder.WithEnvironment((context) =>
        {
            // disable the AWS EC2 credentials lookup, as we are local
            context.EnvironmentVariables[Amazon.Util.EC2InstanceMetadata.AWS_EC2_METADATA_DISABLED] = bool.TrueString;

            context.EnvironmentVariables[$"{LocalStackConfigSection}__UseLocalStack"] = bool.TrueString;

            // config
            if (GetEndpoint(source.Resource, endpointName) is { } endpointReference)
            {
                context.EnvironmentVariables[$"{LocalStackConfigSection}__Config__LocalStackHost"] = new ParameterResource("localstackhost", _ => endpointReference.Host);
                context.EnvironmentVariables[$"{LocalStackConfigSection}__Config__EdgePort"] = new ParameterResource("endport", _ => endpointReference.Port.ToString(System.Globalization.CultureInfo.InvariantCulture));
                if (string.Equals(endpointReference.Scheme, Uri.UriSchemeHttps, StringComparison.Ordinal))
                {
                    context.EnvironmentVariables[$"{LocalStackConfigSection}__Config__UseSsl"] = bool.TrueString;
                }
            }

            // session
            context.EnvironmentVariables[$"{LocalStackConfigSection}__Session__RegionName"] = source.Resource.Region;
        });

        return builder;

        static EndpointReference? GetEndpoint(IResourceWithEndpoints resource, string? endpointName)
        {
            return endpointName is null ? resource.GetEndpoints().FirstOrDefault() : resource.GetEndpoint(endpointName);
        }
    }

    /// <summary>
    /// Adds a Local-Stack container to the application.
    /// </summary>
    /// <param name="builder">The <see cref="IDistributedApplicationBuilder"/>.</param>
    /// <param name="name">The name of the resource.</param>
    /// <param name="port">The port.</param>
    /// <param name="regionEndPoint">The region end point.</param>
    /// <param name="services">The requested services.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    [System.Runtime.CompilerServices.OverloadResolutionPriority(-1)]
    public static IResourceBuilder<LocalStackServerResource> AddLocalStack(this IDistributedApplicationBuilder builder, string name, int? port = null, Amazon.RegionEndpoint? regionEndPoint = null, LocalStackServices.Community services = default) => AddLocalStack(builder, name, port, regionEndPoint?.SystemName, services);

    /// <summary>
    /// Adds a Local-Stack container to the application.
    /// </summary>
    /// <param name="builder">The <see cref="IDistributedApplicationBuilder"/>.</param>
    /// <param name="name">The name of the resource.</param>
    /// <param name="port">The port.</param>
    /// <param name="region">The region.</param>
    /// <param name="services">The requested services.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<LocalStackServerResource> AddLocalStack(this IDistributedApplicationBuilder builder, string name, int? port = null, string? region = null, LocalStackServices.Community services = default)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(name);

        var localStack = new LocalStackServerResource(name, region ?? "us-east-1") { Services = services };

        var resourceBuilder = builder.AddResource(localStack)
            .WithImage(LocalStack.LocalStackContainerImageTags.Image, LocalStack.LocalStackContainerImageTags.Tag)
            .WithImageRegistry(LocalStack.LocalStackContainerImageTags.Registry)
            .WithHttpEndpoint(port, targetPort: 4566)
            .WithEnvironment(context =>
            {
                if (context.ExecutionContext.IsRunMode)
                {
                    context.EnvironmentVariables["LOCALSTACK_DEBUG"] = "1";
                }
            })
            .WithEnvironment(context =>
            {
                if (localStack.Services != default)
                {
                    context.EnvironmentVariables["LOCALSTACK_SERVICES"] = string.Join(',', localStack.GetServiceNames());
                }
            })
            .AddDockerSock()
            .PublishAsContainer();

        AddHealthCheck(resourceBuilder, Uri.UriSchemeHttp, Uri.UriSchemeHttp, services);

        return resourceBuilder;

        static void AddHealthCheck(IResourceBuilder<LocalStackServerResource> builder, string desiredScheme, string endpointName, LocalStackServices.Community services)
        {
            var endpoint = builder.Resource.GetEndpoint(endpointName);

            _ = builder.ApplicationBuilder.Eventing.Subscribe<AfterEndpointsAllocatedEvent>((_, __) => endpoint switch
            {
                { Exists: false } => throw new DistributedApplicationException($"The endpoint '{endpointName}' does not exist on the resource '{builder.Resource.Name}'."),
                { Scheme: { } scheme } when string.Equals(scheme, desiredScheme, StringComparison.Ordinal) => Task.CompletedTask,
                _ => throw new DistributedApplicationException($"The endpoint '{endpointName}' on resource '{builder.Resource.Name}' was not using the '{desiredScheme}' scheme."),
            });

            Uri? uri = null;
            _ = builder.ApplicationBuilder.Eventing.Subscribe<BeforeResourceStartedEvent>(builder.Resource, (_, __) =>
            {
                uri = new Uri(endpoint.Url, UriKind.Absolute);
                return Task.CompletedTask;
            });

            var healthCheckKey = $"{builder.Resource.Name}_{endpointName}_check";

            _ = builder.ApplicationBuilder.Services
                .AddHealthChecks()
                .Add(new Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckRegistration(
                    healthCheckKey,
                    serviceProvider => uri switch
                    {
                        null => throw new DistributedApplicationException("The URI for the health check is not set. Ensure that the resource has been allocated before the health check is executed."),
                        _ => new Aspire.Hosting.LocalStack.LocalStackHealthCheck(serviceProvider.GetRequiredService<HttpClient>) { Uri = uri, Services = services },
                    },
                    failureStatus: null,
                    tags: null));

            _ = builder.WithHealthCheck(healthCheckKey);
        }
    }

    private static IResourceBuilder<T> AddDockerSock<T>(this IResourceBuilder<T> builder)
        where T : ContainerResource => builder.WithContainerRuntimeArgs("-v", "/var/run/docker.sock:/var/run/docker.sock:ro");
}