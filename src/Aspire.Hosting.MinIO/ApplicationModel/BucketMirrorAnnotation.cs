// -----------------------------------------------------------------------
// <copyright file="BucketMirrorAnnotation.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting.ApplicationModel;

/// <summary>
/// The bucket mirror annotation.
/// </summary>
internal sealed class BucketMirrorAnnotation : IResourceAnnotation
{
    /// <summary>
    /// Gets the directory to mirror to the bucket.
    /// </summary>
    public required string Directory { get; init; }

    /// <summary>
    /// Gets the name of the bucket.
    /// </summary>
    public required string BucketName { get; init; }

    /// <summary>
    /// Gets a value indicating whether this bucket mirror is readonly.
    /// </summary>
    public bool ReadOnly { get; init; }
}