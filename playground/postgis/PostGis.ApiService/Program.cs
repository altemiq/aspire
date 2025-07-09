// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Npgsql;

const string ActivitySourceName = "PostGis.ApiService";

var activitySource = new System.Diagnostics.ActivitySource(ActivitySourceName);
var meter = new System.Diagnostics.Metrics.Meter(ActivitySourceName);

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
_ = builder.AddServiceDefaults();

_ = builder.Services
    .AddOpenTelemetry()
    .WithMetrics(m => m.AddMeter(ActivitySourceName))
    .WithTracing(t => t.AddSource(ActivitySourceName));

// Add services to the container.
_ = builder.Services.AddProblemDetails();

builder.AddNpgsqlDataSource("db1-database");

var app = builder.Build();

// Configure the HTTP request pipeline.
_ = app.UseExceptionHandler();

_ = app.MapDefaultEndpoints();

var counter = meter.CreateCounter<int>("map.get.count");

app.MapGet("/", async (NpgsqlDataSource dataSource, ILogger<Program> logger, CancellationToken cancellationToken) =>
{
    counter.Add(1);
    using var source = activitySource.StartActivity(System.Diagnostics.ActivityKind.Server);
    LogMapGet(logger, dataSource);

    _ = source?.AddEvent(new("connection.opening"));
    var connection = await dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
    await using (connection.ConfigureAwait(false))
    {
        using (source?.AddEvent(new("extensions.setup")))
        {
            var command = connection.CreateCommand();
            await using (command.ConfigureAwait(false))
            {
                using (source?.AddEvent(new("extensions.setup.postgis")))
                {
                    command.CommandText = "CREATE EXTENSION IF NOT EXISTS postgis";
                    await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                }
            }
        }

        using (source?.AddEvent(new("command.creating")))
        {
            var command = connection.CreateCommand();
            await using (command.ConfigureAwait(false))
            {
                command.CommandText = "SELECT PostGIS_full_version();";

                _ = source?.AddEvent(new("command.executing"));
                var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
                await using (reader.ConfigureAwait(false))
                {
                    var stringBuilder = new System.Text.StringBuilder();
                    while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                    {
                        var value = reader.GetString(0).AsSpan();

                        while (value.Length > 0)
                        {
                            var index = value.IndexOf('=');
                            if (index is -1)
                            {
                                // go up until the next space
                                index = value.IndexOf(' ');
                                if (index is -1)
                                {
                                    stringBuilder.Append(value);
                                    value = [];
                                }
                                else
                                {
                                    stringBuilder.Append(value[..index]);
                                    value = value[(index + 1)..];
                                }

                                stringBuilder.AppendLine();
                            }
                            else
                            {
                                stringBuilder
                                    .Append(value[..index])
                                    .Append('=');
                                value = value[(index + 1)..];
                                if (value[0] is '"')
                                {
                                    // go until the next quote
                                    stringBuilder.Append('"');
                                    value = value[1..];
                                    index = value.IndexOf('"') + 1;
                                    stringBuilder.Append(value[..index]);
                                    value = value.Length > index ? value[(index + 1)..] : ReadOnlySpan<char>.Empty;
                                }
                                else
                                {
                                    index = value.IndexOf(' ');
                                    if (index is -1)
                                    {
                                        stringBuilder.Append(value).AppendLine();
                                        break;
                                    }

                                    stringBuilder.Append(value[..index]);
                                    value = value[(index + 1)..];
                                }

                                // see if the next character is something other than a letter
                                if (value.Length > 0)
                                {
                                    _ = value[0] switch
                                    {
                                        '[' => AppendSuffix(stringBuilder, ref value, ']'),
                                        '(' => AppendSuffix(stringBuilder, ref value, ')'),
                                        _ => stringBuilder,
                                    };

                                    stringBuilder.AppendLine();
                                }

                                static System.Text.StringBuilder AppendSuffix(System.Text.StringBuilder stringBuilder, ref ReadOnlySpan<char> value, char val)
                                {
                                    var index = value.IndexOf(val) + 1;
                                    stringBuilder.Append(' ').Append(value[..index]);
                                    value = value[(index + 1)..];
                                    return stringBuilder;
                                }
                            }
                        }

                        stringBuilder.AppendLine();
                    }

                    return stringBuilder.ToString();
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