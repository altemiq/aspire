// -----------------------------------------------------------------------
// <copyright file="AppHost.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

var builder = DistributedApplication.CreateBuilder(args);

_ = builder.AddContainer("mycontainer", "myimage")
    .WithContainerfile("Context");

var postgres = builder
    .AddPostGis("database", PostgresVersion.V16, PostGisVersion.V3_5)
    .WithTle()
    .WithRust()
    .WithTleExamples()
    .WithImageBuildPolicy(ImageBuildPolicy.Default);

_ = postgres.AddDatabase("db1")
    .WithTleExtension("uuid_v7");

_ = builder.UpdateDockerfileBuildSymLinks();

await builder.Build().RunAsync(CancellationToken.None).ConfigureAwait(false);