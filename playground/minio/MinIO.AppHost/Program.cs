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

const string BucketName = "aspire";
const string FilesBucketName = "files";
var minio = builder
    .AddMinIO("minio", config: config)
    .WithProfile(profiles, ProfileName)
    .WithReference(profiles)
    .WithAmqpReference(rabbitmq)
    .WithDataVolume()
    .EnsureBucket(
        BucketName,
        BucketProfileName,
        Amazon.S3.EventType.ObjectCreatedAll,
        Amazon.S3.EventType.ObjectRemovedAll,
        Amazon.S3.EventType.ObjectRestoreAll)
    .WithMirror("files", FilesBucketName);

_ = builder.AddAmazonS3(minio);

_ = builder.AddProject<Projects.MinIO_ApiService>("minio-apiservice")
    .WithReference(minio).WaitFor(minio)
    .WithReference(rabbitmq).WaitFor(rabbitmq)
    .WithReference(profiles)
    .WithReference(config);

_ = builder.AddContainer("aws-container", "docker.io/amazon/aws-cli:latest")
    .WithReference(minio).WaitFor(minio)
    .WithExplicitStart()
    .WithReference(profiles)
    .WithReference(config)
    .WithArgs("s3", "ls");

const string SampleName = "sample.tif";

_ = builder
    .AddContainer("gdal-container", "ghcr.io/osgeo/gdal:alpine-small-latest")
    .WithReference(minio).WaitFor(minio)
    .WithExplicitStart()
    .WithReference(profiles)
    .WithEnvironment(callback =>
    {
        callback.EnvironmentVariables["AWS_VIRTUAL_HOSTING"] = bool.FalseString;
        callback.EnvironmentVariables["CPL_AWS_CREDENTIALS_FILE"] = callback.EnvironmentVariables[Amazon.Runtime.CredentialManagement.SharedCredentialsFile.SharedCredentialsFileEnvVar];
        callback.EnvironmentVariables["AWS_S3_ENDPOINT"] = minio.GetEndpoint("api");
    })
    .WithReference(config)
    .WithArgs("gdalinfo", $"/vsis3/{FilesBucketName}/{SampleName}");

await builder.Build().RunAsync().ConfigureAwait(false);