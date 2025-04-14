// -----------------------------------------------------------------------
// <copyright file="GrpcUIExecutableResource.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting.ApplicationModel;

/// <summary>
/// Represents a executable resource for <c>GrpcUI</c>.
/// </summary>
/// <param name="name">The name of the executable resource.</param>
/// <exception cref="InvalidOperationException">Could not find the gRPC executable.</exception>
public sealed class GrpcUIExecutableResource(string name) : ExecutableResource(name, GetToolPath(), Environment.CurrentDirectory)
{
    private static string GetToolPath() => Altemiq.Runtime.Resolve.Tool(Altemiq.Runtime.InteropServices.RuntimeEnvironment.CreateExecutableName("grpcui")) ?? throw new InvalidOperationException("Failed to find the grpcui executable");
}