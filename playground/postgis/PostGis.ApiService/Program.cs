// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

builder.AddNpgsqlDataSource("db1");

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

app.MapDefaultEndpoints();

app.MapGet("/", async (NpgsqlDataSource dataSource, CancellationToken cancellationToken) =>
{
    var connection = await dataSource.OpenConnectionAsync(cancellationToken).ConfigureAwait(false);
    await using (connection.ConfigureAwait(false))
    {
        var command = connection.CreateCommand();
        await using (command.ConfigureAwait(false))
        {
            command.CommandText = "SELECT PostGIS_full_version();";

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