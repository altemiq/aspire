// -----------------------------------------------------------------------
// <copyright file="ContainerBuildEnvironmentResource.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

#pragma warning disable ASPIRECOMPUTE001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning disable ASPIREPUBLISHERS001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

namespace Aspire.Hosting.Containers;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Represents a Container Build environment resource that can host application resources.
/// </summary>
public sealed class ContainerBuildEnvironmentResource : Resource, IComputeEnvironmentResource
{
    /// <summary>
    /// Initialises a new instance of the <see cref="ContainerBuildEnvironmentResource"/> class.
    /// </summary>
    /// <param name="name">The resource name.</param>
    public ContainerBuildEnvironmentResource(string name)
        : base(name) => this.Annotations.Add(new PublishingCallbackAnnotation(this.PublishAsync));

    /// <summary>
    /// Gets the resource mapping.
    /// </summary>
    internal Dictionary<IResource, ContainerBuildServiceResource> ResourceMapping { get; } = new(new ResourceNameComparer());

    private Task PublishAsync(PublishingContext context)
    {
        var imageBuilder = context.Services.GetRequiredService<Publishing.IResourceContainerImageBuilder>();

        var dockerComposePublishingContext = new ContainerBuildPublishingContext(
            context.ExecutionContext,
            imageBuilder,
            context.Logger,
            context.CancellationToken);

        return dockerComposePublishingContext.BuildAsync(context.Model, this);
    }

    private sealed class ResourceNameComparer : IEqualityComparer<IResource>
    {
        public bool Equals(IResource? x, IResource? y)
        {
            if (x is null || y is null)
            {
                return false;
            }

            return x.Name.Equals(y.Name, StringComparison.Ordinal);
        }

        public int GetHashCode(IResource obj) =>
            obj.Name.GetHashCode(StringComparison.Ordinal);
    }
}