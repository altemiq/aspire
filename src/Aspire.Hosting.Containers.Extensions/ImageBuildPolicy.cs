// -----------------------------------------------------------------------
// <copyright file="ImageBuildPolicy.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting;

/// <summary>
/// The image build policy.
/// </summary>
public enum ImageBuildPolicy
{
    /// <summary>
    /// Default.
    /// </summary>
    Default,

    /// <summary>
    /// Only build the image when missing.
    /// </summary>
    Missing,
}