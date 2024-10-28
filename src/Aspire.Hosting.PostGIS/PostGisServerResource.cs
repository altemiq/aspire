// -----------------------------------------------------------------------
// <copyright file="PostGisServerResource.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting.ApplicationModel;

/// <summary>
/// A resource that represents a PostGIS container.
/// </summary>
public class PostGisServerResource(string name, ParameterResource? userName, ParameterResource password) : ApplicationModel.PostgresServerResource(name, userName, password);