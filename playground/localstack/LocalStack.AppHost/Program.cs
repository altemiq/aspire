// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;

var builder = DistributedApplication.CreateBuilder(args);
builder.Services.AddHttpClient();

var localstack = builder
    .AddLocalStack("localstack", services: LocalStackServices.Community.SimpleStorageService)
    .WithDataVolume();

builder.AddProject<Projects.LocalStack_ApiService>("localstack-apiservice")
    .WithReference(localstack)
    .WaitFor(localstack);

await builder.Build().RunAsync().ConfigureAwait(false);