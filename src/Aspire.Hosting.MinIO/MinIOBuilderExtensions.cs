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

            // only set the access keys if we do not have profiles set
            if (!source.Resource.TryGetAnnotationsOfType<AWSProfileAnnotation>(out _))
            {
                context.EnvironmentVariables[Amazon.Runtime.EnvironmentVariablesAWSCredentials.ENVIRONMENT_VARIABLE_ACCESSKEY] = source.Resource.UserNameReference;
                context.EnvironmentVariables[Amazon.Runtime.EnvironmentVariablesAWSCredentials.ENVIRONMENT_VARIABLE_SECRETKEY] = source.Resource.PasswordParameter;
            }

            context.EnvironmentVariables[Amazon.Util.EC2InstanceMetadata.AWS_EC2_METADATA_DISABLED] = bool.TrueString;

            // .NET AWS SDK config
            context.EnvironmentVariables["AWS__ForcePathStyle"] = bool.TrueString;
            if (source.Resource.Region is { } region)
            {
                context.EnvironmentVariables["AWS__AuthenticationRegion"] = region;
            }

            context.EnvironmentVariables["AWS__UseAccelerateEndpoint"] = bool.FalseString;
        });

        return builder;
    }

    /// <summary>
    /// Adds the configuration resource to the MinIO container resource.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="configuration">The configuration builder.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<MinIOServerResource> WithReference(this IResourceBuilder<MinIOServerResource> builder, IResourceBuilder<AWS.IAWSProfileConfig> configuration) =>
        builder.WithConfiguration(configuration.Resource);

    /// <summary>
    /// Adds an <c>AMQP</c> resource to the MinIO container resource.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="amqp">The AMQP builder.</param>
    /// <param name="exchange">The exchange.</param>
    /// <param name="exchangeType">The exchange type.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<MinIOServerResource> WithAmqpReference(this IResourceBuilder<MinIOServerResource> builder, IResourceBuilder<IResourceWithConnectionString> amqp, string? exchange = default, string? exchangeType = "direct")
    {
        _ = builder
            .WithReference(amqp)
            .WaitFor(amqp);

        builder.WithEnvironment(callback =>
        {
            exchange ??= builder.Resource.Name;
            callback.EnvironmentVariables[$"MINIO_NOTIFY_AMQP_ENABLE_{amqp.Resource.Name}"] = "on";
            callback.EnvironmentVariables[$"MINIO_NOTIFY_AMQP_URL_{amqp.Resource.Name}"] = amqp.Resource.ConnectionStringExpression;
            callback.EnvironmentVariables[$"MINIO_NOTIFY_AMQP_EXCHANGE_{amqp.Resource.Name}"] = exchange;

            if (!string.IsNullOrEmpty(exchangeType))
            {
                callback.EnvironmentVariables[$"MINIO_NOTIFY_AMQP_EXCHANGE_TYPE_{amqp.Resource.Name}"] = exchangeType;

                if (exchangeType is "direct")
                {
                    callback.EnvironmentVariables[$"MINIO_NOTIFY_AMQP_ROUTING_KEY_{amqp.Resource.Name}"] = exchange;
                }
            }
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
    /// Adds the configuration to the MinIO container resource.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<MinIOServerResource> WithConfiguration(this IResourceBuilder<MinIOServerResource> builder, AWS.IAWSProfileConfig configuration)
    {
        foreach (var profile in configuration.Profiles)
        {
            _ = builder.WithProfile(profile);
        }

        return builder;
    }

    /// <summary>
    /// Adds the profile to the MinIO container resource.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="profile">The profile.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<MinIOServerResource> WithProfile(this IResourceBuilder<MinIOServerResource> builder, AWS.AWSProfile profile) =>
        builder.WithAnnotation(new AWSProfileAnnotation { Profile = profile });

    /// <summary>
    /// Adds a MinIO container to the application.
    /// </summary>
    /// <param name="builder">The <see cref="IDistributedApplicationBuilder"/>.</param>
    /// <param name="name">The name of the resource. This name will be used as the connection string name when referenced in a dependency.</param>
    /// <param name="userName">The parameter used to provide the user name for the MinIO resource. If <see langword="null"/> a default value will be used.</param>
    /// <param name="password">The parameter used to provide the administrator password for the MinIO resource. If <see langword="null"/> a random password will be generated.</param>
    /// <param name="apiPort">The API port.</param>
    /// <param name="consolePort">The console port.</param>
    /// <param name="regionEndPoint">The region end point.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    [System.Runtime.CompilerServices.OverloadResolutionPriority(-1)]
    public static IResourceBuilder<MinIOServerResource> AddMinIO(this IDistributedApplicationBuilder builder, string name, IResourceBuilder<ParameterResource>? userName = null, IResourceBuilder<ParameterResource>? password = null, int? apiPort = null, int? consolePort = null, Amazon.RegionEndpoint? regionEndPoint = null) =>
        builder.AddMinIO(name, userName, password, apiPort, consolePort, regionEndPoint?.SystemName);

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
        const string Alias = "aspire";

        var passwordParameter = password?.Resource ?? ParameterResourceBuilderExtensions.CreateDefaultPasswordParameter(builder, $"{name}-password");

        var minIOServer = new MinIOServerResource(name, userName?.Resource, passwordParameter, region);

        _ = builder.Eventing.Subscribe<Aspire.Hosting.ApplicationModel.ResourceReadyEvent>(minIOServer, AddUsers);

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
            .WithEnvironment(context =>
            {
                if (region is not null)
                {
                    context.EnvironmentVariables["MINIO_REGION"] = region;
                }
            })
            .WithEnvironment(context => context.EnvironmentVariables[$"MC_HOST_{Alias}"] = $"{Uri.UriSchemeHttp}://{minIOServer.UserNameReference.ValueExpression}:{minIOServer.PasswordParameter.Value}@localhost:{ApiPort}")
            .WithArgs("server", DataLocation)
            .WithHttpHealthCheck(path: "minio/health/live", endpointName: ApiEndpointName)
            .PublishAsContainer();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3011:Reflection should not be used to increase accessibility of classes, methods, or fields", Justification = "Checked")]
        static async Task AddUsers(ApplicationModel.ResourceReadyEvent e, CancellationToken ct)
        {
            var type = typeof(Aspire.Hosting.ApplicationModel.ResourceExtensions);
            if (type.GetMethod("GetResolvedResourceNames", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic) is { } method
                && method.Invoke(null, [e.Resource]) is IEnumerable<string> names
                && names.FirstOrDefault() is { } name
                && e.Resource.TryGetAnnotationsOfType<AWSProfileAnnotation>(out var profiles))
            {
                // get name
                var fileName = Environment.GetEnvironmentVariable("DOTNET_ASPIRE_CONTAINER_RUNTIME") ?? "docker";

                foreach (var profile in profiles.Select(x => x.Profile))
                {
                    _ = await AddUser(fileName, name, profile, ct).ConfigureAwait(false);
                    _ = await AttachPolicy(fileName, name, profile, ct).ConfigureAwait(false);
                }
            }

            static Task<int> AddUser(string containerRuntime, string containerName, AWS.AWSProfile profile, CancellationToken cancellationToken)
            {
                return RunProcess(containerRuntime, ["exec", containerName, "mc", "admin", "user", "add", Alias, profile.AccessKeyId.Value, profile.SecretAccessKey.Value], cancellationToken);
            }

            static Task<int> AttachPolicy(string containerRuntime, string containerName, AWS.AWSProfile profile, CancellationToken cancellationToken)
            {
                return RunProcess(containerRuntime, ["exec", containerName, "mc", "admin", "policy", "attach", Alias, "readwrite", "--user", profile.AccessKeyId.Value], cancellationToken);
            }

            static async Task<int> RunProcess(string fileName, IEnumerable<string> arguments, CancellationToken cancellationToken)
            {
                var processStartInfo = new System.Diagnostics.ProcessStartInfo(fileName)
                {
                    CreateNoWindow = true,
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                };

                foreach (var argument in arguments)
                {
                    processStartInfo.ArgumentList.Add(argument);
                }

                if (System.Diagnostics.Process.Start(processStartInfo) is { } process)
                {
                    await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
                    return process.ExitCode;
                }

                return -1;
            }
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Checked")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "This supression is required.")]
    private sealed class AWSProfileAnnotation : ApplicationModel.IResourceAnnotation
    {
        public required AWS.AWSProfile Profile { get; init; }
    }
}