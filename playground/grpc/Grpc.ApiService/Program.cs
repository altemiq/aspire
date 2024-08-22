// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
_ = builder.AddServiceDefaults();

// Add services to the container.
builder.Services
    .AddGrpc(options => options.EnableDetailedErrors = true);

_ = builder.Services
    .AddProblemDetails()
    .AddGrpcReflection()
    .AddGrpcHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
_ = app.UseExceptionHandler();

_ = app.MapDefaultEndpoints();

_ = app.MapGrpcReflectionService();

_ = app.MapGrpcHealthChecksService();

_ = app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

await app.RunAsync().ConfigureAwait(false);