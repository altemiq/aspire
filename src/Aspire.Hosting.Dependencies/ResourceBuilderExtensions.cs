// -----------------------------------------------------------------------
// <copyright file="ResourceBuilderExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting;

using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Lifecycle;
using Microsoft.Extensions.Logging;
using Polly;

/// <content>
/// Extensions for waiting for dependencies.
/// </content>
public static partial class ResourceBuilderExtensions
{
    /// <summary>
    /// Injects service discovery information as environment variables from the project resource into the destination resource, using the source resource's name as the service name.
    /// Each endpoint defined on the project resource will be injected using the format "services__{sourceResourceName}__{endpointName}__{endpointIndex}={uriString}".
    /// </summary>
    /// <typeparam name="TDestination">The destination resource.</typeparam>
    /// <param name="builder">The resource where the service discovery information will be injected.</param>
    /// <param name="source">The resource from which to extract service discovery information.</param>
    /// <param name="wait">Set to <see langword="true"/> to wait for <paramref name="source"/> to be ready.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<TDestination> WithReference<TDestination>(this IResourceBuilder<TDestination> builder, IResourceBuilder<IResourceWithServiceDiscovery> source, bool wait)
        where TDestination : IResourceWithEnvironment
    {
        builder.WithReference(source);
        if (wait)
        {
            builder.WaitFor(source);
        }

        return builder;
    }

    /// <summary>
    /// Wait for a resource to be running before starting another resource.
    /// </summary>
    /// <typeparam name="T">The resource type.</typeparam>
    /// <param name="builder">The resource builder.</param>
    /// <param name="other">The resource to wait for.</param>
    /// <param name="states">The states to wait for.</param>
    /// <returns>A resource builder with the annotation configured.</returns>
    public static IResourceBuilder<T> WaitFor<T>(this IResourceBuilder<T> builder, IResourceBuilder<IResource> other, params string[] states)
        where T : IResource => WaitFor<T, IResource>(builder, other, states);

    /// <summary>
    /// Wait for a resource to be running before starting another resource.
    /// </summary>
    /// <typeparam name="T1">The input resource type.</typeparam>
    /// <typeparam name="T2">The type of resource to wait for.</typeparam>
    /// <param name="builder">The resource builder.</param>
    /// <param name="other">The resource to wait for.</param>
    /// <param name="states">The states to wait for.</param>
    /// <returns>A resource builder with the annotation configured.</returns>
    public static IResourceBuilder<T1> WaitFor<T1, T2>(this IResourceBuilder<T1> builder, IResourceBuilder<T2> other, params string[] states)
        where T1 : IResource
        where T2 : IResource
    {
        _ = builder.ApplicationBuilder.AddWaitForDependencies();
        return builder.WithAnnotation(new WaitOnAnnotation(other.Resource) { States = NullIfEmpty(states) });

        static TCollection? NullIfEmpty<TCollection>(TCollection value)
            where TCollection : System.Collections.ICollection
        {
            return value.Count is 0 ? default : value;
        }
    }

    /// <summary>
    /// Wait for a resource to run to completion before starting another resource.
    /// </summary>
    /// <typeparam name="T">The resource type.</typeparam>
    /// <param name="builder">The resource builder.</param>
    /// <param name="other">The resource to wait for.</param>
    /// <returns>A resource builder with the annotation configured.</returns>
    public static IResourceBuilder<T> WaitForCompletion<T>(this IResourceBuilder<T> builder, IResourceBuilder<IResource> other)
        where T : IResource => WaitFor<T, IResource>(builder, other);

    /// <summary>
    /// Wait for a resource to run to completion before starting another resource.
    /// </summary>
    /// <typeparam name="T1">The input resource type.</typeparam>
    /// <typeparam name="T2">The type of resource to wait for.</typeparam>
    /// <param name="builder">The resource builder.</param>
    /// <param name="other">The resource to wait for.</param>
    /// <returns>A resource builder with the annotation configured.</returns>
    public static IResourceBuilder<T1> WaitForCompletion<T1, T2>(this IResourceBuilder<T1> builder, IResourceBuilder<T2> other)
        where T1 : IResource
        where T2 : IResource
    {
        _ = builder.ApplicationBuilder.AddWaitForDependencies();
        return builder.WithAnnotation(new WaitOnAnnotation(other.Resource) { WaitUntilCompleted = true });
    }

