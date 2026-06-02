// -----------------------------------------------------------------------
// <copyright file="GarageBuilderExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

/// <summary>
/// Extensions for <c>Garage</c>.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Public API")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Public API")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "This is required")]
public static partial class GarageBuilderExtensions
{
    private const int S3ApiPort = 3900;
    private const string S3ApiEndpoint = "s3-api";
    private const int RpcPort = 3901;
    private const string RpcEndpoint = "rpc";
    private const int S3WebPort = 3902;
    private const string S3WebEndpoint = "s3-web";
    private const int AdminPort = 3903;
    private const string AdminEndpoint = "admin";
    private const int K2VPort = 3904;
    private const string K2VEndpoint = "k2v";

    private const string GarageExecutable = "/garage";
    private const string MetadataLocation = "/tmp/meta";
    private const string DataLocation = "/tmp/data";

    private const string DefaultGarageToml = $"""
                                              metadata_dir = "{MetadataLocation}"
                                              data_dir = "{DataLocation}"
                                              db_engine = "sqlite"

                                              replication_factor = 1

                                              rpc_bind_addr = "[::]:*"
                                              rpc_public_addr = "127.0.0.1:*"

                                              [s3_api]
                                              s3_region = "garage"
                                              api_bind_addr = "[::]:*"
                                              root_domain = ".s3.garage.localhost"

                                              [s3_web]
                                              bind_addr = "[::]:*"
                                              root_domain = ".web.garage.localhost"
                                              index = "index.html"

                                              [k2v_api]
                                              api_bind_addr = "[::]:*"

                                              [admin]
                                              api_bind_addr = "[::]:*"
                                              """;

    /// <summary>
    /// Adds a WebUI administration and development platform for Garage to the application model.
    /// </summary>
    /// <remarks>
    /// This version of the package defaults to the <see cref="Garage.GarageContainerImageTags.WebUITag"/> tag of the <see cref="Garage.GarageContainerImageTags.WebUIImage"/> container image.
    /// </remarks>
    /// <typeparam name="T">The type of garage server.</typeparam>
    /// <param name="builder">The Garage server resource builder.</param>
    /// <param name="configureContainer">Callback to configure WebUI container resource.</param>
    /// <param name="containerName">The name of the container (Optional).</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<T> WithGarageWebUI<T>(this IResourceBuilder<T> builder, Action<IResourceBuilder<GarageWebUIContainerResource>>? configureContainer = null, string? containerName = null)
        where T : GarageServerResource
    {
        ArgumentNullException.ThrowIfNull(builder);

        if (builder.ApplicationBuilder.Resources.OfType<GarageWebUIContainerResource>().SingleOrDefault() is { } existingWebUIResource)
        {
            var builderForExistingResource = builder.ApplicationBuilder.CreateResourceBuilder(existingWebUIResource);
            configureContainer?.Invoke(builderForExistingResource);
            return builder;
        }

        var webUIContainer = new GarageWebUIContainerResource(containerName ?? "garage-web-ui");
        var webUIContainerBuilder = builder.ApplicationBuilder.AddResource(webUIContainer)
            .WithImage(Garage.GarageContainerImageTags.WebUIImage, Garage.GarageContainerImageTags.WebUITag)
            .WithImageRegistry(Garage.GarageContainerImageTags.WebUIRegistry)
            .WithEnvironment("API_BASE_URL", builder.Resource.GetEndpoint(AdminEndpoint))
            .WithEnvironment("API_ADMIN_KEY", builder.Resource.PasswordParameter)
            .WithEnvironment("S3_REGION", builder.Resource.Region)
            .WithEnvironment("S3_ENDPOINT_URL", builder.Resource.GetEndpoint(S3ApiEndpoint))
            .WithHttpEndpoint(targetPort: 3909)
            .ExcludeFromManifest();

        configureContainer?.Invoke(webUIContainerBuilder);

        webUIContainerBuilder.WithRelationship(builder.Resource, "WebUI");

        return builder;
    }

    /// <summary>
    /// Adds a garage user parameter.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="name">The name of the parameter.</param>
    /// <returns>The parameter resource.</returns>
    public static IResourceBuilder<ParameterResource> AddGarageUserParameter(this IDistributedApplicationBuilder builder, string name) =>
        builder.AddParameter(name, new GarageUserNameDefaultParameter(), secret: false, persist: true);

