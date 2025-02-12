// -----------------------------------------------------------------------
// <copyright file="ResourceBuilderExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting;

using Aspire.Hosting.ApplicationModel;

/// <summary>
/// The resource builder extensions.
/// </summary>
public static class ResourceBuilderExtensions
{
    /// <summary>
    /// Sets the HTTP endpoint path.
    /// </summary>
    /// <typeparam name="T">The type of resource.</typeparam>
    /// <param name="builder">The builder.</param>
    /// <param name="path">The end-point path.</param>
    /// <returns>The resource builder.</returns>
    public static IResourceBuilder<T> WithHttpEndpointPath<T>(this IResourceBuilder<T> builder, string path)
        where T : IResourceWithEndpoints => WithEndpointPath(builder, Uri.UriSchemeHttp, path);

    /// <summary>
    /// Sets the HTTPS endpoint path.
    /// </summary>
    /// <typeparam name="T">The type of resource.</typeparam>
    /// <param name="builder">The builder.</param>
    /// <param name="path">The end-point path.</param>
    /// <returns>The resource builder.</returns>
    public static IResourceBuilder<T> WithHttpsEndpointPath<T>(this IResourceBuilder<T> builder, string path)
        where T : IResourceWithEndpoints => WithEndpointPath(builder, Uri.UriSchemeHttps, path);

    /// <summary>
    /// Sets the end-point path.
    /// </summary>
    /// <typeparam name="T">The type of resource.</typeparam>
    /// <param name="builder">The builder.</param>
    /// <param name="name">The name of the end point.</param>
    /// <param name="path">The end-point path.</param>
    /// <returns>The resource builder.</returns>
    public static ApplicationModel.IResourceBuilder<T> WithEndpointPath<T>(this ApplicationModel.IResourceBuilder<T> builder, string name, string path)
        where T : ApplicationModel.IResourceWithEndpoints
    {
        Lifecycle.LifecycleHookServiceCollectionExtensions.TryAddLifecycleHook<EndpointPathLifecycleHook>(builder.ApplicationBuilder.Services);
        return builder.WithAnnotation(new EndpointPathAnnotation(name, path));
    }

    private sealed record class EndpointPathAnnotation(string Name, string Path) : ApplicationModel.IResourceAnnotation;

    private sealed class EndpointPathLifecycleHook(ResourceNotificationService notifications)
        : Lifecycle.IDistributedApplicationLifecycleHook, IAsyncDisposable
    {
        private readonly CancellationTokenSource cts = new();
        private Task? watcherTask;

        public Task AfterEndpointsAllocatedAsync(DistributedApplicationModel appModel, CancellationToken cancellationToken = default)
        {
            this.watcherTask = UpdateEndpointAsync(this.cts.Token);
            return Task.CompletedTask;

            async Task UpdateEndpointAsync(CancellationToken cancellationToken)
            {
                try
                {
                    await foreach (var evt in notifications.WatchAsync(cancellationToken).WithCancellation(cancellationToken).ConfigureAwait(false))
                    {
                        if (TryGetUpdatedUrls(evt, out var urls))
                        {
                            await notifications
                                .PublishUpdateAsync(evt.Resource, snapshot => snapshot with { Urls = urls })
                                .ConfigureAwait(false);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // Ignore
                }

                static bool TryGetUpdatedUrls(ResourceEvent evt, out System.Collections.Immutable.ImmutableArray<UrlSnapshot> urls)
                {
                    if (evt.Snapshot.Urls.Length is 0 || !evt.Resource.TryGetAnnotationsOfType<EndpointPathAnnotation>(out var annotations))
                    {
                        urls = evt.Snapshot.Urls;
                        return false;
                    }

                    var snapshotUrls = evt.Snapshot.Urls.ToDictionary(u => u.Name);
                    var changed = false;
                    foreach (var annotation in annotations)
                    {
                        if (snapshotUrls.TryGetValue(annotation.Name, out var url))
                        {
                            var expectedUrl = new UriBuilder(url.Url) { Path = annotation.Path }.Uri.AbsoluteUri;
                            if (!string.Equals(url.Url, expectedUrl, StringComparison.Ordinal))
                            {
                                changed = true;
                                snapshotUrls[url.Name] = url with { Url = expectedUrl };
                            }
                        }
                    }

                    urls = [.. snapshotUrls.Values];
                    return changed;
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            using (this.cts)
            {
                await this.cts.CancelAsync().ConfigureAwait(false);

                if (this.watcherTask is { } task)
                {
                    await task.ConfigureAwait(false);
                }
            }
        }
    }
}