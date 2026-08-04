// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
_ = builder.AddServiceDefaults();
_ = builder.Services
    .AddOpenTelemetry()
    .WithMetrics(c => c.AddAWSInstrumentation())
    .WithTracing(c => c.AddAWSInstrumentation());

// Add services to the container.
_ = builder.Services
    .AddProblemDetails()
    .AddAWSService<Amazon.S3.IAmazonS3>(builder.Configuration.GetAWSOptions())
    .AddAWSService<Amazon.SQS.IAmazonSQS>(builder.Configuration.GetAWSOptions());

var app = builder.Build();

// Configure the HTTP request pipeline.
_ = app.UseExceptionHandler();

_ = app.MapDefaultEndpoints();

_ = app.MapGet("/", async (Amazon.SQS.IAmazonSQS sqs, Amazon.S3.IAmazonS3 s3, CancellationToken cancellationToken) =>
{
    const string BucketName = "aspire";
    const string QueueName = "ministack-queue";

    var random = System.Security.Cryptography.RandomNumberGenerator.Create();
    var bytes = new byte[1024];
    random.GetBytes(bytes);

    var stream = new MemoryStream(bytes);
    await using (stream.ConfigureAwait(false))
    {
        var putObjectRequest = new Amazon.S3.Model.PutObjectRequest { BucketName = BucketName, Key = $"aspire-test-{DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss", System.Globalization.CultureInfo.InvariantCulture)}", InputStream = stream, };

        _ = await s3.PutObjectAsync(putObjectRequest, cancellationToken).ConfigureAwait(false);
    }

    var getQueueUrlResponse = await sqs.GetQueueUrlAsync(QueueName, cancellationToken).ConfigureAwait(false);
    var messageQueue = new Queue<string>();
    while (true)
    {
        var messages = await sqs
           .ReceiveMessageAsync(
               new Amazon.SQS.Model.ReceiveMessageRequest()
               {
                   QueueUrl = getQueueUrlResponse.QueueUrl,
                   WaitTimeSeconds = 5,
               },
               cancellationToken)
           .ConfigureAwait(false);

        if (messages.Messages is null or { Count: 0 })
        {
            break;
        }

        foreach (var message in messages.Messages)
        {
            messageQueue.Enqueue(message.Body);
        }
    }

    if (messageQueue.Count > 0)
    {
        return string.Join(Environment.NewLine, messageQueue);
    }

    throw new InvalidOperationException("Failed to receive SQS messages");
});

_ = app.MapGet("buckets", async (Amazon.S3.IAmazonS3 s3, CancellationToken cancellationToken) =>
{
    var response = await s3.ListBucketsAsync(cancellationToken).ConfigureAwait(false);

    if (response.Buckets is { Count: > 0 } buckets)
    {
        return string.Join(Environment.NewLine, buckets.Select(x => x.BucketName));
    }

    return "Failed to find any buckets";
});

_ = app.MapGet("/test-data", async (Amazon.S3.IAmazonS3 s3, CancellationToken cancellationToken) =>
{
    var response = await s3.ListObjectsV2Async(new() { BucketName = "test-data" }, cancellationToken).ConfigureAwait(false);

    if (response.S3Objects is { Count: > 0 } objects)
    {
        return string.Join(Environment.NewLine, objects.Select(x => x.Key));
    }

    return "Failed to find anything in test-data";
});

await app.RunAsync().ConfigureAwait(false);