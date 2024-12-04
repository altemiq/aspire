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

_ = app.MapGet("/", static async (Amazon.S3.IAmazonS3 client, CancellationToken cancellationToken) =>
{
    const string BucketName = "aspire";

    var buckets = await client.ListBucketsAsync(cancellationToken).ConfigureAwait(false);

    // ensure the bucket exists
    if (!buckets.Buckets.Select(b => b.BucketName).Contains(BucketName, StringComparer.Ordinal))
    {
        var putBucketRequest = new Amazon.S3.Model.PutBucketRequest { BucketName = BucketName };

        await client.PutBucketAsync(putBucketRequest, cancellationToken).ConfigureAwait(false);
    }

    var random = new Random();
    var bytes = new byte[1024];
    random.NextBytes(bytes);

    var stream = new MemoryStream(bytes);
    await using (stream.ConfigureAwait(false))
    {
        var putObjectRequest = new Amazon.S3.Model.PutObjectRequest
        {
            BucketName = BucketName,
            Key = $"apire-test-{DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss", System.Globalization.CultureInfo.InvariantCulture)}",
            InputStream = stream,
        };

        _ = await client.PutObjectAsync(putObjectRequest, cancellationToken).ConfigureAwait(false);
    }

    var listObjectsRequest = new Amazon.S3.Model.ListObjectsRequest
    {
        BucketName = BucketName,
    };

    return await client.ListObjectsAsync(listObjectsRequest, cancellationToken).ConfigureAwait(false);
});

await app.RunAsync().ConfigureAwait(false);