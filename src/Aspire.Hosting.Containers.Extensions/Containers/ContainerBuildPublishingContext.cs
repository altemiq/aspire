// -----------------------------------------------------------------------
// <copyright file="ContainerBuildPublishingContext.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

#pragma warning disable ASPIRECONTAINERRUNTIME001, ASPIREPIPELINES003

namespace Aspire.Hosting.Containers;

using Microsoft.Extensions.Logging;

/// <summary>
/// Represents a context for building containers for a distributed application.
/// </summary>
internal sealed class ContainerBuildPublishingContext(
    DistributedApplicationExecutionContext executionContext,
    Publishing.IContainerRuntime imageBuilder,
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
        foreach (var targetResource in model.Resources
                     .Select(resource => resource.GetDeploymentTargetAnnotation(environment)?.DeploymentTarget)
                     .OfType<ContainerBuildServiceResource>()
                     .Select(serviceResource => serviceResource.TargetResource))
        {
            if (!targetResource.TryGetLastAnnotation<DockerfileBuildAnnotation>(out var dockerBuildAnnotation))
            {
                continue;
            }

            var options = new Publishing.ContainerImageBuildOptions();
            await imageBuilder.BuildImageAsync(
                dockerBuildAnnotation.ContextPath,
                dockerBuildAnnotation.DockerfilePath,
                options,
                dockerBuildAnnotation.BuildArguments.ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.ToString(), StringComparer.Ordinal),
                dockerBuildAnnotation.BuildSecrets.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString(), StringComparer.Ordinal),
                dockerBuildAnnotation.Stage,
                cancellationToken).ConfigureAwait(false);
        }
    }
}