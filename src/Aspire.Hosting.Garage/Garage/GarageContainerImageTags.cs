// -----------------------------------------------------------------------
// <copyright file="GarageContainerImageTags.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting.Garage;

/// <summary>
/// The <see cref="Garage"/> container image tags.
/// </summary>
internal static class GarageContainerImageTags
{
    /// <summary>
    /// The registry.
    /// </summary>
    public const string Registry = "docker.io";

    /// <summary>
    /// The image.
    /// </summary>
    public const string Image = "dxflrs/garage";

    /// <summary>
    /// The tag.
    /// </summary>
    public const string Tag = "v2.2.0";

    /// <summary>
    /// The WebUI registry.
    /// </summary>
    public const string WebUIRegistry = "docker.io";

    /// <summary>
    /// The WebUI image.
    /// </summary>
    public const string WebUIImage = "khairul169/garage-webui";

    /// <summary>
    /// The WebUI tag.
    /// </summary>
    public const string WebUITag = "latest";
}