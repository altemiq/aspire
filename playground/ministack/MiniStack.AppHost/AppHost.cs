// -----------------------------------------------------------------------
// <copyright file="AppHost.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Microsoft.Extensions.DependencyInjection;

const string BucketName = "aspire";
const string MirrorBucketName = "test-data";
const string MissingFolderBucketName = "missing-folder";
const string ProfileName = "ministack";

var builder = DistributedApplication.CreateBuilder(args);
_ = builder.Services.AddHttpClient();

var region = Amazon.RegionEndpoint.APSoutheast2;

var profiles = builder.AddAWSProfileConfig()
    .AsConfigurationFile()
    .WithProfile(
        ProfileName,
        builder.AddParameter("aws-access-key", new GenerateParameterDefault { Lower = false, Upper = false, Special = false, MinLength = 12 }, persist: true));

var config = builder.AddAWSSDKConfig()
    .WithRegion(region)
    .WithProfile(ProfileName);

builder.SetAWSConfig(config);

var ministack = builder
    .AddMiniStack("ministack", regionEndPoint: region, services: MiniStackServices.SimpleStorageService | MiniStackServices.SimpleNotificationService | MiniStackServices.SimpleQueueService)
    .WithStateVolume()
    .WithDataBindMount(Path.Combine(builder.AppHostDirectory, "data"))
    .WithSqsQueue(ProfileName)
    .EnsureBucket(BucketName, ProfileName, Amazon.S3.EventType.ObjectCreatedAll)
    .EnsureBucket(MissingFolderBucketName, ProfileName)
    .WithMirror(Path.Combine(builder.AppHostDirectory, "..", "..", ".data"), MirrorBucketName)
    .WithMirror(Path.Combine(builder.AppHostDirectory, MissingFolderBucketName), MissingFolderBucketName)
    .WithStackPort(profile: profiles.Resource.Profiles.Single());

builder.AddProject<Projects.MiniStack_ApiService>("ministack-apiservice", opts => opts.ExcludeKestrelEndpoints = true)
    .WithUrls(callback =>
    {
        callback.Urls.Clear();
        var endpoint = callback.GetEndpoint(Uri.UriSchemeHttps);
        callback.Urls.Add(new() { Url = CreateUrl(endpoint).ToString(), Endpoint = endpoint, DisplayText = $"{Uri.UriSchemeHttps}" });
        callback.Urls.Add(new() { Url = CreateUrl(endpoint, "buckets").ToString(), Endpoint = endpoint, DisplayText = $"{Uri.UriSchemeHttps}-buckets" });
        callback.Urls.Add(new() { Url = CreateUrl(endpoint, "test-data").ToString(), Endpoint = endpoint, DisplayText = $"{Uri.UriSchemeHttps}-test-data" });

        endpoint = callback.GetEndpoint(Uri.UriSchemeHttp);
        callback.Urls.Add(new() { Url = CreateUrl(endpoint).ToString(), Endpoint = callback.GetEndpoint(Uri.UriSchemeHttps), DisplayText = $"{Uri.UriSchemeHttp}" });
        callback.Urls.Add(new() { Url = CreateUrl(endpoint, "buckets").ToString(), Endpoint = callback.GetEndpoint(Uri.UriSchemeHttp), DisplayText = $"{Uri.UriSchemeHttp}-buckets" });
        callback.Urls.Add(new() { Url = CreateUrl(endpoint, "test-data").ToString(), Endpoint = callback.GetEndpoint(Uri.UriSchemeHttp), DisplayText = $"{Uri.UriSchemeHttp}-test-data" });

        static Uri CreateUrl(EndpointReference? endpoint, string? path = null)
        {
            var builder = new UriBuilder(endpoint?.Scheme, endpoint?.Host, endpoint?.Port ?? -1)
            {
                Path = path,
            };

            return builder.Uri;
        }
    })
    .WithReference(ministack).WaitFor(ministack)
    .WithReference(profiles)
    .WithReference(config);

await builder.Build().RunAsync().ConfigureAwait(false);