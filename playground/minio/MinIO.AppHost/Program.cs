// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;

const string ProfileName = "minio";
const string BucketProfileName = "bucket";

var builder = DistributedApplication.CreateBuilder(args);
builder.Services.AddHttpClient();

var region = Amazon.RegionEndpoint.APSoutheast2;

var profiles = builder.AddAWSProfileConfig()
    .AsConfigurationFile()
    .WithProfile(ProfileName)
    .WithProfile(BucketProfileName);

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
    .WithProfile(profiles, ProfileName)
    .WithReference(profiles)
    .WithAmqpReference(rabbitmq)
    .WithDataVolume();

_ = builder.AddAmazonS3(minio);

_ = minio.EnsureBucket(
    "aspire",
    BucketProfileName,
    Amazon.S3.EventType.ObjectCreatedAll,
    Amazon.S3.EventType.ObjectRemovedAll,
    Amazon.S3.EventType.ObjectRestoreAll);

_ = builder.AddProject<Projects.MinIO_ApiService>("minio-apiservice")
    .WithReference(minio).WaitFor(minio)
    .WithReference(rabbitmq).WaitFor(rabbitmq)
    .WithReference(profiles)
    .WithReference(config);

_ = builder.AddContainer("minio-container", "docker.io/amazon/aws-cli:latest")
    .WithReference(minio).WaitFor(minio)
    .WithReference(profiles)
    .WithReference(config)
    .WithArgs("s3", "ls");

await builder.Build().RunAsync().ConfigureAwait(false);