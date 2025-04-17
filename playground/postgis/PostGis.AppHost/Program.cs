// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

var builder = DistributedApplication.CreateBuilder(args);

var db1 = builder
    .AddPostGis("db1")
    .WithTle();
db1.WithPgAdmin(c => c.WaitFor(db1).WithImageTag("9").WithTheme(PgAdminTheme.System));

var database = db1.AddDatabase("db1-database");

_ = builder.AddProject<Projects.PostGis_ApiService>("apiservice")
    .WithReference(db1)
    .WaitFor(database);

await builder.Build().RunAsync().ConfigureAwait(false);