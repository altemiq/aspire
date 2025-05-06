// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var builder = DistributedApplication.CreateBuilder(args);

var db1 = builder
    .AddPostgres("db1").WithImageTag("16")
    .WithDataVolume()
    .WithTle()
    .WithPlRust();

db1.WithPgAdmin(container =>
    container
        .WaitFor(db1)
        .WithImageTag("9")
        .WithImagePullPolicy(ImagePullPolicy.Always)
        .WithTheme(PgAdminTheme.System));

var database = db1.AddDatabase("db1-database")
    .WithTleExtension("uuid_v7");

_ = builder.AddProject<Projects.Postgres_ApiService>("apiservice")
    .WithReference(database)
    .WaitFor(database);

await builder.Build().RunAsync().ConfigureAwait(false);