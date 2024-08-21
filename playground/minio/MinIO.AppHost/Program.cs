// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

var builder = DistributedApplication.CreateBuilder(args);

var minio = builder.AddMinIO("minio").WithHealthChecks();

builder.AddProject<Projects.MinIO_ApiService>("minio-apiservice")
    .WithReference(minio, wait: true);

await builder.Build().RunAsync().ConfigureAwait(false);