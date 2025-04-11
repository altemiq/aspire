// -----------------------------------------------------------------------
// <copyright file="LocalStackContainerImageTags.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting.LocalStack;

/// <summary>
/// The <see cref="LocalStack"/> container image tags.
/// </summary>
internal static class LocalStackContainerImageTags
{
    /// <summary>
    /// The registry.
    /// </summary>
    public const string Registry = "docker.io";

    /// <summary>
    /// The image.
    /// </summary>
    public const string Image = "localstack/localstack";

    /// <summary>
    /// The tag.
    /// </summary>
    public const string Tag = "4.0";
}