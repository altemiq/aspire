// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Npgsql;

const string ActivitySourceName = "PostGis.ApiService";

var activitySource = new System.Diagnostics.ActivitySource(ActivitySourceName);

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

builder.Services.AddOpenTelemetry().WithTracing(t => t.AddSource(ActivitySourceName));

// Add services to the container.
builder.Services.AddProblemDetails();

builder.AddNpgsqlDataSource("db1");

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

app.MapDefaultEndpoints();

app.MapGet("/", async (NpgsqlDataSource dataSource, ILogger<Program> logger, CancellationToken cancellationToken) =>
{
    using var source = activitySource.StartActivity("GET postgis version", System.Diagnostics.ActivityKind.Server);
    Program.LogMapGet(logger, dataSource);

    source?.AddEvent(new("opening connection"));
    var connection = await dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
    await using (connection.ConfigureAwait(false))
    {
        source?.AddEvent(new("creating command"));
        var command = connection.CreateCommand();
        await using (command.ConfigureAwait(false))
        {
            command.CommandText = "SELECT PostGIS_full_version();";

            source?.AddEvent(new("executing connection"));
            var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
            await using (reader.ConfigureAwait(false))
            {
                var stringBuilder = new System.Text.StringBuilder();
                while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                {
                    stringBuilder.AppendLine(reader.GetString(0));
                }

                return stringBuilder.ToString();
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
    public static partial void LogMapGet(ILogger logger, NpgsqlDataSource dataSource);
}