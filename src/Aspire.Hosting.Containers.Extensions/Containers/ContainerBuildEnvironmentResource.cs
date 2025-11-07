// -----------------------------------------------------------------------
// <copyright file="ContainerBuildEnvironmentResource.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

#pragma warning disable ASPIREPIPELINES001, ASPIREPIPELINES003

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
        : base(name) => this.Annotations.Add(new Pipelines.PipelineStepAnnotation(_ =>
    {
        var steps = new List<Pipelines.PipelineStep>();

        var publishStep = new Pipelines.PipelineStep
        {
            Name = $"publish-{this.Name}",
            Action = this.PublishAsync,
        };

        publishStep.RequiredBy(Pipelines.WellKnownPipelineSteps.Publish);
        steps.Add(publishStep);

        return steps;
    }));

    /// <summary>
    /// Gets the resource mapping.
    /// </summary>
    internal Dictionary<IResource, ContainerBuildServiceResource> ResourceMapping { get; } = new(new ResourceNameComparer());

    private Task PublishAsync(Pipelines.PipelineStepContext context)
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