    /// <summary>
    /// Adds a garage password parameter.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="name">The name of the parameter.</param>
    /// <returns>The parameter resource.</returns>
    public static IResourceBuilder<ParameterResource> AddGaragePasswordParameter(this IDistributedApplicationBuilder builder, string name) =>
        builder.AddParameter(name, new GaragePasswordDefaultParameter(), secret: true, persist: true);

    /// <summary>
    /// Adds the configuration to the Garage container resource.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<GarageServerResource> WithConfiguration(this IResourceBuilder<GarageServerResource> builder, AWS.IAWSProfileConfig configuration)
    {
        foreach (var profile in configuration.Profiles)
        {
            _ = builder.WithProfile(profile);
        }

        return builder;
    }

    /// <summary>
    /// Adds the profile to the Garage container resource.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="config">The profile configuration.</param>
    /// <param name="profile">The profile.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<GarageServerResource> WithProfile(this IResourceBuilder<GarageServerResource> builder, IResourceBuilder<AWS.IAWSProfileConfig> config, string profile) => builder.WithProfile(config.Resource, profile);

    /// <summary>
    /// Adds the profile to the Garage container resource.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="config">The profile configuration.</param>
    /// <param name="profile">The profile.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<GarageServerResource> WithProfile(this IResourceBuilder<GarageServerResource> builder, AWS.IAWSProfileConfig config, string profile)
    {
        if (config.Profiles.FirstOrDefault(p => string.Equals(p.Name, profile, StringComparison.Ordinal)) is { } profileConfig)
        {
            return builder.WithProfile(profileConfig);
        }

        return builder;
    }

    /// <summary>
    /// Adds the profile to the Garage container resource.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="profile">The profile.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<GarageServerResource> WithProfile(this IResourceBuilder<GarageServerResource> builder, AWS.AWSProfile profile)
    {
        if (builder.Resource.Annotations.OfType<AWSProfileAnnotation>().Any(a => a.Profile.Equals(profile)))
        {
            return builder;
        }

        return builder.WithAnnotation(new AWSProfileAnnotation { Profile = profile });
    }

