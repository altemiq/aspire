// -----------------------------------------------------------------------
// <copyright file="StackPortContainerResource.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting.MiniStack;

/// <summary>
/// The StackPort container resource.
/// </summary>
/// <param name="name">The resource name.</param>
public class StackPortContainerResource(string name) : ContainerResource(name);