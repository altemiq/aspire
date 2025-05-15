// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

var builder = DistributedApplication.CreateBuilder(args);

var db1 = builder
    .AddPostGis("db1")
    .WithDataVolume();

db1.WithPgAdmin(container =>
    container
        .WaitFor(db1)
        .WithTheme(PgAdminTheme.System)
        .WithImageTag("9")
        .WithImagePullPolicy(ImagePullPolicy.Always));

var database = db1.AddDatabase("db1-database");

_ = builder.AddProject<Projects.PostGis_ApiService>("apiservice")
    .WithReference(database)
    .WaitFor(database);

await builder.Build().RunAsync().ConfigureAwait(false);