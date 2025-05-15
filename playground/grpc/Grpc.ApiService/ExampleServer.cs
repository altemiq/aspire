// -----------------------------------------------------------------------
// <copyright file="ExampleServer.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Grpc.ApiService;

/// <summary>
/// The example server.
/// </summary>
public class ExampleServer : Altemiq.Protobuf.Service.Example.ExampleBase
{
    /// <inheritdoc />
    public override Task<Google.Protobuf.WellKnownTypes.Empty> Accept(Altemiq.Protobuf.Service.ActualMessage request, Core.ServerCallContext context) => Task.FromResult(new Google.Protobuf.WellKnownTypes.Empty());
}