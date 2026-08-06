// -----------------------------------------------------------------------
// <copyright file="MiniStackBuilderExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting;

using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

/// <summary>
/// Provides extensions for <c>ministack</c>.
/// </summary>
public static partial class MiniStackBuilderExtensions
{
#pragma warning disable S5443 // Temporary files should not be created in publicly writable directories
    private const string StateLocation = "/tmp/ministack-state";
    private const string DataLocation = "/tmp/ministack-data";
#pragma warning restore S5443 // Temporary files should not be created in publicly writable directories

    /// <summary>
    /// Adds a named volume for the state folder to a MiniStack container resource.
    /// </summary>
    /// <param name="builder">The resource builder.</param>
    /// <param name="name">The name of the volume. Defaults to an auto-generated name based on the application and resource names.</param>
    /// <param name="isReadOnly">A flag that indicates if this is a read-only volume.</param>
    /// <returns>The <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<MiniStackServerResource> WithStateVolume(this IResourceBuilder<MiniStackServerResource> builder, string? name = null, bool isReadOnly = false)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder
            .WithVolume(name ?? VolumeNameGenerator.Generate(builder, "state"), StateLocation, isReadOnly)
            .WithEnvironment("PERSIST_STATE", "1");
    }

    /// <summary>
    /// Adds a bind mount for the state folder to a MiniStack container resource.
    /// </summary>
    /// <param name="builder">The resource builder.</param>
    /// <param name="source">The source directory on the host to mount into the container.</param>
    /// <param name="isReadOnly">A flag that indicates if this is a read-only mount.</param>
    /// <returns>The <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<MiniStackServerResource> WithStateBindMount(this IResourceBuilder<MiniStackServerResource> builder, string source, bool isReadOnly = false)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(source);

        return builder
            .WithBindMount(source, StateLocation, isReadOnly)
            .WithEnvironment("PERSIST_STATE", "1");
    }

    /// <summary>
    /// Adds a named volume for the data folder to a MiniStack container resource.
    /// </summary>
    /// <param name="builder">The resource builder.</param>
    /// <param name="name">The name of the volume. Defaults to an auto-generated name based on the application and resource names.</param>
    /// <param name="isReadOnly">A flag that indicates if this is a read-only volume.</param>
    /// <returns>The <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<MiniStackServerResource> WithDataVolume(this IResourceBuilder<MiniStackServerResource> builder, string? name = null, bool isReadOnly = false)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder
            .WithVolume(name ?? VolumeNameGenerator.Generate(builder, "data"), DataLocation, isReadOnly)
            .WithEnvironment("PERSIST_STATE", "1")
            .WithEnvironment("S3_PERSIST", "1");
    }

    /// <summary>
    /// Adds a bind mount for the data folder to a MiniStack container resource.
    /// </summary>
    /// <param name="builder">The resource builder.</param>
    /// <param name="source">The source directory on the host to mount into the container.</param>
    /// <param name="isReadOnly">A flag that indicates if this is a read-only mount.</param>
    /// <returns>The <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<MiniStackServerResource> WithDataBindMount(this IResourceBuilder<MiniStackServerResource> builder, string source, bool isReadOnly = false)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(source);

        return builder
            .WithBindMount(source, DataLocation, isReadOnly)
            .WithEnvironment("PERSIST_STATE", "1")
            .WithEnvironment("S3_PERSIST", "1");
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
    public static IResourceBuilder<TDestination> WithReference<TDestination>(this IResourceBuilder<TDestination> builder, IResourceBuilder<MiniStackServerResource> source, string? endpointName = default)
        where TDestination : IResourceWithEnvironment
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(source);

        if (source is IResourceBuilder<IResourceWithServiceDiscovery> serviceDiscovery)
        {
            _ = builder.WithReference(serviceDiscovery);
        }

        _ = builder.WithEnvironment(context =>
        {
            // set the end point
            context.EnvironmentVariables[Amazon.Util.EnvironmentVariables.GLOBAL_ENDPOINT_ENVIRONMENT_VARIABLE] = GetEndpoint(source.Resource, endpointName);

            // disable the AWS EC2 credentials lookup, as we are local
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

        static EndpointReference GetEndpoint(IResourceWithEndpoints resource, string? endpointName)
        {
            return endpointName is null ? resource.GetEndpoints().First() : resource.GetEndpoint(endpointName);
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
    public static IResourceBuilder<MiniStackServerResource> AddMiniStack(this IDistributedApplicationBuilder builder, string name, int? port = null, Amazon.RegionEndpoint? regionEndPoint = null, MiniStackServices? services = default) => AddMiniStack(builder, name, port, regionEndPoint?.SystemName, services);

    /// <summary>
    /// Adds a Local-Stack container to the application.
    /// </summary>
    /// <param name="builder">The <see cref="IDistributedApplicationBuilder"/>.</param>
    /// <param name="name">The name of the resource.</param>
    /// <param name="port">The port.</param>
    /// <param name="region">The region.</param>
    /// <param name="services">The requested services.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<MiniStackServerResource> AddMiniStack(this IDistributedApplicationBuilder builder, string name, int? port = null, string? region = null, MiniStackServices? services = default)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(name);

        var miniStack = new MiniStackServerResource(name, region ?? "us-east-1") { Services = services };

        IServiceProvider? serviceProvider = default;

        var resourceBuilder = builder.AddResource(miniStack)
            .WithImage(MiniStack.MiniStackContainerImageTags.Image, MiniStack.MiniStackContainerImageTags.Tag)
            .WithImageRegistry(MiniStack.MiniStackContainerImageTags.Registry)
            .WithHttpEndpoint(port, targetPort: 4566)
            .WithEnvironment(context =>
            {
                if (context.ExecutionContext.IsRunMode)
                {
                    context.EnvironmentVariables["LOG_LEVEL"] = "DEBUG";
                }
            })
            .WithEnvironment(context =>
            {
                if (miniStack.Services != default)
                {
                    context.EnvironmentVariables["SERVICES"] = string.Join(',', miniStack.GetServiceNames());
                }

                if (region is not null)
                {
                    context.EnvironmentVariables["MINISTACK_REGION"] = region;
                }
            })
            .WithDockerSock(() => serviceProvider ?? throw new InvalidOperationException())
            .PublishAsContainer();

        AddHealthCheck(resourceBuilder, Uri.UriSchemeHttp, () => resourceBuilder.GetEndpoint(Uri.UriSchemeHttp), services);

        // get the services from the event.
        builder.Eventing.Subscribe<BeforeStartEvent>((evt, ct) =>
        {
            ct.ThrowIfCancellationRequested();
            serviceProvider = evt.Services;
            return Task.CompletedTask;
        });

        AddResetCommand(resourceBuilder);

        return resourceBuilder;

        static void AddHealthCheck(IResourceBuilder<MiniStackServerResource> builder, string scheme, Func<EndpointReference> endpointSelector, MiniStackServices? services)
        {
            var endpoint = endpointSelector() ?? throw new DistributedApplicationException($"Could not create health check for resource '{builder.Resource.Name}' as the endpoint selector returned null.");

            var endpointName = endpoint.EndpointName;

            if (!string.Equals(endpoint.Scheme, scheme, StringComparison.Ordinal))
            {
                throw new DistributedApplicationException($"The endpoint '{endpointName}' on resource '{builder.Resource.Name}' was not using the '{scheme}' scheme.");
            }

            var healthCheckKey = $"{builder.Resource.Name}_{endpointName}_check";

            _ = builder.ApplicationBuilder.Services.AddLogging(configure =>
            {
                // The MiniStackHealthCheck makes use of http client factory.
                _ = configure.AddFilter("System.Net.Http.HttpClient." + healthCheckKey + ".LogicalHandler", LogLevel.None);
                _ = configure.AddFilter("System.Net.Http.HttpClient." + healthCheckKey + ".ClientHandler", LogLevel.None);
            });

            _ = builder.ApplicationBuilder.Services.AddHttpClient(healthCheckKey);

            _ = builder.ApplicationBuilder.Services
                .AddHealthChecks()
                .Add(new(
                    healthCheckKey,
                    serviceProvider => new MiniStack.MiniStackHealthCheck(() => serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient(healthCheckKey)) { Endpoint = endpoint, Services = services },
                    failureStatus: null,
                    tags: null));

            _ = builder.WithHealthCheck(healthCheckKey);
        }

        static void AddResetCommand(IResourceBuilder<MiniStackServerResource> builder)
        {
            builder.WithHttpCommand("/_ministack/reset", "Reset", commandOptions: new HttpCommandOptions { IconName = "ArrowReset", IconVariant = IconVariant.Filled });
        }
    }

    /// <summary>
    /// Adds the SQS queue to the resource.
    /// </summary>
    /// <typeparam name="TResource">The type of MiniStack resource.</typeparam>
    /// <param name="builder">The input builder.</param>
    /// <param name="profile">The optional profile.</param>
    /// <returns>The builder for chaining.</returns>
    public static IResourceBuilder<TResource> WithSqsQueue<TResource>(this IResourceBuilder<TResource> builder, string? profile = default)
        where TResource : MiniStackServerResource
    {
        string? queueName = default;

        // add SQS to the host
        _ = builder.ApplicationBuilder.AddAmazonSqs(builder);

        // add the queue
        var parameter = builder.WithQueue(() => queueName!);

        builder.ApplicationBuilder.Eventing.Subscribe<ResourceReadyEvent>(builder.Resource, async (evt, cancellationToken) =>
        {
            var notificationService = evt.Services.GetService<ResourceNotificationService>();
            if (notificationService is not null)
            {
                await notificationService
                    .PublishUpdateAsync(
                        parameter.Resource,
                        s => s with { State = new(KnownResourceStates.Waiting, KnownResourceStateStyles.Info) })
                    .ConfigureAwait(false);
            }
#pragma warning disable S3011
            var propertyInfo = parameter.Resource.GetType().GetProperty("WaitForValueTcs", BindingFlags.Instance | BindingFlags.NonPublic);
#pragma warning restore S3011

            // get the property
            var taskCompletionSource = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
            propertyInfo?.SetValue(parameter.Resource, taskCompletionSource);

            var client = evt.Services.GetRequiredKeyedAwsService<Amazon.SQS.IAmazonSQS>(evt.Resource.Name, profile);

            // ensure the queue exists
            var (urn, _) = await GetQueueArnAndUrlAsync(client, parameter.Resource.Name, cancellationToken).ConfigureAwait(false);

            queueName = urn;

            // set the parameter to success
            if (taskCompletionSource.TrySetResult(queueName) && notificationService is not null)
            {
                // update the parameter here
                await notificationService
                    .PublishUpdateAsync(
                        parameter.Resource,
                        s => s with
                        {
                            State = new(KnownResourceStates.Finished, KnownResourceStateStyles.Success),
                            Properties = [new(CustomResourceKnownProperties.Source, queueName)],
                        })
                    .ConfigureAwait(false);
            }

            static async Task<(string QueueArn, string QueueUrl)> GetQueueArnAndUrlAsync(Amazon.SQS.IAmazonSQS sqs, string queueName, CancellationToken cancellationToken)
            {
                if (await GetQueueUrlAsync(sqs, queueName, cancellationToken).ConfigureAwait(false) is not { } queueUrl)
                {
                    // the queue does not exist, so create it.
                    var createQueueResponse = await sqs.CreateQueueAsync(queueName, cancellationToken).ConfigureAwait(false);
                    queueUrl = createQueueResponse.QueueUrl;
                }

                var getQueueAttributesResponse = await sqs
                    .GetQueueAttributesAsync(
                        new() { QueueUrl = queueUrl, AttributeNames = [Amazon.SQS.Util.SQSConstants.ATTRIBUTE_QUEUE_ARN], },
                        cancellationToken)
                    .ConfigureAwait(false);

                return (getQueueAttributesResponse.QueueARN, queueUrl);

                static async Task<string?> GetQueueUrlAsync(Amazon.SQS.IAmazonSQS sqs, string queueName, CancellationToken cancellationToken)
                {
                    try
                    {
                        var response = await sqs.GetQueueUrlAsync(queueName, cancellationToken).ConfigureAwait(false);
                        return response.QueueUrl;
                    }
                    catch (Amazon.SQS.Model.QueueDoesNotExistException)
                    {
                        return default;
                    }
                }
            }
        });

        return builder;
    }

    /// <summary>
    /// Ensures that the specified bucket exists.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="bucketName">The bucket name.</param>
    /// <param name="eventTypes">The event types.</param>
    /// <returns>The resource builder.</returns>
    public static IResourceBuilder<MiniStackServerResource> EnsureBucket(this IResourceBuilder<MiniStackServerResource> builder, string bucketName, params IEnumerable<Amazon.S3.EventType> eventTypes)
    {
        _ = builder.ApplicationBuilder.AddAmazonS3(builder);
        if (builder is IResourceBuilder<IResource> resourceBuilder)
        {
            resourceBuilder.EnsureBucket(bucketName, EnsureBucketEventPolicy, eventTypes);
        }

        return builder;
    }

    /// <summary>
    /// Ensures that the specified bucket exists.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="bucketName">The bucket name.</param>
    /// <param name="profile">The profile to use.</param>
    /// <param name="eventTypes">The event types.</param>
    /// <returns>The resource builder.</returns>
    public static IResourceBuilder<MiniStackServerResource> EnsureBucket(this IResourceBuilder<MiniStackServerResource> builder, string bucketName, string profile, params IEnumerable<Amazon.S3.EventType> eventTypes)
    {
        _ = builder.ApplicationBuilder.AddAmazonS3(builder);
        if (builder is IResourceBuilder<IResource> resourceBuilder)
        {
            resourceBuilder.EnsureBucket(bucketName, profile, EnsureBucketEventPolicy, eventTypes);
        }

        return builder;
    }

    /// <summary>
    /// Ensures that the specified bucket exists.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="bucketName">The bucket name.</param>
    /// <param name="queue">The optional queue.</param>
    /// <param name="eventTypes">The event types.</param>
    /// <returns>The resource builder.</returns>
    public static IResourceBuilder<MiniStackServerResource> EnsureBucket(this IResourceBuilder<MiniStackServerResource> builder, string bucketName, IResourceBuilder<ParameterResource>? queue, params IEnumerable<Amazon.S3.EventType> eventTypes)
    {
        _ = builder.ApplicationBuilder.AddAmazonS3(builder);
        if (builder is IResourceBuilder<IResource> resourceBuilder)
        {
            resourceBuilder.EnsureBucket(bucketName, queue, EnsureBucketEventPolicy, eventTypes);
        }

        return builder;
    }

    /// <summary>
    /// Ensures that the specified bucket exists.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="bucketName">The bucket name.</param>
    /// <param name="profile">The profile to use.</param>
    /// <param name="queue">The optional queue.</param>
    /// <param name="eventTypes">The event types.</param>
    /// <returns>The resource builder.</returns>
    public static IResourceBuilder<MiniStackServerResource> EnsureBucket(this IResourceBuilder<MiniStackServerResource> builder, string bucketName, string? profile, IResourceBuilder<ParameterResource>? queue, params IEnumerable<Amazon.S3.EventType> eventTypes)
    {
        _ = builder.ApplicationBuilder.AddAmazonS3(builder);
        if (builder is IResourceBuilder<IResource> resourceBuilder)
        {
            resourceBuilder.EnsureBucket(bucketName, profile, queue, EnsureBucketEventPolicy, eventTypes);
        }

        return builder;
    }

    /// <summary>
    /// Adds a folder to the MiniStack server resource and executes commands for mirroring into a bucket.
    /// </summary>
    /// <typeparam name="T">The type of resource.</typeparam>
    /// <param name="builder">The builder.</param>
    /// <param name="source">The source path to bind to container resource.</param>
    /// <param name="bucketName">The name of the bucket.</param>
    /// <param name="isReadOnly">Whether the source is read-only.</param>
    /// <returns>The resource builder.</returns>
    public static IResourceBuilder<T> WithMirror<T>(this IResourceBuilder<T> builder, string source, string bucketName, bool isReadOnly = true)
        where T : MiniStackServerResource
    {
        // ensure we have a lock annotation
        if (!builder.Resource.HasAnnotationOfType<BucketMirrorLockAnnotation>())
        {
            _ = builder.WithAnnotation(new BucketMirrorLockAnnotation());
        }

        // check to see if we've already added the with bucket mirror handler
        if (builder.Resource.TryGetLastAnnotation<BucketMirrorLockAnnotation>(out var lockAnnotation)
            && Interlocked.Exchange(ref lockAnnotation.Check, 1) is 0)
        {
            _ = builder.ApplicationBuilder.Eventing.Subscribe<ResourceReadyEvent>(builder.Resource, Mirror);
        }

        return builder.WithAnnotation(new BucketMirrorAnnotation { Directory = source, BucketName = bucketName, ReadOnly = isReadOnly });

        static async Task Mirror(ResourceReadyEvent e, CancellationToken cancellationToken)
        {
            if (e.Resource.TryGetAnnotationsOfType<BucketMirrorAnnotation>(out var annotations))
            {
                var logger = e.Services.GetRequiredService<ResourceLoggerService>().GetLogger(e.Resource);
                Amazon.S3.IAmazonS3? client = default;
                foreach (var annotation in annotations)
                {
                    var bucketName = annotation.BucketName;

                    client ??= e.Services.GetRequiredKeyedAwsService<Amazon.S3.IAmazonS3>(e.Resource.Name);

                    // see if the directory exists
                    if (!Directory.Exists(annotation.Directory))
                    {
                        LogDirectoryNotFound(logger, annotation.Directory);

                        if (await Amazon.S3.Util.AmazonS3Util.DoesS3BucketExistV2Async(client, bucketName).ConfigureAwait(false))
                        {
                            LogDeletingBucket(logger, bucketName);
                            await client.DeleteBucketAsync(new Amazon.S3.Model.DeleteBucketRequest { BucketName = bucketName, BucketRegion = Amazon.S3.S3Region.FindValue(client.Config.AuthenticationRegion) }, cancellationToken).ConfigureAwait(false);
                        }

                        continue;
                    }

                    if (!await Amazon.S3.Util.AmazonS3Util.DoesS3BucketExistV2Async(client, bucketName).ConfigureAwait(false))
                    {
                        LogCreateBucket(logger, bucketName);
                        await client.PutBucketAsync(new Amazon.S3.Model.PutBucketRequest { BucketName = bucketName, BucketRegionName = client.Config.AuthenticationRegion }, cancellationToken).ConfigureAwait(false);
                    }

                    var transferUtility = new Amazon.S3.Transfer.TransferUtility(client);
                    var directory = Path.GetFullPath(annotation.Directory);
                    var directoryLength = directory.Length + 1;

                    foreach (var filePath in Directory.GetFiles(directory, "*", SearchOption.AllDirectories))
                    {
                        var key = filePath[directoryLength..].Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                        if (await FileNeedsUploadAsync(client, bucketName, key, filePath, cancellationToken).ConfigureAwait(false))
                        {
                            LogUploadingFile(logger, key, bucketName);
                            await transferUtility
                                .UploadAsync(
                                    new()
                                    {
                                        BucketName = bucketName,
                                        FilePath = filePath,
                                        Key = key,
                                    },
                                    cancellationToken)
                                .ConfigureAwait(false);
                        }

                        static async Task<bool> FileNeedsUploadAsync(Amazon.S3.IAmazonS3 client, string bucketName, string key, string filePath, CancellationToken cancellationToken)
                        {
                            try
                            {
                                if (await client.GetObjectMetadataAsync(bucketName, key, cancellationToken).ConfigureAwait(false) is { LastModified: { } lastModified })
                                {
                                    return File.GetLastWriteTimeUtc(filePath) > lastModified.ToUniversalTime();
                                }

                                return true;
                            }
                            catch (Amazon.S3.AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                            {
                                // File does not exist in S3
                                return true;
                            }
                        }
                    }

                    var listResponse = await client.ListObjectsV2Async(new() { BucketName = bucketName }, cancellationToken).ConfigureAwait(false);

                    if (listResponse.S3Objects is null or { Count: 0 })
                    {
                        // there are no objects in the bucket, so nothing to check
                        continue;
                    }

                    foreach (var item in listResponse.S3Objects)
                    {
                        var localPath = Path.Combine(directory, item.Key.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar));
                        if (!File.Exists(localPath))
                        {
                            LogDeletingFile(logger, item.Key, item.BucketName);
                            await client.DeleteObjectAsync(item.BucketName, item.Key, cancellationToken).ConfigureAwait(false);
                        }
                        else if (!annotation.ReadOnly
                            && item is { LastModified: { } lastModified }
                            && File.GetLastWriteTimeUtc(localPath) < lastModified.ToUniversalTime())
                        {
                            // download this
                            LogDownloadingFile(logger, item.Key, item.BucketName);
                            await transferUtility.DownloadAsync(localPath, item.BucketName, item.Key, cancellationToken).ConfigureAwait(false);

                            // ensure we update the last modified time
                            File.SetLastWriteTimeUtc(localPath, lastModified);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Adds a StackPort container for MiniStack to the application model.
    /// </summary>
    /// <remarks>
    /// This version of the package defaults to the <inheritdoc cref="MiniStack.MiniStackContainerImageTags.StackPortTag"/> tag of the <inheritdoc cref="MiniStack.MiniStackContainerImageTags.StackPortImage"/> container image.
    /// </remarks>
    /// <typeparam name="T">The MiniStack server resource type.</typeparam>
    /// <param name="builder">The MiniStack server resource builder.</param>
    /// <param name="configureContainer">Callback to configure StackPort container resource.</param>
    /// <param name="containerName">The name of the container (Optional).</param>
    /// <param name="profile">The optional AWS profile to use for credentials.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    /// <ats-returns>The resource builder.</ats-returns>
    public static IResourceBuilder<T> WithStackPort<T>(this IResourceBuilder<T> builder, Action<IResourceBuilder<MiniStack.StackPortContainerResource>>? configureContainer = null, string? containerName = null, Aspire.Hosting.AWS.AWSProfile? profile = default)
        where T : MiniStackServerResource
    {
        if (builder.ApplicationBuilder.Resources.OfType<MiniStack.StackPortContainerResource>().SingleOrDefault() is { } existingStackPortResource)
        {
            var builderForExistingResource = builder.ApplicationBuilder.CreateResourceBuilder(existingStackPortResource);
            configureContainer?.Invoke(builderForExistingResource);
            return builder;
        }

        containerName ??= "stackport";

        var stackPortContainer = new MiniStack.StackPortContainerResource(containerName);
        var stackPortContainerBuilder = builder.ApplicationBuilder.AddResource(stackPortContainer)
            .WithImage(MiniStack.MiniStackContainerImageTags.StackPortImage, MiniStack.MiniStackContainerImageTags.StackPortTag)
            .WithImageRegistry(MiniStack.MiniStackContainerImageTags.StackPortRegistry)
            .WithEnvironment(ctx =>
            {
                ctx.EnvironmentVariables[Amazon.Util.EnvironmentVariables.GLOBAL_ENDPOINT_ENVIRONMENT_VARIABLE] = builder.Resource.GetEndpoint(Uri.UriSchemeHttp);
                ctx.EnvironmentVariables[Amazon.Runtime.EnvironmentVariableAWSRegion.ENVIRONMENT_VARIABLE_REGION] = builder.Resource.Region;
                ctx.EnvironmentVariables[Amazon.Runtime.EnvironmentVariablesAWSCredentials.ENVIRONMENT_VARIABLE_ACCESSKEY] = profile?.AccessKeyId ?? (object)"ministack";
                ctx.EnvironmentVariables[Amazon.Runtime.EnvironmentVariablesAWSCredentials.ENVIRONMENT_VARIABLE_SECRETKEY] = profile?.SecretAccessKey ?? (object)"ministack";
                if (profile?.SessionToken is { } sessionToken)
                {
                    ctx.EnvironmentVariables[Amazon.Runtime.EnvironmentVariablesAWSCredentials.ENVIRONMENT_VARIABLE_SESSION_TOKEN] = sessionToken;
                }
            })
            .WithHttpEndpoint(targetPort: 8080)
            .WithHttpHealthCheck(path: "/api/health")
            .ExcludeFromManifest();

        configureContainer?.Invoke(stackPortContainerBuilder);

        stackPortContainerBuilder.WaitFor(builder);
        stackPortContainerBuilder.WithRelationship(builder.Resource, "Manager");

        return builder;
    }

    /// <summary>
    /// Adds Amazon S3 to the host.
    /// </summary>
    /// <typeparam name="TResource">The type of MiniStack resource.</typeparam>
    /// <param name="builder">The input builder.</param>
    /// <param name="resourceBuilder">The MiniStack resource builder.</param>
    /// <param name="configuration">The AWS configuration.</param>
    /// <returns>The builder for chaining.</returns>
    public static IDistributedApplicationBuilder AddAmazonS3<TResource>(this IDistributedApplicationBuilder builder, IResourceBuilder<TResource> resourceBuilder, AWS.IAWSSDKConfig? configuration = default)
        where TResource : MiniStackServerResource
    {
        return configuration is null
            ? builder.AddAmazonS3(resourceBuilder, Uri.UriSchemeHttp, UpdateConfigurationCore)
            : builder.AddAmazonS3(resourceBuilder, configuration, Uri.UriSchemeHttp, UpdateConfigurationCore);

        void UpdateConfigurationCore(IConfigurationBuilder config)
        {
            UpdateConfiguration(resourceBuilder.Resource, config);
        }
    }

    /// <summary>
    /// Adds Amazon S3 to the host.
    /// </summary>
    /// <typeparam name="TResource">The type of MiniStack resource.</typeparam>
    /// <param name="builder">The input builder.</param>
    /// <param name="resource">The MiniStack resource.</param>
    /// <param name="configuration">The AWS configuration.</param>
    /// <returns>The builder for chaining.</returns>
    public static IDistributedApplicationBuilder AddAmazonS3<TResource>(this IDistributedApplicationBuilder builder, TResource resource, AWS.IAWSSDKConfig? configuration = default)
        where TResource : MiniStackServerResource
    {
        return configuration is null
            ? builder.AddAmazonS3(resource, Uri.UriSchemeHttp, UpdateConfigurationCore)
            : builder.AddAmazonS3(resource, configuration, Uri.UriSchemeHttp, UpdateConfigurationCore);

        void UpdateConfigurationCore(IConfigurationBuilder config)
        {
            UpdateConfiguration(resource, config);
        }
    }

    /// <summary>
    /// Adds Amazon SQS to the host.
    /// </summary>
    /// <typeparam name="TResource">The type of MiniStack resource.</typeparam>
    /// <param name="builder">The input builder.</param>
    /// <param name="resourceBuilder">The MiniStack resource builder.</param>
    /// <param name="configuration">The AWS configuration.</param>
    /// <returns>The builder for chaining.</returns>
    public static IDistributedApplicationBuilder AddAmazonSqs<TResource>(this IDistributedApplicationBuilder builder, IResourceBuilder<TResource> resourceBuilder, AWS.IAWSSDKConfig? configuration = default)
        where TResource : MiniStackServerResource => AddAmazonSqs(builder, resourceBuilder.Resource, Uri.UriSchemeHttp, configuration);

    /// <summary>
    /// Adds Amazon SQS to the host.
    /// </summary>
    /// <typeparam name="TResource">The type of MiniStack resource.</typeparam>
    /// <param name="builder">The input builder.</param>
    /// <param name="resource">The MiniStack resource.</param>
    /// <param name="configuration">The AWS configuration.</param>
    /// <returns>The builder for chaining.</returns>
    public static IDistributedApplicationBuilder AddAmazonSqs<TResource>(this IDistributedApplicationBuilder builder, TResource resource, AWS.IAWSSDKConfig? configuration = default)
        where TResource : MiniStackServerResource => AddAmazonSqs(builder, resource, Uri.UriSchemeHttp, configuration);

    private static IDistributedApplicationBuilder AddAmazonSqs(
        IDistributedApplicationBuilder builder,
        IResourceWithEndpoints resource,
        string endpointName,
        AWS.IAWSSDKConfig? configuration = default,
        Action<IConfigurationBuilder>? configureConfiguration = default)
    {
        // ensure we have a lock annotation
        if (!resource.HasAnnotationOfType<AddAmazonSqsLockAnnotation>())
        {
            resource.Annotations.Add(new AddAmazonSqsLockAnnotation());
        }

        // check to see if we've already added the AddAmazonSqs handler
        if (resource.TryGetLastAnnotation<AddAmazonSqsLockAnnotation>(out var lockAnnotation)
            && Interlocked.Exchange(ref lockAnnotation.Check, 1) is 0)
        {
            _ = builder.Services.TryAddKeyedAWSService<Amazon.SQS.IAmazonSQS>(resource.Name);

            _ = builder.Eventing.Subscribe<ResourceEndpointsAllocatedEvent>(resource, (_, _) =>
            {
                Environment.SetEnvironmentVariable($"{Amazon.Util.EnvironmentVariables.SERVICE_SPECIFIC_ENDPOINT_ENVIRONMENT_VARIABLE_PREFIX}{Amazon.SQS.AmazonSQSConfig.ServiceId}", resource.GetEndpoint(endpointName).Url, EnvironmentVariableTarget.Process);
                Environment.SetEnvironmentVariable(Amazon.Util.EC2InstanceMetadata.AWS_EC2_METADATA_DISABLED, bool.TrueString, EnvironmentVariableTarget.Process);

                RefreshEnvironmentVariables(builder.Configuration);

                configureConfiguration?.Invoke(builder.Configuration);

                return Task.CompletedTask;

                static void RefreshEnvironmentVariables(IConfigurationRoot configurationRoot)
                {
                    foreach (var provider in configurationRoot.Providers.OfType<Microsoft.Extensions.Configuration.EnvironmentVariables.EnvironmentVariablesConfigurationProvider>())
                    {
                        provider.Load();
                    }
                }
            });
        }

        if (configuration is not null)
        {
            _ = builder.SetAWSConfig(configuration);
        }

        return builder;
    }

    private static async Task EnsureBucketEventPolicy(IResource resource, IServiceProvider services, string bucketName, string queueArn, string? profile)
    {
        var sqs = services.GetRequiredKeyedAwsService<Amazon.SQS.IAmazonSQS>(resource.Name, profile);

        // get the queue url
        var response = await sqs.GetQueueUrlAsync(Amazon.Arn.Parse(queueArn).Resource, CancellationToken.None).ConfigureAwait(false);

        await sqs.AuthorizeS3ToSendMessageAsync(response.QueueUrl, bucketName).ConfigureAwait(false);
    }

    private static void UpdateConfiguration(MiniStackServerResource resource, IConfigurationBuilder configuration)
    {
        if (configuration.Properties.ContainsKey($"AWS:{nameof(Amazon.S3.AmazonS3Config.ForcePathStyle)}"))
        {
            return;
        }

        var dictionary = new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            { $"AWS:{nameof(Amazon.S3.AmazonS3Config.ForcePathStyle)}", bool.TrueString },
            { $"AWS:{nameof(Amazon.S3.AmazonS3Config.AuthenticationRegion)}",  resource.Region },
            { $"AWS:{nameof(Amazon.S3.AmazonS3Config.UseAccelerateEndpoint)}", bool.FalseString },
        };

        _ = configuration.AddInMemoryCollection(dictionary);
    }

    private static IResourceBuilder<T> WithDockerSock<T>(this IResourceBuilder<T> builder, Func<IServiceProvider> servicesFactory)
        where T : ContainerResource
    {
        return builder.WithContainerRuntimeArgs(GetSock);

        async Task GetSock(ContainerRuntimeArgsCallbackContext callback)
        {
            if (await ContainerRuntime.GetRemoteSockAsync(servicesFactory(), callback.CancellationToken).ConfigureAwait(false) is { } sock)
            {
                callback.Args.Add("-v");
                callback.Args.Add($"{sock}:/var/run/docker.sock:ro");
            }
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Changing AWS profile to {Profile}")]
    private static partial void LogChangeProfile(ILogger logger, string profile);

    [LoggerMessage(Level = LogLevel.Information, Message = "Creating Bucket {BucketName}")]
    private static partial void LogCreateBucket(ILogger logger, string bucketName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Deleting Bucket {BucketName}")]
    private static partial void LogDeletingBucket(ILogger logger, string bucketName);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Directory not found: {Directory}")]
    private static partial void LogDirectoryNotFound(ILogger logger, string directory);

    [LoggerMessage(Level = LogLevel.Information, Message = "Transferring {Key} to {BucketName}")]
    private static partial void LogUploadingFile(ILogger logger, string key, string bucketName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Transferring {Key} from {BucketName}")]
    private static partial void LogDownloadingFile(ILogger logger, string key, string bucketName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Deleting {Key} from {BucketName}")]
    private static partial void LogDeletingFile(ILogger logger, string key, string bucketName);

    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "This is for locking")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "This is required")]
    private sealed class BucketMirrorLockAnnotation : IResourceAnnotation
    {
        public int Check;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "This is for locking")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "This is required")]
    private sealed class AddAmazonSqsLockAnnotation : IResourceAnnotation
    {
        public int Check;
    }
}