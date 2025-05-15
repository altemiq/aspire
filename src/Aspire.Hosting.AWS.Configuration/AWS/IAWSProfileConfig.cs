// -----------------------------------------------------------------------
// <copyright file="IAWSProfileConfig.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting.AWS;

/// <summary>
/// The AWS configuration file.
/// </summary>
public interface IAWSProfileConfig : IResource
{
    /// <summary>
    /// Gets the profiles.
    /// </summary>
    IList<AWSProfile> Profiles { get; }
}