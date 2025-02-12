// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

var builder = DistributedApplication.CreateBuilder(args);

_ = builder.AddContainer("tinyows", "camptocamp/tinyows")
    .WithImageRegistry("docker.io")
    .WithHttpEndpoint(targetPort: 80)
    .WithHttpEndpointPath("cgi-bin/tinyows");

await builder.Build().RunAsync().ConfigureAwait(false);