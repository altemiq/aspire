// -----------------------------------------------------------------------
// <copyright file="MiniStackContainerImageTags.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting.MiniStack;

/// <summary>
/// The <see cref="MiniStack"/> container image tags.
/// </summary>
internal static class MiniStackContainerImageTags
{
    /// <summary>
    /// The registry.
    /// </summary>
    public const string Registry = "docker.io";

    /// <summary>
    /// The image.
    /// </summary>
    public const string Image = "ministackorg/ministack";

    /// <summary>
    /// The tag.
    /// </summary>
    public const string Tag = "1.3";

    /// <summary>
    /// The StackPort registry.
    /// </summary>
    public const string StackPortRegistry = "docker.io";

    /// <summary>
    /// The StackPort image.
    /// </summary>
    public const string StackPortImage = "davireis/stackport";

    /// <summary>
    /// The StackPort tag.
    /// </summary>
    public const string StackPortTag = "0.3.4";
}