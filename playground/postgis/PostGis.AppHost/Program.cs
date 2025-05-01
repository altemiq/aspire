// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

var builder = DistributedApplication.CreateBuilder(args);

var db1 = builder
    .AddPostGis("db1").WithImageTag("16-3.5")
    .WithDataVolume()
    .WithTle()
    .WithPlRust();

db1.WithPgAdmin(container =>
    container
        .WaitFor(db1)
        .WithImageTag("9")
        .WithImagePullPolicy(ImagePullPolicy.Always)
        .WithTheme(PgAdminTheme.System));

var database = db1.AddDatabase("db1-database");

_ = builder.AddProject<Projects.PostGis_ApiService>("apiservice")
    .WithReference(db1)
    .WaitFor(database);

await builder.Build().RunAsync().ConfigureAwait(false);