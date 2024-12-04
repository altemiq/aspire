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
    public required ApplicationModel.ParameterResource AccessKeyId { get; init; }

    /// <summary>
    /// Gets the secret access key.
    /// </summary>
    public required ApplicationModel.ParameterResource SecretAccessKey { get; init; }

    /// <summary>
    /// Gets the session token.
    /// </summary>
    public ApplicationModel.ParameterResource? SessionToken { get; init; }
}