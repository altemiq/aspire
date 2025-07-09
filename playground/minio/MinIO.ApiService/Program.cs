// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using MinIO.ApiService;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

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
    .AddAWSService<global::Amazon.S3.IAmazonS3>(builder.Configuration.GetAWSOptions());

builder.AddRabbitMQClient("rabbitmq");
_ = builder.Services.AddHostedService<Program.RabbitMqListener>();

var app = builder.Build();

// Configure the HTTP request pipeline.
_ = app.UseExceptionHandler();

_ = app.MapDefaultEndpoints();

_ = app.MapGet("/", static async (Amazon.S3.IAmazonS3 client, CancellationToken cancellationToken) =>
{
    const string BucketName = "aspire";
    const string Queue = "arn:minio:sqs:ap-southeast-2:rabbitmq:amqp";

    // ensure the bucket exists
    if (!await Amazon.S3.Util.AmazonS3Util.DoesS3BucketExistV2Async(client, BucketName).ConfigureAwait(false))
    {
        var putBucketRequest = new Amazon.S3.Model.PutBucketRequest { BucketName = BucketName };

        _ = await client.PutBucketAsync(putBucketRequest, cancellationToken).ConfigureAwait(false);
    }

    var notifications = await client.GetBucketNotificationAsync(BucketName, cancellationToken).ConfigureAwait(false);
    if (notifications.QueueConfigurations?.TrueForAll(static q => !q.Queue.Equals(Queue, StringComparison.Ordinal) && !q.Events.Contains(Amazon.S3.EventType.ObjectCreatedAll)) != false)
    {
        var putBucketNotificationRequest = new Amazon.S3.Model.PutBucketNotificationRequest
        {
            BucketName = BucketName,
            QueueConfigurations =
            [
                new Amazon.S3.Model.QueueConfiguration
                {
                    Queue = Queue,
                    Events = [
                        Amazon.S3.EventType.ObjectCreatedAll,
                    ],
                },
            ],
        };

        _ = await client.PutBucketNotificationAsync(putBucketNotificationRequest, cancellationToken).ConfigureAwait(false);
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

/// <summary>
/// The program class.
/// </summary>
internal partial class Program
{
    /// <summary>
    /// The RabbitMQ listener.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    public sealed partial class RabbitMqListener(IServiceProvider serviceProvider) : BackgroundService
    {
        /// <inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            const string name = "minio";
            var connection = serviceProvider.GetRequiredService<RabbitMQ.Client.IConnection>();
            var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<RabbitMqListener>();
            var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken).ConfigureAwait(false);

            await channel.ExchangeDeclareAsync(name, ExchangeType.Direct, cancellationToken: stoppingToken).ConfigureAwait(false);
            _ = await channel.QueueDeclareAsync(name, cancellationToken: stoppingToken).ConfigureAwait(false);
            await channel.QueueBindAsync(name, name, name, cancellationToken: stoppingToken).ConfigureAwait(false);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (_, ea) =>
            {
                LogReceivedMessage(logger);
                await channel.BasicAckAsync(ea.DeliveryTag, multiple: false, stoppingToken).ConfigureAwait(false);
            };

            // this consumer tag identifies the subscription when it has to be cancelled
            var consumerTag = await channel.BasicConsumeAsync(name, autoAck: false, consumer, stoppingToken).ConfigureAwait(false);

            _ = await stoppingToken;

            await channel.BasicCancelAsync(consumerTag, noWait: true, CancellationToken.None).ConfigureAwait(false);
        }

        [LoggerMessage(Level = LogLevel.Information, Message = "Received RabbitMQ message")]
        private static partial void LogReceivedMessage(ILogger logger);
    }
}