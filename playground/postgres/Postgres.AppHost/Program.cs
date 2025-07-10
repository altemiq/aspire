// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

const string DatabaseServer = "db1";

var builder = DistributedApplication.CreateBuilder(args);

var database = builder
    .AddPostgres16(DatabaseServer)
    .WithEnvironment("COLORBT_SHOW_HIDDEN", "1")
    .WithEnvironment("RUST_BACKTRACE", "full")
    .WithEnvironment("RUST_LOG", "debug")
    .WithDataVolume()
    .WithTle()
    .WithRust()
    .WithDotnet()
    .WithPgAdmin((container, database) =>
        container
            .WaitFor(database)
            .WithTheme(PgAdminTheme.System)
            .WithImageTag("9")
            .WithImagePullPolicy(ImagePullPolicy.Always))
    .AddDatabase($"{DatabaseServer}-database")
    .WithTleExtension("uuid_v7");

_ = builder.AddProject<Projects.Postgres_ApiService>("apiservice")
    .WithReference(database)
    .WaitFor(database);

await builder.Build().RunAsync().ConfigureAwait(false);