// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;

const string BucketName = "aspire";
const string MirrorBucketName = "test-data";
const string ProfileName = "localstack";

var builder = DistributedApplication.CreateBuilder(args);
_ = builder.Services.AddHttpClient();

var region = Amazon.RegionEndpoint.APSoutheast2;

var profiles = builder.AddAWSProfileConfig()
    .AsConfigurationFile()
    .WithProfile(ProfileName);

var config = builder.AddAWSSDKConfig()
    .WithRegion(region)
    .WithProfile(ProfileName);

var localstack = builder
    .AddLocalStack("localstack", regionEndPoint: region, services: LocalStackServices.Community.SimpleStorageService | LocalStackServices.Community.SimpleNotificationService | LocalStackServices.Community.SimpleQueueService)
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent)
    .WithSqsQueue(ProfileName)
    .EnsureBucket(BucketName, ProfileName, Amazon.S3.EventType.ObjectCreatedAll)
    .WithMirror(Path.Combine(builder.AppHostDirectory, "..", "..", ".data"), MirrorBucketName);

builder.AddProject<Projects.LocalStack_ApiService>("localstack-apiservice")
    .WithReference(localstack).WaitFor(localstack)
    .WithReference(profiles)
    .WithReference(config);

await builder.Build().RunAsync().ConfigureAwait(false);