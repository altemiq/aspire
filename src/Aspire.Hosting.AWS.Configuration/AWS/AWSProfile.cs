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
    public override bool Equals(object? obj) =>
        obj is AWSProfile profile
        && string.Equals(profile.Name, this.Name, StringComparison.Ordinal)
        && string.Equals(GetValue(profile.AccessKeyId), GetValue(this.AccessKeyId), StringComparison.Ordinal)
        && string.Equals(GetValue(profile.SecretAccessKey), GetValue(this.SecretAccessKey), StringComparison.Ordinal)
        && string.Equals(GetValue(profile.SessionToken), GetValue(this.SessionToken), StringComparison.Ordinal);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        int hash = 17;
        hash = Add(hash, GetHashCodeCore(this.Name));
        hash = Add(hash, GetHashCodeCore(GetValue(this.AccessKeyId)));
        hash = Add(hash, GetHashCodeCore(GetValue(this.SecretAccessKey)));
        hash = Add(hash, GetHashCodeCore(GetValue(this.SessionToken)));
        return hash;

        static int Add(int hash, int hashCode)
        {
            if (hashCode is 0)
            {
                return hash;
            }

            unchecked
            {
                return (hash * 31) + hashCode;
            }
        }

        static int GetHashCodeCore(string? str)
        {
            if (str is null)
            {
                return default;
            }

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

    private static string? GetValue(ParameterResource? resource)
    {
        if (resource is null)
        {
            return null;
        }

        var valueTask = resource.GetValueAsync(CancellationToken.None);
        if (valueTask.IsCompletedSuccessfully)
        {
            return valueTask.Result;
        }

        return valueTask.AsTask().GetAwaiter().GetResult();
    }
}