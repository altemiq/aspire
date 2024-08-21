// -----------------------------------------------------------------------
// <copyright file="ConfigurationExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting;

using Microsoft.Extensions.Configuration;

/// <summary>
/// <see cref="Microsoft.Extensions.Configuration"/> extensions.
/// </summary>
internal static class ConfigurationExtensions
{
    /// <summary>
    /// Gets a value indicating whether we are running under <c>podman</c>.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    /// <returns><see langword="true"/> if running under <c>podman</c>; otherwise <see langword="false"/>.</returns>
    public static bool IsPodman(this IConfiguration configuration) => configuration.GetSection("DcpPublisher:ContainerRuntime") is { Value: "podman" };
}