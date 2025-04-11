// -----------------------------------------------------------------------
// <copyright file="MapServerContainerImageTags.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting.MapServer;

/// <summary>
/// The <see cref="MapServer"/> container image tags.
/// </summary>
internal static class MapServerContainerImageTags
{
    /// <summary>
    /// The registry.
    /// </summary>
    public const string Registry = "docker.io";

    /// <summary>
    /// The image.
    /// </summary>
    public const string Image = "camptocamp/mapserver";

    /// <summary>
    /// The tag.
    /// </summary>
    public const string Tag = "8.0";
}