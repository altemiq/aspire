// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

var builder = DistributedApplication.CreateBuilder(args);

var db1 = builder
    .AddPostgres16("db1")
    .WithEnvironment("COLORBT_SHOW_HIDDEN", "1")
    .WithEnvironment("RUST_BACKTRACE", "full")
    .WithEnvironment("RUST_LOG", "debug")
    .WithDataVolume()
    .WithTle()
    .WithPlRust();

db1.WithPgAdmin(container =>
    container
        .WaitFor(db1)
        .WithTheme(PgAdminTheme.System)
        .WithImageTag("9")
        .WithImagePullPolicy(ImagePullPolicy.Always));

var database = db1.AddDatabase("db1-database")
    .WithTleExtension("uuid_v7");

_ = builder.AddProject<Projects.Postgres_ApiService>("apiservice")
    .WithReference(database)
    .WaitFor(database);

await builder.Build().RunAsync().ConfigureAwait(false);