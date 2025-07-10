// -----------------------------------------------------------------------
// <copyright file="ContainerBuildPublishingContext.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

#pragma warning disable ASPIREPUBLISHERS001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

namespace Aspire.Hosting.Containers;

using Microsoft.Extensions.Logging;
using Publishing;

/// <summary>
/// Represents a context for building containers for a distributed application.
/// </summary>
internal sealed class ContainerBuildPublishingContext(
    DistributedApplicationExecutionContext executionContext,
    Publishing.IResourceContainerImageBuilder imageBuilder,
    ILogger logger,
    CancellationToken cancellationToken = default)
{
    /// <summary>
    /// Builds the images.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <param name="environment">The environment.</param>
    /// <returns>The task.</returns>
    internal async Task BuildAsync(DistributedApplicationModel model, ContainerBuildEnvironmentResource environment)
    {
        if (!executionContext.IsPublishMode)
        {
            logger.NotInPublishingMode();
            return;
        }

        logger.StartBuildingContainers();

        ArgumentNullException.ThrowIfNull(model);

        await this.BuildCoreAsync(model, environment).ConfigureAwait(false);

        logger.FinishBuildingContainers();
    }

    private async Task BuildCoreAsync(DistributedApplicationModel model, ContainerBuildEnvironmentResource environment)
    {
        foreach (var serviceResource in model.Resources
                     .Select(resource => resource.GetDeploymentTargetAnnotation(environment)?.DeploymentTarget)
                     .OfType<ContainerBuildServiceResource>())
        {
            await imageBuilder.BuildImageAsync(serviceResource.TargetResource, options: default, cancellationToken).ConfigureAwait(false);
        }
    }
}