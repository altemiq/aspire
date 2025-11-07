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
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "ClassNeverInstantiated.Global", Justification = "Instantiated via DI")]
[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "This is required")]
internal sealed class ContainerBuildInfrastructure(ILogger<ContainerBuildInfrastructure> logger) : Lifecycle.IDistributedApplicationEventingSubscriber
{
    /// <inheritdoc/>
    public Task SubscribeAsync(Eventing.IDistributedApplicationEventing eventing, DistributedApplicationExecutionContext executionContext, CancellationToken cancellationToken)
    {
        eventing.Subscribe<BeforeStartEvent>(async (@event, ct) =>
        {
            if (executionContext.IsRunMode)
            {
                return;
            }

            // Find Container Build environment resources
            var appModel = @event.Model;
            var containerBuildEnvironments = appModel.Resources.OfType<ContainerBuildEnvironmentResource>().ToArray();

            if (containerBuildEnvironments.Length > 1)
            {
                throw new NotSupportedException("Multiple Container Build environments are not supported.");
            }

            var environment = containerBuildEnvironments.FirstOrDefault();

            if (environment is null)
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
                var serviceResource = await containerBuildEnvironmentContext.CreateContainerBuildServiceResourceAsync(r, ct).ConfigureAwait(false);

                // Add deployment target annotation to the resource
#pragma warning disable ASPIRECOMPUTE001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
                r.Annotations.Add(new DeploymentTargetAnnotation(serviceResource) { ComputeEnvironment = environment });
#pragma warning restore ASPIRECOMPUTE001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            }
        });

        return Task.CompletedTask;
    }
}