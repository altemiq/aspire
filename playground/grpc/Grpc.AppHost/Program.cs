// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Grpc_ApiService>("grpc-apiservice", Uri.UriSchemeHttp)
    .WithGrpcUI();

await builder.Build().RunAsync().ConfigureAwait(false);