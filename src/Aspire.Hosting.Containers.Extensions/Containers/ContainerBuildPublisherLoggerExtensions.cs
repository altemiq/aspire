// -----------------------------------------------------------------------
// <copyright file="ContainerBuildPublisherLoggerExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting.Containers;

using Microsoft.Extensions.Logging;

/// <summary>
/// Provides a set of extension methods for interacting with ANSI console
/// functionality within the Kubernetes hosting context.
/// This static class facilitates additional behaviors or utility methods
/// that extend the base ANSI console capabilities.
/// </summary>
internal static partial class ContainerBuildPublisherLoggerExtensions
{
    [LoggerMessage(LogLevel.Information, "Building containers")]
    internal static partial void StartBuildingContainers(this ILogger logger);

    [LoggerMessage(LogLevel.Warning, "Not in publishing mode. Skipping building containers.")]
    internal static partial void NotInPublishingMode(this ILogger logger);

    [LoggerMessage(LogLevel.Information, "Successfully built containers")]
    internal static partial void FinishBuildingContainers(this ILogger logger);
}