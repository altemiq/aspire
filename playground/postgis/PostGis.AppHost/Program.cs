// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

var builder = DistributedApplication.CreateBuilder(args);

var db1 = builder.AddPostgres("db1").WithPgAdmin();

_ = builder.AddProject<Projects.PostGis_ApiService>("apiservice")
    .WithReference(db1)
    .WaitFor(db1);

await builder.Build().RunAsync().ConfigureAwait(false);