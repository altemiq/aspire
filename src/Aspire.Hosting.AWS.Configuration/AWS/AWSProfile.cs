// -----------------------------------------------------------------------
// <copyright file="AWSProfile.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting.AWS;

/// <summary>
/// An AWS profile.
/// </summary>
public sealed class AWSProfile
{
    /// <summary>
    /// Gets the name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the access key ID.
    /// </summary>
    public required ParameterResource AccessKeyId { get; init; }

    /// <summary>
    /// Gets the secret access key.
    /// </summary>
    public required ParameterResource SecretAccessKey { get; init; }

    /// <summary>
    /// Gets the session token.
    /// </summary>
    public ParameterResource? SessionToken { get; init; }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (obj is AWSProfile profile
            && string.Equals(profile.Name, this.Name, StringComparison.Ordinal)
            && string.Equals(profile.AccessKeyId.Value, this.AccessKeyId.Value, StringComparison.Ordinal)
            && string.Equals(profile.SecretAccessKey.Value, this.SecretAccessKey.Value, StringComparison.Ordinal))
        {
            return (profile.SessionToken, this.SessionToken) switch
            {
                (null, null) => true,
                (not null, null) or (null, not null) => false,
                ({ Value: var first }, { Value: var second }) => string.Equals(first, second, StringComparison.Ordinal),
            };
        }

        return false;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        int hash = 17;
        hash = Add(hash, GetHashCodeCore(this.Name));
        hash = Add(hash, GetHashCodeCore(this.AccessKeyId.Value));
        hash = Add(hash, GetHashCodeCore(this.SecretAccessKey.Value));
        if (this.SessionToken is { } sessionToken)
        {
            hash = Add(hash, GetHashCodeCore(sessionToken.Value));
        }

        return hash;

        static int Add(int hash, int hashCode)
        {
            unchecked
            {
                return (hash * 31) + hashCode;
            }
        }

        static int GetHashCodeCore(string str)
        {
            unchecked
            {
                var hash1 = 5381;
                var hash2 = hash1;

                for (var i = 0; i < str.Length && str[i] is not '\0'; i += 2)
                {
                    hash1 = ((hash1 << 5) + hash1) ^ str[i];
                    if (i == str.Length - 1 || str[i + 1] is '\0')
                    {
                        break;
                    }

                    hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
                }

                return hash1 + (hash2 * 1566083941);
            }
        }
    }
}