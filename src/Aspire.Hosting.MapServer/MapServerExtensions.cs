// -----------------------------------------------------------------------
// <copyright file="MapServerExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting;

/// <summary>
/// <c>MapServer</c> extensions.
/// </summary>
public static class MapServerExtensions
{
    /// <summary>
    /// Adds a <see cref="MapServerResource"/> to the application.
    /// </summary>
    /// <param name="builder">The application builder.</param>
    /// <param name="name">The name of the resource.</param>
    /// <returns>The <see cref="MapServerResource"/>.</returns>
    public static IResourceBuilder<MapServerResource> AddMapServer(this IDistributedApplicationBuilder builder, string name)
    {
        var resource = new MapServerResource(name);
        return builder
            .AddResource(resource)
            .WithImage(MapServer.MapServerContainerImageTags.Image, MapServer.MapServerContainerImageTags.Tag)
            .WithImageRegistry(MapServer.MapServerContainerImageTags.Registry)
            .WithHttpEndpoint(targetPort: 80)
            .WithEnvironment("MS_DEBUGLEVEL", "5");
    }

    /// <summary>
    /// Adds a <see cref="MapServerResource"/> to the application.
    /// </summary>
    /// <param name="builder">The application builder.</param>
    /// <param name="name">The name of the resource.</param>
    /// <param name="contextPath">Path to be used as the context for the container image build.</param>
    /// <param name="dockerfilePath">Override path for the Dockerfile if it is not in the <paramref name="contextPath"/>.</param>
    /// <param name="stage">The stage representing the image to be published in a multi-stage Dockerfile.</param>
    /// <returns>The <see cref="MapServerResource"/>.</returns>
    public static IResourceBuilder<MapServerResource> AddMapServer(this IDistributedApplicationBuilder builder, string name, string contextPath, string? dockerfilePath = null, string? stage = null)
    {
        var resource = new MapServerResource(name);
        return builder
            .AddResource(resource)
            .WithImage("placeholder", "latest") // Image name will be replaced by WithDockerfile.
            .WithDockerfile(contextPath, dockerfilePath, stage)
            .WithHttpEndpoint(targetPort: 80)
            .WithEnvironment("MS_DEBUGLEVEL", "5");
    }
}