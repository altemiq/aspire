// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

const string Name = "grpc-apiservice";
var builder = DistributedApplication.CreateBuilder(args);

_ = builder.AddProject<Projects.Grpc_ApiService>(Name, Uri.UriSchemeHttp)
    .WithGrpcHealthCheck(Uri.UriSchemeHttp, Uri.UriSchemeHttp)
    .WithGrpcUI((api, grpc) => grpc.WaitFor(api), executableName: $"{Name}-exe")
    .WithGrpcUI((api, grpc) => grpc.WaitFor(api), containerName: $"{Name}-container");

await builder.Build().RunAsync().ConfigureAwait(false);