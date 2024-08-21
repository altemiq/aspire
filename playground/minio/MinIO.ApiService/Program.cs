// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
_ = builder.AddServiceDefaults();

// Add services to the container.
_ = builder.Services
    .AddProblemDetails()
    .AddAWSService<global::Amazon.S3.IAmazonS3>(builder.Configuration.GetAWSOptions<global::Amazon.S3.AmazonS3Config>());

var app = builder.Build();

// Configure the HTTP request pipeline.
_ = app.UseExceptionHandler();

_ = app.MapDefaultEndpoints();

_ = app.MapGet("/", async (Amazon.S3.IAmazonS3 client, CancellationToken cancellationToken) => await client.ListBucketsAsync(cancellationToken).ConfigureAwait(false));

await app.RunAsync().ConfigureAwait(false);