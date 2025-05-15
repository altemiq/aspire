// -----------------------------------------------------------------------
// <copyright file="IGrpcUIResource.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting.ApplicationModel;

/// <summary>
/// The <see cref="Grpc"/> <see cref="IResource"/>.
/// </summary>
public interface IGrpcUIResource : IResourceWithEndpoints, IResourceWithArgs;