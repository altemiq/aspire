// -----------------------------------------------------------------------
// <copyright file="HealthCheckAnnotation.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting;

using Aspire.Hosting.ApplicationModel;

/// <summary>
/// The health check <see cref="IResourceAnnotation"/>.
/// </summary>
/// <param name="factory">The health check factory.</param>
public sealed class HealthCheckAnnotation(Func<CancellationToken, Task<Microsoft.Extensions.Diagnostics.HealthChecks.IHealthCheck?>> factory) : IResourceAnnotation
{
    /// <summary>
    /// Gets the health check factory.
    /// </summary>
    public Func<CancellationToken, Task<Microsoft.Extensions.Diagnostics.HealthChecks.IHealthCheck?>> Factory { get; } = factory;

    /// <summary>
    /// Creates a new <see cref="HealthCheckAnnotation"/> using the connection string factory.
    /// </summary>
    /// <typeparam name="T">The type of resource.</typeparam>
    /// <param name="resource">The resource.</param>
    /// <param name="connectionStringFactory">The connection string factory.</param>
    /// <returns>The created <see cref="HealthCheckAnnotation"/>.</returns>
    public static HealthCheckAnnotation Create<T>(T resource, Func<string, Microsoft.Extensions.Diagnostics.HealthChecks.IHealthCheck> connectionStringFactory)
        where T : IResourceWithConnectionString =>
            new(async token =>
                await resource.GetConnectionStringAsync(token).ConfigureAwait(false) is { } connectionString
                    ? connectionStringFactory(connectionString)
                    : null);

    /// <summary>
    /// Creates a new <see cref="HealthCheckAnnotation"/> using the connection string factory.
    /// </summary>
    /// <typeparam name="T">The type of resource.</typeparam>
    /// <param name="resource">The resource.</param>
    /// <param name="connectionStringFactory">The connection string factory.</param>
    /// <returns>The created <see cref="HealthCheckAnnotation"/>.</returns>
    public static HealthCheckAnnotation Create<T>(T resource, Func<string, CancellationToken, Task<Microsoft.Extensions.Diagnostics.HealthChecks.IHealthCheck>> connectionStringFactory)
        where T : IResourceWithConnectionString =>
            new(async token =>
                await resource.GetConnectionStringAsync(token).ConfigureAwait(false) is { } connectionString
                    ? await connectionStringFactory(connectionString, token).ConfigureAwait(false)
                    : null);

    /// <summary>
    /// Creates a new <see cref="HealthCheckAnnotation"/> using the URI factory.
    /// </summary>
    /// <typeparam name="T">The type of resource.</typeparam>
    /// <param name="resource">The resource.</param>
    /// <param name="endpointFactory">The endpoint factory.</param>
    /// <param name="endpointName">The endpoint name.</param>
    /// <returns>The created <see cref="HealthCheckAnnotation"/>.</returns>
    public static HealthCheckAnnotation Create<T>(T resource, Func<Uri, Microsoft.Extensions.Diagnostics.HealthChecks.IHealthCheck> endpointFactory, string endpointName)
        where T : IResourceWithEndpoints =>
            new(_ =>
                GetUri(resource, endpointName) is { } uri
                    ? Task.FromResult<Microsoft.Extensions.Diagnostics.HealthChecks.IHealthCheck?>(endpointFactory(uri))
                    : Task.FromResult<Microsoft.Extensions.Diagnostics.HealthChecks.IHealthCheck?>(default));

    /// <summary>
    /// Creates a new <see cref="HealthCheckAnnotation"/> using the URI factory.
    /// </summary>
    /// <typeparam name="T">The type of resource.</typeparam>
    /// <param name="resource">The resource.</param>
    /// <param name="endpointFactory">The endpoint factory.</param>
    /// <param name="endpointName">The endpoint name.</param>
    /// <returns>The created <see cref="HealthCheckAnnotation"/>.</returns>
    public static HealthCheckAnnotation Create<T>(T resource, Func<Uri, CancellationToken, Task<Microsoft.Extensions.Diagnostics.HealthChecks.IHealthCheck>> endpointFactory, string endpointName)
        where T : IResourceWithEndpoints =>
            new(async token =>
                GetUri(resource, endpointName) is { } uri
                    ? await endpointFactory(uri, token).ConfigureAwait(false)
                    : null);

    private static Uri? GetUri(IResourceWithEndpoints resource, string endpointName) => resource.GetEndpoint(endpointName) is { IsAllocated: true } endpointReference && Uri.TryCreate(endpointReference.Url, UriKind.Absolute, out var uri) ? uri : null;
}