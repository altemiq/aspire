// -----------------------------------------------------------------------
// <copyright file="PostGisServerResource.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting.ApplicationModel;

extern alias core;

/// <summary>
/// A resource that represents a PostGIS container.
/// </summary>
/// <param name="name">The resource name.</param>
/// <param name="userName">The user name.</param>
/// <param name="password">The password.</param>
public class PostGisServerResource([ResourceName] string name, ParameterResource? userName, ParameterResource password) : core::Aspire.Hosting.ApplicationModel.PostgresServerResource(name, userName, password)
{
    /// <summary>
    /// The primary end point name.
    /// </summary>
    internal const string PrimaryEndpointName = "tcp";
}