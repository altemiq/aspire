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
public class PostGisServerResource(string name, ParameterResource? userName, ParameterResource password) : core::Aspire.Hosting.ApplicationModel.PostgresServerResource(name, userName, password)
{
    /// <summary>
    /// The primary end point name.
    /// </summary>
    internal const string PrimaryEndpointName = "tcp";
}