// -----------------------------------------------------------------------
// <copyright file="ResourceBuilderExtensions.cs" company="Altavec">
// Copyright (c) Altavec. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

/// <summary>
/// The resource builder extensions.
/// </summary>
public static partial class ResourceBuilderExtensions
{
    /// <summary>
    /// Adds amazon S3 to the mix.
    /// </summary>
    /// <typeparam name="TResource">The type of S3 resource.</typeparam>
    /// <param name="builder">The input builder.</param>
    /// <param name="resourceBuilder">The S3 resource builder.</param>
    /// <param name="configuration">The AWS configuration.</param>
    /// <param name="endpointName">The end point name.</param>
    /// <param name="configureConfiguration">The action to configure the configuration.</param>
    /// <returns>The builder for chaining.</returns>
    public static IDistributedApplicationBuilder AddAmazonS3<TResource>(
        this IDistributedApplicationBuilder builder,
        IResourceBuilder<TResource> resourceBuilder,
        AWS.IAWSSDKConfig configuration,
        string endpointName,
        Action<IConfigurationBuilder>? configureConfiguration = default)
        where TResource : IResourceWithEndpoints => AddAmazonS3(builder, () => resourceBuilder.GetEndpoint(endpointName), configuration, configureConfiguration);

    /// <summary>
    /// Adds amazon S3 to the mix.
    /// </summary>
    /// <typeparam name="TResource">The type of S3 resource.</typeparam>
    /// <param name="builder">The input builder.</param>
    /// <param name="resource">The S3 resource.</param>
    /// <param name="configuration">The AWS configuration.</param>
    /// <param name="endpointName">The end point name.</param>
    /// <param name="configureConfiguration">The action to configure the configuration.</param>
    /// <returns>The builder for chaining.</returns>
    public static IDistributedApplicationBuilder AddAmazonS3<TResource>(
        this IDistributedApplicationBuilder builder,
        TResource resource,
        AWS.IAWSSDKConfig configuration,
        string endpointName,
        Action<IConfigurationBuilder>? configureConfiguration = default)
        where TResource : IResourceWithEndpoints => AddAmazonS3(builder, () => resource.GetEndpoint(endpointName), configuration, configureConfiguration);

