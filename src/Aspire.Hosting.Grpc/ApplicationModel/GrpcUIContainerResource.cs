// -----------------------------------------------------------------------
// <copyright file="GrpcUIContainerResource.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting.ApplicationModel;

/// <summary>
/// Represents a container resource for <c>GrpcUI</c>.
/// </summary>
/// <param name="name">The name of the container resource.</param>
public sealed class GrpcUIContainerResource(string name) : ContainerResource(name), IGrpcUIResource;