    /// <summary>
    /// Adds a lifecycle hook that waits for all dependencies to be "running" before starting resources. If that resource
    /// has a health check, it will be executed before the resource is considered "running".
    /// </summary>
    /// <param name="builder">The <see cref="IDistributedApplicationBuilder" />.</param>
    private static IDistributedApplicationBuilder AddWaitForDependencies(this IDistributedApplicationBuilder builder)
    {
        builder.Services.TryAddLifecycleHook<WaitForDependenciesRunningHook>();
        return builder;
    }

    private sealed class WaitOnAnnotation(IResource resource) : IResourceAnnotation
    {
        public IResource Resource { get; } = resource;

        public string[]? States { get; init; }

        public bool WaitUntilCompleted { get; init; }
    }

    private sealed partial class WaitForDependenciesRunningHook(
        DistributedApplicationExecutionContext executionContext,
        ResourceNotificationService resourceNotificationService) :
        IDistributedApplicationLifecycleHook,
        IAsyncDisposable
    {
        private readonly CancellationTokenSource cancellationTokenSource = new();

        public async ValueTask DisposeAsync()
        {
            using (this.cancellationTokenSource)
            {
                await this.cancellationTokenSource.CancelAsync().ConfigureAwait(false);
            }
        }

        public Task BeforeStartAsync(DistributedApplicationModel appModel, CancellationToken cancellationToken = default)
        {
            // We don't need to execute any of this logic in publish mode
            if (executionContext.IsPublishMode)
            {
                return Task.CompletedTask;
            }

            // The global list of resources being waited on
            var waitingResources = new System.Collections.Concurrent.ConcurrentDictionary<IResource, System.Collections.Concurrent.ConcurrentDictionary<WaitOnAnnotation, TaskCompletionSource>>();

            // For each resource, add an environment callback that waits for dependencies to be running
            foreach (var resource in appModel.Resources)
            {
                if (resource.Annotations.OfType<WaitOnAnnotation>().ToLookup(a => a.Resource) is { Count: not 0 } resourcesToWaitOn)
                {
                    // Abuse the environment callback to wait for dependencies to be running
                    resource.Annotations.Add(new EnvironmentCallbackAnnotation(async context =>
                    {
                        var dependencies = new List<Task>();

                        // Find connection strings and endpoint references and get the resource they point to
                        foreach (var group in resourcesToWaitOn)
                        {
                            var resourceToWaitOn = group.Key;

                            // REVIEW: This logic does not handle cycles in the dependency graph (that would result in a deadlock)

                            // Don't wait for yourself
                            if (resourceToWaitOn != resource && resourceToWaitOn is not null)
                            {
                                var pendingAnnotations = waitingResources.GetOrAdd(
                                    resourceToWaitOn,
                                    _ => new());

                                foreach (var waitOn in group)
                                {
                                    var taskCompletionSource = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

                                    async Task Wait()
                                    {
                                        if (context.Logger is not null)
                                        {
                                            LogWaitingForResource(context.Logger, waitOn.Resource.Name);
                                        }

                                        await taskCompletionSource.Task.ConfigureAwait(false);

                                        if (context.Logger is not null)
                                        {
                                            LogWaitingForResourceCompleted(context.Logger, waitOn.Resource.Name);
                                        }
                                    }

                                    pendingAnnotations[waitOn] = taskCompletionSource;

                                    dependencies.Add(Wait());
                                }
                            }
                        }

                        await resourceNotificationService.PublishUpdateAsync(resource, s => s with { State = new("Waiting", KnownResourceStateStyles.Info) }).ConfigureAwait(false);

                        await Task.WhenAll(dependencies).WaitAsync(context.CancellationToken).ConfigureAwait(false);
                    }));
                }
            }

            _ = Task.Run(
                async () =>
                {
                    var cancellationToken = this.cancellationTokenSource.Token;

                    // Watch for global resource state changes
                    await foreach (var resourceEvent in resourceNotificationService.WatchAsync(cancellationToken).ConfigureAwait(false))
                    {
                        if (waitingResources.TryGetValue(resourceEvent.Resource, out var pendingAnnotations))
                        {
                            foreach (var (waitOn, taskCompletionSource) in pendingAnnotations)
                            {
                                if (waitOn.States is { } states
                                    && states.Contains(resourceEvent.Snapshot.State?.Text, StringComparer.Ordinal))
                                {
                                    _ = pendingAnnotations.TryRemove(waitOn, out _);
                                    _ = DoTheHealthCheck(resourceEvent, taskCompletionSource, cancellationToken);
                                }
                                else if (waitOn.WaitUntilCompleted)
                                {
                                    if (IsKnownTerminalState(resourceEvent.Snapshot))
                                    {
                                        _ = pendingAnnotations.TryRemove(waitOn, out _);
                                        _ = DoTheHealthCheck(resourceEvent, taskCompletionSource, cancellationToken);
                                    }
                                }
                                else if (waitOn.States is null)
                                {
                                    if (resourceEvent.Snapshot.State?.Text is "Running")
                                    {
                                        _ = pendingAnnotations.TryRemove(waitOn, out _);
                                        _ = DoTheHealthCheck(resourceEvent, taskCompletionSource, cancellationToken);
                                    }
                                    else if (IsKnownTerminalState(resourceEvent.Snapshot))
                                    {
                                        _ = pendingAnnotations.TryRemove(waitOn, out _);
                                        _ = taskCompletionSource.TrySetException(new DistributedApplicationException($"Dependency {waitOn.Resource.Name} failed to start"));
                                    }
                                }

                                // These states are terminal but we need a better way to detect that
                                static bool IsKnownTerminalState(CustomResourceSnapshot snapshot)
                                {
                                    return snapshot
                                        is { State.Text: "FailedToStart" or "Exited" }
                                        or { ExitCode: not null };
                                }
                            }
                        }
                    }
                },
                cancellationToken);

            return Task.CompletedTask;
        }

        private static async Task DoTheHealthCheck(ResourceEvent resourceEvent, TaskCompletionSource tcs, CancellationToken cancellationToken = default)
        {
            var resource = resourceEvent.Resource;

            // Right now, every resource does an independent health check, we could instead cache
            // the health check result and reuse it for all resources that depend on the same resource
            HealthCheckAnnotation? healthCheckAnnotation = default;

            // Find the relevant health check annotation.
            // If the resource has a parent, walk up the tree until we find the health check annotation.
            while (!resource.TryGetLastAnnotation(out healthCheckAnnotation))
            {
                // If the resource has a parent, walk up the tree
                if (resource is IResourceWithParent parent)
                {
                    resource = parent.Parent;
                }
                else
                {
                    break;
                }
            }

            Func<CancellationToken, ValueTask>? operation = null;

            if (healthCheckAnnotation is { Factory: { } factory })
            {
                Microsoft.Extensions.Diagnostics.HealthChecks.IHealthCheck? check = default;

                try
                {
                    check = await factory(cancellationToken).ConfigureAwait(false);

                    if (check is not null)
                    {
                        var context = new Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckContext
                        {
                            Registration = new Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckRegistration(
                                string.Empty,
                                check,
                                Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy,
                                []),
                        };

                        operation = async cancellationToken =>
                        {
                            var result = await check.CheckHealthAsync(context, cancellationToken).ConfigureAwait(false);

                            if (result is { Exception: { } ex })
                            {
                                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw(ex);
                            }

                            if (result is { Status: not Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Healthy })
                            {
                                throw new InvalidOperationException("Health check failed");
                            }
                        };
                    }
                }
                catch (Exception ex)
                {
                    _ = tcs.TrySetException(ex);

                    if (check is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }

                    return;
                }
            }

            try
            {
                if (operation is not null)
                {
                    await CreateResiliencyPipeline().ExecuteAsync(operation, cancellationToken).ConfigureAwait(false);
                }

                _ = tcs.TrySetResult();
            }
            catch (Exception ex)
            {
                _ = tcs.TrySetException(ex);
            }
        }

        private static ResiliencePipeline CreateResiliencyPipeline()
        {
            var retryUntilCancelled = new Polly.Retry.RetryStrategyOptions
            {
                BackoffType = DelayBackoffType.Constant,
                MaxRetryAttempts = 12,
                Delay = TimeSpan.FromSeconds(5),
                UseJitter = true,
            };

            return new ResiliencePipelineBuilder().AddRetry(retryUntilCancelled).Build();
        }

        [LoggerMessage(Level = LogLevel.Information, Message = "Waiting for {Resource}.")]
        private static partial void LogWaitingForResource(ILogger logger, string resource);

        [LoggerMessage(Level = LogLevel.Information, Message = "Waiting for {Resource} completed.")]
        private static partial void LogWaitingForResourceCompleted(ILogger logger, string resource);
    }
}