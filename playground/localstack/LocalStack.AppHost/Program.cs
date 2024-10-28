// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

var builder = DistributedApplication.CreateBuilder(args);

var localstack = builder
    .AddLocalStack("localstack", services: LocalStackServices.Community.SimpleStorageService)
    .WithDataVolume();

builder.AddProject<Projects.LocalStack_ApiService>("localstack-apiservice")
    .WithReference(localstack)
    .WaitFor(localstack);

await builder.Build().RunAsync().ConfigureAwait(false);