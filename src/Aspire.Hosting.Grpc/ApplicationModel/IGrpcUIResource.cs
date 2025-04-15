// -----------------------------------------------------------------------
// <copyright file="IGrpcUIResource.cs" company="altemiq">
// Copyright (c) altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting.ApplicationModel;

/// <summary>
/// The <see cref="Grpc"/> <see cref="IResource"/>.
/// </summary>
public interface IGrpcUIResource : IResourceWithEndpoints, IResourceWithArgs;