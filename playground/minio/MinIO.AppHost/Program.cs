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

const string BucketName = "aspire";

_ = minio.EnsureBucket(
    BucketName,
    BucketProfileName,
    Amazon.S3.EventType.ObjectCreatedAll,
    Amazon.S3.EventType.ObjectRemovedAll,
    Amazon.S3.EventType.ObjectRestoreAll);

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
    .WithArgs("gdalinfo", $"/vsis3/{BucketName}/{SampleName}");

// add the file to the bucket
builder.Eventing.Subscribe<ResourceReadyEvent>(minio.Resource, async (evt, cancellationToken) =>
{
    var client = evt.Services.GetRequiredKeyedService<Amazon.S3.IAmazonS3>(evt.Resource.Name);
    if (client.Config is Amazon.Runtime.ClientConfig clientConfig)
    {
        // get the profile
        var chain = new Amazon.Runtime.CredentialManagement.CredentialProfileStoreChain();
        if (chain.TryGetAWSCredentials(BucketProfileName, out var profileCredentials))
        {
            clientConfig.DefaultAWSCredentials = profileCredentials;
        }
    }

    if (await KeyExistsAsync(client, BucketName, SampleName, cancellationToken).ConfigureAwait(false))
    {
        return;
    }

    var putRequest = new Amazon.S3.Model.PutObjectRequest { BucketName = BucketName, Key = SampleName, InputStream = File.OpenRead(Path.Combine(builder.AppHostDirectory, SampleName)) };

    _ = await client.PutObjectAsync(putRequest, cancellationToken).ConfigureAwait(false);

    static async Task<bool> KeyExistsAsync(Amazon.S3.IAmazonS3 client, string bucket, string key, CancellationToken cancellationToken)
    {
        var request = new Amazon.S3.Model.GetObjectMetadataRequest { BucketName = bucket, Key = key };

        try
        {
            await client.GetObjectMetadataAsync(request, cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch (Amazon.S3.AmazonS3Exception)
        {
            return false;
        }
    }
});

await builder.Build().RunAsync().ConfigureAwait(false);