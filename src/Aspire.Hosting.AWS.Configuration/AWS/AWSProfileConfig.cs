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

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is AWSProfileConfig profileConfig && this.Profiles.SequenceEqual(profileConfig.Profiles);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        var hash = 17;
        foreach (var profile in this.Profiles)
        {
            hash = Add(hash, profile.GetHashCode());
        }

        return hash;

        static int Add(int hash, int hashCode)
        {
            unchecked
            {
                return (hash * 31) + hashCode;
            }
        }
    }
}