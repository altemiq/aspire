// -----------------------------------------------------------------------
// <copyright file="MinIOContainerImageTags.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting.MinIO;

/// <summary>
/// The <see cref="MinIO"/> container image tags.
/// </summary>
internal static class MinIOContainerImageTags
{
    /// <summary>
    /// The registry.
    /// </summary>
    public const string Registry = "quay.io";

    /// <summary>
    /// The image.
    /// </summary>
    public const string Image = "minio/minio";

    /// <summary>
    /// The tag.
    /// </summary>
    public const string Tag = "latest";
}