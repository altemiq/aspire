// -----------------------------------------------------------------------
// <copyright file="LocalStackServerResource.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting.ApplicationModel;

/// <summary>
/// A resource that represents a LocalStack container.
/// </summary>
/// <param name="name">The name of the resource.</param>
/// <param name="region">The region.</param>
public sealed class LocalStackServerResource(string name, string region) : ContainerResource(name), IResourceWithServiceDiscovery
{
    /// <summary>
    /// Gets the region.
    /// </summary>
    public string Region { get; } = region;

    /// <summary>
    /// Gets the services.
    /// </summary>
    public LocalStackServices.Community Services { get; init; }

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
    internal static IEnumerable<string> GetServiceNames(LocalStackServices.Community services)
    {
        var resultValue = (long)services;
        var type = typeof(LocalStackServices.Community);
        var fields = type.GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
        for (var i = fields.Length - 1; i >= 0; i--)
        {
            var field = fields[i];
            if (field.GetValue(null) is not LocalStackServices.Community enumValue)
            {
                continue;
            }

            var longValue = (long)enumValue;
            if ((resultValue & longValue) == longValue)
            {
                yield return Attribute.GetCustomAttribute(field, typeof(System.ComponentModel.DescriptionAttribute)) is System.ComponentModel.DescriptionAttribute descriptionAttribute
                    ? descriptionAttribute.Description.ToLowerInvariant()
                    : field.Name.ToLowerInvariant();
            }
        }
    }
}