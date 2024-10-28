// -----------------------------------------------------------------------
// <copyright file="MinIOBuilderExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting;

using Aspire.Hosting.ApplicationModel;

/// <summary>
/// Extensions for <c>MinIO</c>.
/// </summary>
public static class MinIOBuilderExtensions
{
    private const string UserEnvVarName = "MINIO_ROOT_USER";
    private const string PasswordEnvVarName = "MINIO_ROOT_PASSWORD";
    private const string ApiEndpointName = "api";
    private const string DataLocation = "/data";

    /// <summary>
    /// Injects service discovery information as environment variables from the project resource into the destination resource, using the source resource's name as the service name.
    /// Each endpoint defined on the project resource will be injected using the format <c>services__{sourceResourceName}__{endpointIndex}={endpointNameQualifiedUriString}</c>.
    /// </summary>
    /// <typeparam name="TDestination">The destination resource.</typeparam>
    /// <param name="builder">The resource where the service discovery information will be injected.</param>
    /// <param name="source">The resource from which to extract service discovery information.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<TDestination> WithReference<TDestination>(this IResourceBuilder<TDestination> builder, IResourceBuilder<MinIOServerResource> source)
        where TDestination : ApplicationModel.IResourceWithEnvironment
    {
        if (source is ApplicationModel.IResourceBuilder<IResourceWithServiceDiscovery> serviceDiscovery)
        {
            _ = builder.WithReference(serviceDiscovery);
        }

        _ = builder.WithEnvironment(context =>
        {
            // environment config
            context.EnvironmentVariables["AWS_ENDPOINT_URL_S3"] = source.Resource.GetEndpoint(ApiEndpointName);
            context.EnvironmentVariables["AWS_ACCESS_KEY_ID"] = source.Resource.UserNameReference;
            context.EnvironmentVariables["AWS_SECRET_ACCESS_KEY"] = source.Resource.PasswordParameter;
            context.EnvironmentVariables["AWS_EC2_METADATA_DISABLED"] = bool.TrueString;

            // .NET AWS SDK config
            context.EnvironmentVariables["AWS__ForcePathStyle"] = bool.TrueString;
            context.EnvironmentVariables["AWS__Region"] = source.Resource.Region;
            context.EnvironmentVariables["AWS__UseAccelerateEndpoint"] = bool.FalseString;
        });

        return builder;
    }

    /// <summary>
    /// Adds a named volume for the data folder to a MinIO container resource.
    /// </summary>
    /// <param name="builder">The resource builder.</param>
    /// <param name="name">The name of the volume. Defaults to an auto-generated name based on the application and resource names.</param>
    /// <param name="isReadOnly">A flag that indicates if this is a read-only volume.</param>
    /// <returns>The <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<MinIOServerResource> WithDataVolume(this IResourceBuilder<MinIOServerResource> builder, string? name = null, bool isReadOnly = false)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.WithVolume(name ?? Utils.VolumeNameGenerator.CreateVolumeName(builder, "data"), DataLocation, isReadOnly);
    }

    /// <summary>
    /// Adds a bind mount for the data folder to a MinIO container resource.
    /// </summary>
    /// <param name="builder">The resource builder.</param>
    /// <param name="source">The source directory on the host to mount into the container.</param>
    /// <param name="isReadOnly">A flag that indicates if this is a read-only mount.</param>
    /// <returns>The <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<MinIOServerResource> WithDataBindMount(this IResourceBuilder<MinIOServerResource> builder, string source, bool isReadOnly = false)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(source);

        return builder.WithBindMount(source, DataLocation, isReadOnly);
    }

    /// <summary>
    /// Adds a MinIO container to the application.
    /// </summary>
    /// <param name="builder">The <see cref="IDistributedApplicationBuilder"/>.</param>
    /// <param name="name">The name of the resource. This name will be used as the connection string name when referenced in a dependency.</param>
    /// <param name="userName">The parameter used to provide the user name for the MinIO resource. If <see langword="null"/> a default value will be used.</param>
    /// <param name="password">The parameter used to provide the administrator password for the MinIO resource. If <see langword="null"/> a random password will be generated.</param>
    /// <param name="apiPort">The API port.</param>
    /// <param name="consolePort">The console port.</param>
    /// <param name="region">The region.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<MinIOServerResource> AddMinIO(this IDistributedApplicationBuilder builder, string name, IResourceBuilder<ParameterResource>? userName = null, IResourceBuilder<ParameterResource>? password = null, int? apiPort = null, int? consolePort = null, string? region = default)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(name);

        const int ApiPort = 9000;
        const int ConsolePort = 9001;

        var passwordParameter = password?.Resource ?? ParameterResourceBuilderExtensions.CreateDefaultPasswordParameter(builder, $"{name}-password");
        region ??= "us-east-1";

        var minIOServer = new MinIOServerResource(name, userName?.Resource, passwordParameter, region);
        return builder.AddResource(minIOServer)
            .WithImage(MinIO.MinIOContainerImageTags.Image, MinIO.MinIOContainerImageTags.Tag)
            .WithImageRegistry(MinIO.MinIOContainerImageTags.Registry)
            .WithHttpEndpoint(port: apiPort, targetPort: ApiPort, name: ApiEndpointName)
            .WithHttpEndpoint(port: consolePort, targetPort: ConsolePort, name: "console")
            .WithEnvironment(context =>
            {
                context.EnvironmentVariables[UserEnvVarName] = minIOServer.UserNameReference;
                context.EnvironmentVariables[PasswordEnvVarName] = minIOServer.PasswordParameter;
            })
            .WithEnvironment("MINIO_STORAGE_CLASS_STANDARD", "EC:0")
            .WithEnvironment("MINIO_ADDRESS", () => $":{ApiPort}")
            .WithEnvironment("MINIO_CONSOLE_ADDRESS", () => $":{ConsolePort}")
            .WithEnvironment("MINIO_REGION", region)
            .WithArgs("server", DataLocation)
            .WithHttpHealthCheck(path: "minio/health/live", endpointName: ApiEndpointName)
            .PublishAsContainer();
    }
}