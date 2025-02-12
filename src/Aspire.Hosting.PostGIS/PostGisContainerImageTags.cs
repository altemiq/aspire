// -----------------------------------------------------------------------
// <copyright file="PostGisContainerImageTags.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting.PostGis;

/// <summary>
/// The <c>PostGIS</c> container image tags.
/// </summary>
internal static class PostGisContainerImageTags
{
    /// <summary>
    /// The registry.
    /// </summary>
    public const string Registry = "docker.io";

    /// <summary>
    /// The image.
    /// </summary>
    public const string Image = "postgis/postgis";

    /// <summary>
    /// The PostGIS tag.
    /// </summary>
    public const string PostGisTag = "3.5";

    /// <summary>
    /// The tag.
    /// </summary>
    public const string Tag = $"17-{PostGisTag}";
}