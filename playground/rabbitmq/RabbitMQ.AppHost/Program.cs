// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

var builder = DistributedApplication.CreateBuilder(args);

var rabbitmq = builder.AddRabbitMQ("rabbitmq").WithHealthCheck();

builder.AddProject<Projects.RabbitMQ_ApiService>("rabbitmq-apiservice")
    .WithReference(rabbitmq, wait: true);

await builder.Build().RunAsync().ConfigureAwait(false);