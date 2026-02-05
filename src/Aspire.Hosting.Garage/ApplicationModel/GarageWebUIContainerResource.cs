// -----------------------------------------------------------------------
// <copyright file="GarageWebUIContainerResource.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting.ApplicationModel;

/// <summary>
/// Represents a container resource for the Garage WebUI.
/// </summary>
/// <param name="name">The name of the container resource.</param>
public class GarageWebUIContainerResource(string name) : ContainerResource(name);