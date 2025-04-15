// -----------------------------------------------------------------------
// <copyright file="ExampleServer.cs" company="Altavec">
// Copyright (c) Altavec. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Grpc.ApiService;

/// <summary>
/// The example server.
/// </summary>
public class ExampleServer : Altavec.Protobuf.Service.Example.ExampleBase
{
    /// <inheritdoc />
    public override Task<Google.Protobuf.WellKnownTypes.Empty> Accept(Altavec.Protobuf.Service.ActualMessage request, global::Grpc.Core.ServerCallContext context) => Task.FromResult(new Google.Protobuf.WellKnownTypes.Empty());
}