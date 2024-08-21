// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
_ = builder.AddServiceDefaults();

// Add services to the container.
_ = builder.Services.AddProblemDetails();

builder.AddRabbitMQClient("rabbitmq");

var app = builder.Build();

// Configure the HTTP request pipeline.
_ = app.UseExceptionHandler();

_ = app.MapDefaultEndpoints();

_ = app.MapGet("/", (RabbitMQ.Client.IConnection connection) => connection.CreateModel());

await app.RunAsync().ConfigureAwait(false);