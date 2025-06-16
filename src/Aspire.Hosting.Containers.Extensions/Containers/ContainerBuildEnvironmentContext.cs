// -----------------------------------------------------------------------
// <copyright file="ContainerBuildEnvironmentContext.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting.Containers;

using Microsoft.Extensions.Logging;

/// <summary>
/// The Container Build environment context.
/// </summary>
/// <param name="environment">The environment.</param>
/// <param name="logger">The logger.</param>
internal sealed partial class ContainerBuildEnvironmentContext(ContainerBuildEnvironmentResource environment, ILogger logger)
{
    /// <summary>
    /// Creates the <see cref="ContainerBuildServiceResource"/>.
    /// </summary>
    /// <param name="resource">The resource.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The service resource.</returns>
    public Task<ContainerBuildServiceResource> CreateContainerBuildServiceResourceAsync(IResource resource, CancellationToken cancellationToken)
    {
        if (environment.ResourceMapping.TryGetValue(resource, out var existingResource))
        {
            return Task.FromResult(existingResource);
        }

        cancellationToken.ThrowIfCancellationRequested();

        LogCreatingResource(logger, resource.Name);

        var serviceResource = new ContainerBuildServiceResource(resource.Name, resource, environment);
        environment.ResourceMapping[resource] = serviceResource;

        return Task.FromResult(serviceResource);
    }

    [LoggerMessage(LogLevel.Information, "Creating Container Build service resource for {ResourceName}")]
    private static partial void LogCreatingResource(ILogger logger, string resourceName);
}