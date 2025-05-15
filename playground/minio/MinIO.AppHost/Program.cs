// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Altavec">
// Copyright (c) Altavec. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;

const string ProfileName = "minio";

var builder = DistributedApplication.CreateBuilder(args);
builder.Services.AddHttpClient();

var region = Amazon.RegionEndpoint.APSoutheast2;

var profiles = builder.AddAWSProfileConfig()
    .AsConfigurationFile()
    .WithProfile(ProfileName);

var config = builder.AddAWSSDKConfig()
    .WithRegion(region)
    .WithProfile(ProfileName);

var rabbitmq = builder
    .AddRabbitMQ("rabbitmq")
    .WithManagementPlugin()
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume();

var minio = builder
    .AddMinIO("minio", config: config)
    .WithReference(profiles)
    .WithAmqpReference(rabbitmq)
    .WithDataVolume();

_ = builder.AddAmazonS3(minio, config);

_ = minio.EnsureBucket(
    "aspire",
    Amazon.S3.EventType.ObjectCreatedAll,
    Amazon.S3.EventType.ObjectRemovedAll,
    Amazon.S3.EventType.ObjectRestoreAll);

builder.AddProject<Projects.MinIO_ApiService>("minio-apiservice")
    .WithReference(minio).WaitFor(minio)
    .WithReference(rabbitmq).WaitFor(rabbitmq)
    .WithReference(profiles)
    .WithReference(config);

await builder.Build().RunAsync().ConfigureAwait(false);