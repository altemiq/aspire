// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Npgsql;

const string ActivitySourceName = "Postgres.ApiService";

var activitySource = new System.Diagnostics.ActivitySource(ActivitySourceName);
var meter = new System.Diagnostics.Metrics.Meter(ActivitySourceName);

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

builder.Services
    .AddOpenTelemetry()
    .WithMetrics(m => m.AddMeter(ActivitySourceName))
    .WithTracing(t => t.AddSource(ActivitySourceName));

// Add services to the container.
builder.Services.AddProblemDetails();

builder.AddNpgsqlDataSource("db1-database");

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

app.MapDefaultEndpoints();

var dataSource = app.Services.GetRequiredService<NpgsqlDataSource>();
var connection = await dataSource.OpenConnectionAsync().ConfigureAwait(false);
await using (connection.ConfigureAwait(false))
{
    using var source = activitySource.StartActivity("create.extensions", System.Diagnostics.ActivityKind.Server);
    var command = connection.CreateCommand();
    await using (command.ConfigureAwait(false))
    {
        await CreateExtension(command, "plrust", source, app.Lifetime.ApplicationStopping).ConfigureAwait(false);
        await CreateExtension(command, "pg_tle", source, app.Lifetime.ApplicationStopping).ConfigureAwait(false);
        await CreateExtension(command, "uuid_v7", source, app.Lifetime.ApplicationStopping).ConfigureAwait(false);

        async Task CreateExtension(NpgsqlCommand npgsqlCommand, string name, System.Diagnostics.Activity? activity,  CancellationToken cancellationToken)
        {
            using (activity?.AddEvent(new($"extensions.setup.{name}")))
            {
                // check to see if this exists as an extension
                npgsqlCommand.CommandText = $"CREATE EXTENSION IF NOT EXISTS {name}";
                await npgsqlCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }
}

var counter = meter.CreateCounter<int>("mapget.count");

app.MapGet("/", async (NpgsqlDataSource dataSource, ILogger<Program> logger, CancellationToken cancellationToken) =>
{
    counter.Add(1);
    using var source = activitySource.StartActivity("GET extensions", System.Diagnostics.ActivityKind.Server);
    LogMapGet(logger, dataSource);

    _ = source?.AddEvent(new("connection.opening"));
    var currentConnection = await dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
    await using (currentConnection.ConfigureAwait(false))
    {
        using (source?.AddEvent(new("command.creating")))
        {
            var command = currentConnection.CreateCommand();
            await using (command.ConfigureAwait(false))
            {
                _ = source?.AddEvent(new("command.executing"));

                var stringBuilder = new System.Text.StringBuilder();
                await RunQueryAsync(command, "SELECT extname FROM pg_extension;", record => stringBuilder.AppendLine(record.GetString(0)), cancellationToken).ConfigureAwait(false);
                stringBuilder.AppendLine();
                await RunQueryAsync(command, "SELECT generate_uuid_v7();", record => stringBuilder.AppendLine(record.GetGuid(0).ToString()), cancellationToken).ConfigureAwait(false);
                await RunQueryAsync(command, "SELECT uuid_v7_to_timestamptz('018bbaec-db78-7d42-ab07-9b8055faa6cc');", record => stringBuilder.AppendLine(record.GetDateTime(0).ToString(System.Globalization.CultureInfo.CurrentCulture)), cancellationToken).ConfigureAwait(false);
                await RunQueryAsync(command, "SELECT timestamptz_to_uuid_v7('2023-11-10 15:29:26.776-05');", record => stringBuilder.AppendLine(record.GetGuid(0).ToString()), cancellationToken).ConfigureAwait(false);
                await RunQueryAsync(command, "SELECT uuid_v7_to_timestamptz('018bbaec-db78-7afa-b2e6-c328ae861711');", record => stringBuilder.AppendLine(record.GetDateTime(0).ToString(System.Globalization.CultureInfo.CurrentCulture)), cancellationToken).ConfigureAwait(false);

                return stringBuilder.ToString();

                static async Task RunQueryAsync(
                    NpgsqlCommand command,
                    string query,
                    Action<System.Data.IDataRecord> callback,
                    CancellationToken cancellationToken)
                {
                    command.CommandText = query;
                    var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
                    await using (reader.ConfigureAwait(false))
                    {
                        await using (reader.ConfigureAwait(false))
                        {
                            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                            {
                                callback(reader);
                            }
                        }
                    }
                }
            }
        }
    }
});

await app.RunAsync().ConfigureAwait(false);

/// <content>
/// Program class.
/// </content>
internal sealed partial class Program
{
    private Program()
    {
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Entering MapGet for data source {DataSource}")]
    public static partial void LogMapGet(ILogger logger, [LogProperties] NpgsqlDataSource dataSource);
}