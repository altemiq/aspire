// -----------------------------------------------------------------------
// <copyright file="ContainerBuildServiceResource.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting.Containers;

/// <summary>
/// The Container Build service resource.
/// </summary>
/// <param name="name">The name.</param>
/// <param name="resource">The resource to build.</param>
/// <param name="composeEnvironmentResource">The parent resource.</param>
public class ContainerBuildServiceResource(string name, IResource resource, ContainerBuildEnvironmentResource composeEnvironmentResource) : Resource(name), IResourceWithParent<ContainerBuildEnvironmentResource>
{
    /// <inheritdoc/>
    public ContainerBuildEnvironmentResource Parent => composeEnvironmentResource;

    /// <summary>
    /// Gets the resource that is the target of this Container Build service.
    /// </summary>
    internal IResource TargetResource => resource;
}