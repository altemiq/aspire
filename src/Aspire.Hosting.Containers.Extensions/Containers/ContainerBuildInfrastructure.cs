// -----------------------------------------------------------------------
// <copyright file="ContainerBuildInfrastructure.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting.Containers;

using Microsoft.Extensions.Logging;

/// <summary>
/// The Container Build infrastructure.
/// </summary>
/// <param name="logger">The logger.</param>
/// <param name="executionContext">The execution context.</param>
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "ClassNeverInstantiated.Global", Justification = "Instantiated via DI")]
internal sealed class ContainerBuildInfrastructure(
    ILogger<ContainerBuildInfrastructure> logger,
    DistributedApplicationExecutionContext executionContext) : Lifecycle.IDistributedApplicationLifecycleHook
{
    /// <inheritdoc/>
    public async Task BeforeStartAsync(DistributedApplicationModel appModel, CancellationToken cancellationToken = default)
    {
        if (executionContext.IsRunMode)
        {
            return;
        }

        // Find Container Build environment resources
        var containerBuildEnvironments = appModel.Resources.OfType<ContainerBuildEnvironmentResource>().ToArray();

        if (containerBuildEnvironments.Length > 1)
        {
            throw new NotSupportedException("Multiple Container Build environments are not supported.");
        }

        var environment = containerBuildEnvironments.FirstOrDefault();

        if (environment == null)
        {
            return;
        }

        var containerBuildEnvironmentContext = new ContainerBuildEnvironmentContext(environment, logger);

        foreach (var r in appModel.Resources)
        {
            if (r.TryGetLastAnnotation<ManifestPublishingCallbackAnnotation>(out var lastAnnotation) && lastAnnotation == ManifestPublishingCallbackAnnotation.Ignore)
            {
                continue;
            }

            // Skip resources that are not containers
            if (!r.IsContainer())
            {
                continue;
            }

            // Create a Container Build compute resource for the resource
            var serviceResource = await containerBuildEnvironmentContext.CreateContainerBuildServiceResourceAsync(r, cancellationToken).ConfigureAwait(false);

            // Add deployment target annotation to the resource
#pragma warning disable ASPIRECOMPUTE001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            r.Annotations.Add(new DeploymentTargetAnnotation(serviceResource) { ComputeEnvironment = environment });
#pragma warning restore ASPIRECOMPUTE001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        }
    }
}