    /// <summary>
    /// Adds a named volume for the data folder to a MinIO container resource.
    /// </summary>
    /// <param name="builder">The resource builder.</param>
    /// <param name="name">The name of the volume. Defaults to an auto-generated name based on the application and resource names.</param>
    /// <param name="isReadOnly">A flag that indicates if this is a read-only volume.</param>
    /// <returns>The <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<GarageServerResource> WithDataVolume(this IResourceBuilder<GarageServerResource> builder, string? name = null, bool isReadOnly = false)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.WithVolume(name ?? VolumeNameGenerator.Generate(builder, "data"), DataLocation, isReadOnly);
    }

    /// <summary>
    /// Injects service discovery information as environment variables from the project resource into the destination resource, using the source resource's name as the service name.
    /// Each endpoint defined on the project resource will be injected using the format <c>services__{sourceResourceName}__{endpointIndex}={endpointNameQualifiedUriString}</c>.
    /// </summary>
    /// <typeparam name="TDestination">The destination resource.</typeparam>
    /// <param name="builder">The resource where the service discovery information will be injected.</param>
    /// <param name="source">The resource from which to extract service discovery information.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<TDestination> WithReference<TDestination>(this IResourceBuilder<TDestination> builder, IResourceBuilder<GarageServerResource> source)
        where TDestination : IResourceWithEnvironment
    {
        if (source is IResourceBuilder<IResourceWithServiceDiscovery> serviceDiscovery)
        {
            _ = builder.WithReference(serviceDiscovery);
        }

        _ = builder.WithEnvironment(context =>
        {
            // environment config
            context.EnvironmentVariables[$"{Amazon.Util.EnvironmentVariables.SERVICE_SPECIFIC_ENDPOINT_ENVIRONMENT_VARIABLE_PREFIX}{Amazon.S3.AmazonS3Config.ServiceId}"] = source.Resource.GetEndpoint(S3ApiEndpoint);

            // only set the access keys if we do not have profiles set
            if (!source.Resource.TryGetAnnotationsOfType<AWSProfileAnnotation>(out _))
            {
                context.EnvironmentVariables[Amazon.Runtime.EnvironmentVariablesAWSCredentials.ENVIRONMENT_VARIABLE_ACCESSKEY] = source.Resource.UserNameReference;
                context.EnvironmentVariables[Amazon.Runtime.EnvironmentVariablesAWSCredentials.ENVIRONMENT_VARIABLE_SECRETKEY] = source.Resource.PasswordParameter;
            }

            context.EnvironmentVariables[Amazon.Util.EC2InstanceMetadata.AWS_EC2_METADATA_DISABLED] = bool.TrueString;

            // .NET AWS SDK config
            context.EnvironmentVariables[$"AWS__{nameof(Amazon.S3.AmazonS3Config.ForcePathStyle)}"] = bool.TrueString;
            if (source.Resource.Region is { } region)
            {
                context.EnvironmentVariables[$"AWS__{nameof(Amazon.S3.AmazonS3Config.AuthenticationRegion)}"] = region;
            }

            context.EnvironmentVariables[$"AWS__{nameof(Amazon.S3.AmazonS3Config.UseAccelerateEndpoint)}"] = bool.FalseString;
        });

        return builder;
    }

    /// <summary>
    /// Adds the configuration resource to the MinIO container resource.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="configuration">The configuration builder.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<GarageServerResource> WithReference(this IResourceBuilder<GarageServerResource> builder, IResourceBuilder<AWS.IAWSProfileConfig> configuration) =>
        builder.WithConfiguration(configuration.Resource);

    /// <summary>
    /// Ensures that the specified bucket exists.
    /// </summary>
    /// <typeparam name="TResource">The type of S3 resource.</typeparam>
    /// <param name="builder">The builder.</param>
    /// <param name="bucketName">The bucket name.</param>
    /// <param name="profile">The profile to use.</param>
    /// <returns>The resource builder.</returns>
    public static IResourceBuilder<TResource> EnsureBucket<TResource>(this IResourceBuilder<TResource> builder, string bucketName, string? profile = default)
        where TResource : GarageServerResource
    {
        // ensure we have a lock annotation
        if (!builder.Resource.HasAnnotationOfType<EnsureBucketLockAnnotation>())
        {
            _ = builder.WithAnnotation(new EnsureBucketLockAnnotation());
        }

        // check to see if we've already added the ensure bucket handler
        if (builder.Resource.TryGetLastAnnotation<EnsureBucketLockAnnotation>(out var lockAnnotation)
            && Interlocked.Exchange(ref lockAnnotation.Check, 1) is 0)
        {
            _ = builder.OnResourceReady(static async (resource, evt, cancellationToken) =>
            {
                // ensure the bucket exists
                var rls = evt.Services.GetRequiredService<ResourceLoggerService>();
                var logger = rls.GetLogger(resource);
                var containerRuntime = await ContainerRuntime.GetNameAsync(evt.Services, cancellationToken).ConfigureAwait(false);

                foreach (var annotation in evt.Resource.Annotations.OfType<BucketAnnotation>())
                {
                    var (annotationBucketName, annotationProfile) = annotation;

                    LogCreatingBucket(logger, annotationBucketName);

                    try
                    {
                        await evt.Resource.ExecAsync(containerRuntime, [GarageExecutable, "bucket", "create", annotationBucketName], logger, cancellationToken).ConfigureAwait(false);
                        await evt.Resource.ExecAsync(containerRuntime, [GarageExecutable, "bucket", "website", "--allow", annotationBucketName], logger, cancellationToken).ConfigureAwait(false);

                        if (annotationProfile is not null)
                        {
                            await evt.Resource.ExecAsync(
                                containerRuntime,
                                [
                                    GarageExecutable,
                                    "bucket",
                                    "allow",
                                    "--read",
                                    "--write",
                                    "--owner",
                                    annotationBucketName,
                                    "--key",
                                    annotationProfile,
                                ],
                                logger,
                                cancellationToken).ConfigureAwait(false);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogFailedBucketCreation(logger, annotationBucketName, ex);
                        throw;
                    }
                }
            });
        }

        _ = builder.WithAnnotation(new BucketAnnotation(bucketName, profile));

        return builder;
    }

    /// <summary>
    /// Adds a Garage container to the application.
    /// </summary>
    /// <param name="builder">The <see cref="IDistributedApplicationBuilder"/>.</param>
    /// <param name="name">The name of the resource. This name will be used as the connection string name when referenced in a dependency.</param>
    /// <param name="userName">The parameter used to provide the username for the Garage resource. If <see langword="null"/> a default value will be used.</param>
    /// <param name="password">The parameter used to provide the administrator password for the Garage resource. If <see langword="null"/> a random password will be generated.</param>
    /// <param name="config">The AWS config.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    [System.Runtime.CompilerServices.OverloadResolutionPriority(-1)]
    public static IResourceBuilder<GarageServerResource> AddGarage(this IDistributedApplicationBuilder builder, string name, IResourceBuilder<ParameterResource>? userName = null, IResourceBuilder<ParameterResource>? password = null, AWS.IAWSSDKConfig? config = null) =>
        builder.AddGarage(name, userName, password, config?.Region);

    /// <summary>
    /// Adds a Garage container to the application.
    /// </summary>
    /// <param name="builder">The <see cref="IDistributedApplicationBuilder"/>.</param>
    /// <param name="name">The name of the resource. This name will be used as the connection string name when referenced in a dependency.</param>
    /// <param name="userName">The parameter used to provide the username for the Garage resource. If <see langword="null"/> a default value will be used.</param>
    /// <param name="password">The parameter used to provide the administrator password for the Garage resource. If <see langword="null"/> a random password will be generated.</param>
    /// <param name="regionEndPoint">The region end point.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    [System.Runtime.CompilerServices.OverloadResolutionPriority(-1)]
    public static IResourceBuilder<GarageServerResource> AddGarage(this IDistributedApplicationBuilder builder, string name, IResourceBuilder<ParameterResource>? userName = null, IResourceBuilder<ParameterResource>? password = null, Amazon.RegionEndpoint? regionEndPoint = null) =>
        builder.AddGarage(name, userName, password, regionEndPoint?.SystemName);

    /// <summary>
    /// Adds a Garage container to the application.
    /// </summary>
    /// <param name="builder">The <see cref="IDistributedApplicationBuilder"/>.</param>
    /// <param name="name">The name of the resource. This name will be used as the connection string name when referenced in a dependency.</param>
    /// <param name="userName">The parameter used to provide the username for the Garage resource. If <see langword="null"/> a default value will be used.</param>
    /// <param name="password">The parameter used to provide the administrator password for the Garage resource. If <see langword="null"/> a random password will be generated.</param>
    /// <param name="region">The region.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<GarageServerResource> AddGarage(this IDistributedApplicationBuilder builder, string name, IResourceBuilder<ParameterResource>? userName = null, IResourceBuilder<ParameterResource>? password = null, string? region = default)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(name);

        var passwordParameter = password?.Resource ?? ParameterResourceBuilderExtensions.CreateDefaultPasswordParameter(builder, $"{name}-password");

        var garageServer = new GarageServerResource(name, userName?.Resource, passwordParameter, region);

        Lifecycle.EventingSubscriberServiceCollectionExtensions.TryAddEventingSubscriber<GarageEventSubscriber>(builder.Services);

        return builder.AddResource(garageServer)
            .WithImage(Garage.GarageContainerImageTags.Image, Garage.GarageContainerImageTags.Tag)
            .WithImageRegistry(Garage.GarageContainerImageTags.Registry)
            .WithContainerFiles(
                "/etc",
                async (context, cancellationToken) =>
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var toml = Tomlyn.TomlSerializer.Deserialize<Tomlyn.Model.TomlTable>(DefaultGarageToml)!;

                    toml["rpc_bind_addr"] = $"[::]:{RpcPort}";
                    toml["rpc_public_addr"] = $"127.0.0.1:{RpcPort}";
                    toml["rpc_secret"] = GetHexString(context.Model.Name, 32);

                    if (toml["s3_api"] is Tomlyn.Model.TomlTable s3ApiTable)
                    {
                        s3ApiTable["s3_region"] = region ?? "us-east-1";
                        s3ApiTable["api_bind_addr"] = $"[::]:{S3ApiPort}";
                    }

                    if (toml["s3_web"] is Tomlyn.Model.TomlTable s3WebTable)
                    {
                        s3WebTable["bind_addr"] = $"[::]:{S3WebPort}";
                    }

                    if (toml["k2v_api"] is Tomlyn.Model.TomlTable k2VApiTable)
                    {
                        k2VApiTable["api_bind_addr"] = $"[::]:{K2VPort}";
                    }

                    if (toml["admin"] is Tomlyn.Model.TomlTable adminTable)
                    {
                        var serverPassword = "password";
                        if (context.Model is GarageServerResource garageServerResource
                            && await garageServerResource.PasswordParameter.GetValueAsync(cancellationToken).ConfigureAwait(false) is { } passwordParameterValue)
                        {
                            serverPassword = passwordParameterValue;
                        }

                        adminTable["api_bind_addr"] = $"[::]:{AdminPort}";
                        adminTable["admin_token"] = serverPassword;
                        adminTable["metrics_token"] = serverPassword;
                    }

                    var file = new ContainerFile { Name = "garage.toml", Contents = Tomlyn.TomlSerializer.Serialize(toml) };
                    return [file];
                })
            .WithHttpEndpoint(targetPort: S3ApiPort, name: S3ApiEndpoint)
            .WithUrlForEndpoint(S3ApiEndpoint, callback => callback.DisplayText = S3ApiEndpoint)
            .WithHttpEndpoint(targetPort: RpcPort, name: RpcEndpoint)
            .WithUrlForEndpoint(RpcEndpoint, callback => callback.DisplayText = RpcEndpoint)
            .WithHttpEndpoint(targetPort: S3WebPort, name: S3WebEndpoint)
            .WithUrlForEndpoint(S3WebEndpoint, callback => callback.DisplayText = S3WebEndpoint)
            .WithHttpEndpoint(targetPort: AdminPort, name: AdminEndpoint)
            .WithUrlForEndpoint(AdminEndpoint, callback => callback.DisplayText = AdminEndpoint)
            .WithHttpEndpoint(targetPort: K2VPort, name: K2VEndpoint)
            .WithUrlForEndpoint(K2VEndpoint, callback => callback.DisplayText = K2VEndpoint)
            .WithHttpHealthCheck(path: "health", endpointName: AdminEndpoint)
            .PublishAsContainer();
    }

    private static string GetHexString(string name, int length) => GetHexString(new Random(StringComparer.Ordinal.GetHashCode(name)), length);

    private static string GetHexString(int length) => GetHexString(new Random(), length);

    private static string GetHexString(Random random, int length)
    {
        var bytes = new byte[length];
        random.NextBytes(bytes);
        return string.Concat(bytes.Select(b => b.ToString("x2", System.Globalization.CultureInfo.InvariantCulture)));
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Checking to see if {BucketName} exists")]
    private static partial void LogCheckBucketExists(ILogger logger, string bucketName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Creating {BucketName}")]
    private static partial void LogCreatingBucket(ILogger logger, string bucketName);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to create {BucketName}")]
    private static partial void LogFailedBucketCreation(ILogger logger, string bucketName, Exception exception);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Checked")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "This suppression is required.")]
    private sealed class AWSProfileAnnotation : IResourceAnnotation
    {
        public required AWS.AWSProfile Profile { get; init; }
    }

    private sealed record BucketAnnotation(string BucketName, string? Profile) : IResourceAnnotation;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "This is for locking")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "This is required")]
    private sealed class EnsureBucketLockAnnotation : IResourceAnnotation
    {
        public int Check;
    }

    private sealed class EchoLogger(ILogger logger) : ILogger
    {
        private readonly System.Text.StringBuilder stringBuilder = new();

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (this.stringBuilder.Length > 0)
            {
                this.stringBuilder.AppendLine();
            }

            this.stringBuilder.Append(formatter(state, exception));
            logger.Log(logLevel, eventId, state, exception, formatter);
        }

        public bool IsEnabled(LogLevel logLevel) => logger.IsEnabled(logLevel);

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull => logger.BeginScope(state);

        public override string ToString() => this.stringBuilder.ToString();

        public void Reset() => this.stringBuilder.Clear();
    }

    private sealed class GarageUserNameDefaultParameter : ParameterDefault
    {
        public override string GetDefaultValue() => $"GK{GetHexString(12)}";

        public override void WriteToManifest(Publishing.ManifestPublishingContext context)
        {
        }
    }

    private sealed class GaragePasswordDefaultParameter : ParameterDefault
    {
        public override string GetDefaultValue() => GetHexString(32);

        public override void WriteToManifest(Publishing.ManifestPublishingContext context)
        {
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "ClassNeverInstantiated.Local", Justification = "This is used through DI")]
    private sealed class GarageEventSubscriber(
        IServiceProvider services,
        ResourceNotificationService resourceNotificationService,
        ResourceLoggerService resourceLoggerService) : Lifecycle.IDistributedApplicationEventingSubscriber, IAsyncDisposable
    {
        private readonly CancellationTokenSource shutdownCts = new();
        private Task? setupGarage;

        public Task SubscribeAsync(Eventing.IDistributedApplicationEventing eventing, DistributedApplicationExecutionContext executionContext, CancellationToken cancellationToken)
        {
            eventing.Subscribe<BeforeStartEvent>((_, _) =>
            {
                this.setupGarage = Temp(resourceNotificationService, resourceLoggerService, services, this.shutdownCts.Token);
                return Task.CompletedTask;
            });

            return Task.CompletedTask;

            static async Task Temp(ResourceNotificationService resourceNotificationService, ResourceLoggerService loggerService, IServiceProvider services, CancellationToken cancellationToken)
            {
                Dictionary<IResource, string?> states = [];
                await foreach (var notification in resourceNotificationService.WatchAsync(cancellationToken).ConfigureAwait(false))
                {
                    if (notification.Resource is not GarageServerResource garage)
                    {
                        continue;
                    }

                    states.TryAdd(notification.Resource, notification.Snapshot.State?.Text);

                    if (string.Equals(notification.Snapshot.State?.Text, KnownResourceStates.Running, StringComparison.Ordinal)
                        && !string.Equals(states[notification.Resource], KnownResourceStates.Running, StringComparison.Ordinal))
                    {
                        // we're transitioning from something else to running
                        await Setup(garage, loggerService, services, cancellationToken).ConfigureAwait(false);
                    }

                    states[notification.Resource] = notification.Snapshot.State?.Text;
                }
            }

            static async Task Setup(IResource resource, ResourceLoggerService loggerService, IServiceProvider services, CancellationToken cancellationToken)
            {
                var logger = loggerService.GetLogger(resource);
                var containerRuntime = await ContainerRuntime.GetNameAsync(services, cancellationToken).ConfigureAwait(false);
                await SetupLayout(resource, logger, containerRuntime, cancellationToken).ConfigureAwait(false);
                await AddUsers(resource, logger, containerRuntime, cancellationToken).ConfigureAwait(false);

                static async Task SetupLayout(IResource resource, ILogger logger, string containerRuntime, CancellationToken cancellationToken)
                {
                    // get the node id
                    var echoLogger = new EchoLogger(logger);
                    await resource.ExecAsync(containerRuntime, [GarageExecutable, "node", "id", "--quiet"], echoLogger, cancellationToken).ConfigureAwait(false);
                    var nodeId = GetNodeId(echoLogger);

                    await resource.ExecAsync(containerRuntime, [GarageExecutable, "layout", "assign", "--zone", "aspire", "--capacity", "1GB", nodeId], logger, cancellationToken).ConfigureAwait(false);
                    await resource.ExecAsync(containerRuntime, [GarageExecutable, "layout", "apply", "--version", "1"], logger, cancellationToken).ConfigureAwait(false);

                    static string GetNodeId(EchoLogger echoLogger)
                    {
                        var split = echoLogger.ToString().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
                        echoLogger.Reset();
                        var nodeString = split[^1];
                        var position = nodeString.IndexOf('@', StringComparison.Ordinal);
                        return nodeString[..position];
                    }
                }

                static async Task AddUsers(IResource resource, ILogger logger, string containerRuntime, CancellationToken cancellationToken)
                {
                    if (resource.TryGetAnnotationsOfType<AWSProfileAnnotation>(out var profiles))
                    {
                        // create the profiles
                        foreach (var profile in profiles.Select(x => x.Profile))
                        {
                            await resource.ExecAsync(containerRuntime, [GarageExecutable, "key", "import", "-n", profile.Name, "--yes", profile.AccessKeyId, profile.SecretAccessKey], logger, cancellationToken).ConfigureAwait(false);
                            await resource.ExecAsync(containerRuntime, [GarageExecutable, "key", "allow", "--create-bucket", profile.Name], logger, cancellationToken).ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        // create a key
                        await resource.ExecAsync(containerRuntime, [GarageExecutable, "key", "create", "default-key"], logger, cancellationToken).ConfigureAwait(false);
                        await resource.ExecAsync(containerRuntime, [GarageExecutable, "key", "allow", "--create-bucket", "default-key"], logger, cancellationToken).ConfigureAwait(false);
                    }
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            await this.shutdownCts.CancelAsync().ConfigureAwait(false);
            if (this.setupGarage is { } task)
            {
                try
                {
                    await task.ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    // we have asked to be canceled
                }
            }

            this.shutdownCts.Dispose();
        }
    }
}