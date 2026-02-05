// -----------------------------------------------------------------------
// <copyright file="AppHost.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

var builder = DistributedApplication.CreateBuilder(args);

const string Alias = "garage";
const string BucketName = "aspire";
const string ProfileName = Alias;

var region = Amazon.RegionEndpoint.APSoutheast2;

var profiles = builder.AddAWSProfileConfig()
    .AsConfigurationFile()
    .WithProfile(
        ProfileName,
        accessKeyId: builder.AddGarageUserParameter($"{Alias}-access-key"),
        secretAccessKey: builder.AddGaragePasswordParameter($"{Alias}-secret-key"));

var config = builder.AddAWSSDKConfig()
    .WithRegion(region)
    .WithProfile(ProfileName);

var garage = builder
    .AddGarage(Alias, regionEndPoint: region)
    .WithDataVolume()
    .WithProfile(profiles, ProfileName)
    .EnsureBucket(BucketName, ProfileName)
    .EnsureBucket("test-data", ProfileName)
    .WithGarageWebUI();

_ = builder.AddProject<Projects.Garage_ApiService>($"{Alias}-apiservice", opts => opts.ExcludeKestrelEndpoints = true)
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
    .WithReference(garage).WaitFor(garage)
    .WithReference(profiles)
    .WithReference(config);

await builder.Build().RunAsync().ConfigureAwait(false);