    /// <summary>
    /// Gets the queue name.
    /// </summary>
    /// <typeparam name="TResource">The type of S3 resource.</typeparam>
    /// <param name="builder">The builder.</param>
    /// <returns>The parameter builder.</returns>
    public static IResourceBuilder<ParameterResource> GetQueueName<TResource>(this IResourceBuilder<TResource> builder)
        where TResource : IResource
    {
        var name = GetQueueResourceName(builder);
        return builder.ApplicationBuilder.Resources.OfType<ParameterResource>().FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.Ordinal)) is { } parameter
            ? builder.ApplicationBuilder.CreateResourceBuilder(parameter)
            : throw new InvalidOperationException();
    }

    /// <summary>
    /// Tries to get the queue name parameter.
    /// </summary>
    /// <typeparam name="TResource">The type of resource.</typeparam>
    /// <param name="builder">The builder.</param>
    /// <param name="resource">The parameter resource.</param>
    /// <returns>The resource builder.</returns>
    public static bool TryGetQueueName<TResource>(this IResourceBuilder<TResource> builder, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out IResourceBuilder<ParameterResource>? resource)
        where TResource : IResource
    {
        var name = GetQueueResourceName(builder);
        if (builder.ApplicationBuilder.Resources.OfType<ParameterResource>().FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.Ordinal)) is { } parameter)
        {
            resource = builder.ApplicationBuilder.CreateResourceBuilder(parameter);
            return true;
        }

        resource = default;
        return false;
    }

    /// <summary>
    /// Ensures that the specified bucket exists.
    /// </summary>
    /// <typeparam name="TResource">The type of S3 resource.</typeparam>
    /// <param name="builder">The builder.</param>
    /// <param name="bucketName">The bucket name.</param>
    /// <param name="eventTypes">The event types.</param>
    /// <returns>The resource builder.</returns>
    public static IResourceBuilder<TResource> EnsureBucket<TResource>(this IResourceBuilder<TResource> builder, string bucketName, params IEnumerable<Amazon.S3.EventType> eventTypes)
        where TResource : IResource => EnsureBucket(builder, bucketName, TryGetQueueName(builder, out var queueName) ? queueName : null, eventTypes);

    /// <summary>
    /// Ensures that the specified bucket exists.
    /// </summary>
    /// <typeparam name="TResource">The type of S3 resource.</typeparam>
    /// <param name="builder">The builder.</param>
    /// <param name="bucketName">The bucket name.</param>
    /// <param name="queue">The optional queue.</param>
    /// <param name="eventTypes">The event types.</param>
    /// <returns>The resource builder.</returns>
    public static IResourceBuilder<TResource> EnsureBucket<TResource>(this IResourceBuilder<TResource> builder, string bucketName, IResourceBuilder<ParameterResource>? queue, params IEnumerable<Amazon.S3.EventType> eventTypes)
        where TResource : IResource
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
            _ = builder.ApplicationBuilder.Eventing.Subscribe<ResourceReadyEvent>(builder.Resource, async (evt, cancellationToken) =>
            {
                // ensure the bucket exists
                var rls = evt.Services.GetRequiredService<ResourceLoggerService>();
                var logger = rls.GetLogger(evt.Resource);
                var client = evt.Services.GetRequiredService<Amazon.S3.IAmazonS3>();

                foreach (var annotation in evt.Resource.Annotations.OfType<BucketAnnotation>())
                {
                    var (annotationBucketName, annotationQueue, annotationEventTypes) = annotation;

                    // ensure the bucket exists
                    LogCheckBucketExists(logger, annotationBucketName);
                    var bucketExists = await Amazon.S3.Util.AmazonS3Util.DoesS3BucketExistV2Async(client, annotationBucketName).ConfigureAwait(false);
                    if (!bucketExists)
                    {
                        LogCreatingBucket(logger, annotationBucketName);
                        try
                        {
                            await client.EnsureBucketExistsAsync(annotationBucketName).ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            LogFailedBucketCreation(logger, annotationBucketName, ex);
                            throw;
                        }
                    }

                    if (annotationQueue is null || annotationEventTypes is not { Count: not 0 })
                    {
                        continue;
                    }

                    var queueName = annotationQueue.Resource.Value;

                    // ensure the bucket notifications exist
                    LogCheckBucketNotificationExists(logger, annotationBucketName, queueName, annotationEventTypes);
                    var notifications = await client.GetBucketNotificationAsync(annotationBucketName, cancellationToken)
                        .ConfigureAwait(false);
                    if (notifications?.QueueConfigurations is { } queueConfigurations
                        && !queueConfigurations.TrueForAll(q =>
                            !q.Queue.Equals(queueName, StringComparison.Ordinal) ||
                            !q.Events.SequenceEqual(annotationEventTypes)))
                    {
                        continue;
                    }

                    LogCreatingBucketNotification(logger, annotationBucketName, queueName, annotationEventTypes);
                    var putBucketNotificationRequest = new Amazon.S3.Model.PutBucketNotificationRequest
                    {
                        BucketName = annotationBucketName,
                        QueueConfigurations =
                        [
                            new Amazon.S3.Model.QueueConfiguration
                            {
                                Queue = queueName,
                                Events = annotationEventTypes,
                            },
                        ],
                    };

                    try
                    {
                        _ = await client.PutBucketNotificationAsync(putBucketNotificationRequest, cancellationToken)
                            .ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        LogFailedBucketNotificationCreation(logger, annotationBucketName, queueName, annotationEventTypes, ex);
                        throw;
                    }
                }
            });
        }

        _ = builder.WithAnnotation(new BucketAnnotation(bucketName, queue, [.. eventTypes]));

        return builder;
    }

    /// <summary>
    /// Sets the queue name for the resource.
    /// </summary>
    /// <typeparam name="TResource">The resource.</typeparam>
    /// <param name="builder">The builder.</param>
    /// <param name="queueName">The queue name.</param>
    /// <returns>The resource builder.</returns>
    public static IResourceBuilder<ParameterResource> WithQueue<TResource>(this IResourceBuilder<TResource> builder, string queueName)
        where TResource : IResource
    {
        var name = GetQueueResourceName(builder);
        return builder.ApplicationBuilder.Resources.OfType<ParameterResource>().FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.Ordinal)) is { } parameter
            ? builder.ApplicationBuilder.CreateResourceBuilder(parameter)
            : builder.ApplicationBuilder.AddParameter(name, queueName, secret: false);
    }

    private static string GetQueueResourceName<TResource>(IResourceBuilder<TResource> builder)
        where TResource : IResource => $"{builder.Resource.Name}-queue";

    private static IDistributedApplicationBuilder AddAmazonS3(
        IDistributedApplicationBuilder builder,
        Func<EndpointReference> getEndpoint,
        AWS.IAWSSDKConfig configuration,
        Action<IConfigurationBuilder>? configureConfiguration = default)
    {
        _ = builder.Services.AddAWSService<Amazon.S3.IAmazonS3>();

        _ = builder.Eventing.Subscribe<AfterEndpointsAllocatedEvent>((_, _) =>
        {
            Environment.SetEnvironmentVariable("AWS_ENDPOINT_URL_S3", getEndpoint().Url, EnvironmentVariableTarget.Process);
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

        builder.SetAWSConfig(configuration);

        return builder;
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Checking to see if {BucketName} exists")]
    private static partial void LogCheckBucketExists(ILogger logger, string bucketName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Creating {BucketName}")]
    private static partial void LogCreatingBucket(ILogger logger, string bucketName);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to create {BucketName}")]
    private static partial void LogFailedBucketCreation(ILogger logger, string bucketName, Exception exception);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Checking to see if {BucketName} notification for {Queue} with {Events} exists")]
    private static partial void LogCheckBucketNotificationExists(ILogger logger, string bucketName, string? queue, IEnumerable<Amazon.S3.EventType> events);

    [LoggerMessage(Level = LogLevel.Information, Message = "Creating {BucketName} notification for {Queue} with {Events}")]
    private static partial void LogCreatingBucketNotification(ILogger logger, string bucketName, string queue, IEnumerable<Amazon.S3.EventType> events);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to create {BucketName} notification for {Queue} with {Events}")]
    private static partial void LogFailedBucketNotificationCreation(ILogger logger, string bucketName, string queue, IEnumerable<Amazon.S3.EventType> events, Exception exception);

    private sealed record BucketAnnotation(string BucketName, IResourceBuilder<ParameterResource>? Queue, List<Amazon.S3.EventType> EventTypes) : IResourceAnnotation;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "This is for locking")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "This is required")]
    private sealed class EnsureBucketLockAnnotation : IResourceAnnotation
    {
        public int Check;
    }
}