// -----------------------------------------------------------------------
// <copyright file="AWSProfileConfig.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting.AWS;

/// <summary>
/// The AWS configuration file.
/// </summary>
public class AWSProfileConfig : IAWSProfileConfig
{
    /// <inheritdoc/>
    public IList<AWSProfile> Profiles { get; } = [];

    /// <inheritdoc/>
    public required string Name { get; init; }

    /// <inheritdoc/>
    public ApplicationModel.ResourceAnnotationCollection Annotations { get; } = [];
}