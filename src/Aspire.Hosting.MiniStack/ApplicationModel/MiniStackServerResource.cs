// -----------------------------------------------------------------------
// <copyright file="MiniStackServerResource.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting.ApplicationModel;

/// <summary>
/// A resource that represents a MiniStack container.
/// </summary>
/// <param name="name">The name of the resource.</param>
/// <param name="region">The region.</param>
public class MiniStackServerResource(string name, string region) : ContainerResource(name), IResourceWithServiceDiscovery
{
    /// <summary>
    /// Gets the region.
    /// </summary>
    public string Region { get; } = region;

    /// <summary>
    /// Gets the services.
    /// </summary>
    public MiniStackServices? Services { get; init; }

    /// <summary>
    /// Gets the service names.
    /// </summary>
    /// <returns>The service names.</returns>
    public IEnumerable<string> GetServiceNames() => GetServiceNames(this.Services);

    /// <summary>
    /// Gets the service names.
    /// </summary>
    /// <param name="services">The services to get the names for.</param>
    /// <returns>The service names.</returns>
    internal static IEnumerable<string> GetServiceNames(MiniStackServices? services)
    {
        if (services is null)
        {
            yield break;
        }

        foreach (var service in services)
        {
            yield return service.Value;
        }
    }
}