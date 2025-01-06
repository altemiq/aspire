// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using LocalStack.Client.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
_ = builder.AddServiceDefaults();

// Add services to the container.
_ = builder.Services
    .AddProblemDetails()
    .AddLocalStack(builder.Configuration)
    .AddAwsService<global::Amazon.S3.IAmazonS3>(builder.Configuration.GetAWSOptions());

var app = builder.Build();

// Configure the HTTP request pipeline.
_ = app.UseExceptionHandler();

_ = app.MapDefaultEndpoints();

_ = app.MapGet("/", async (Amazon.S3.IAmazonS3 client, CancellationToken cancellationToken) => await client.ListBucketsAsync(cancellationToken).ConfigureAwait(false));

await app.RunAsync().ConfigureAwait(false);