// -----------------------------------------------------------------------
// <copyright file="PostGisBuilderExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

/// <summary>
/// Extensions for <c>PostGIS</c>.
/// </summary>
public static partial class PostGisBuilderExtensions
{
    private const string UserEnvVarName = "POSTGRES_USER";
    private const string PasswordEnvVarName = "POSTGRES_PASSWORD";

    /// <summary>
    /// Configures the Postgres container resource to enable the PostGIS extension.
    /// </summary>
    /// <typeparam name="T">The type of postgres container.</typeparam>
    /// <param name="builder">The <see cref="IDistributedApplicationBuilder"/>.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<T> WithPostGis<T>(this IResourceBuilder<T> builder)
        where T : PostgresServerResource
    {
        var tag = PostGis.PostGisContainerImageTags.Tag;
        if (builder.Resource.TryGetLastAnnotation<ContainerImageAnnotation>(out var containerImage)
            && containerImage is { Tag: { } containerImageTag })
        {
            tag = GetTag(containerImageTag, System.Globalization.CultureInfo.InvariantCulture);
        }

        _ = builder
            .WithImage(PostGis.PostGisContainerImageTags.Image, tag)
            .WithImageRegistry(PostGis.PostGisContainerImageTags.Registry);

        return builder;

        static string GetTag(string tag, IFormatProvider? formatProvider)
        {
            if (tag is "latest")
            {
                return tag;
            }

            Span<Range> ranges = stackalloc Range[2];
            var tagSpan = tag.AsSpan();
            var values = tagSpan.Split(ranges, '-');
            string prefix;
            string suffix;
            if (double.TryParse(tagSpan[ranges[0]], formatProvider, out var version))
            {
                prefix = $"{double.Truncate(version).ToString(formatProvider)}-{PostGis.PostGisContainerImageTags.PostGisTag}";
                suffix = values is 1
                    ? string.Empty
                    : GetSuffix(tagSpan[ranges[1]]);
            }
            else
            {
                prefix = PostGis.PostGisContainerImageTags.Tag;
                suffix = GetSuffix(tagSpan);
            }

            return prefix + suffix;

            static string GetSuffix(ReadOnlySpan<char> suffix)
            {
                return suffix switch
                {
                    "bullseye" => string.Empty,
                    "alpine" => "-alpine",
                    _ => throw new InvalidOperationException("Invalid OS for PostGIS"),
                };
            }
        }
    }

    /// <summary>
    /// Adds a PostGIS resource to the application model. A container is used for local development. This version the package defaults to the 17-3.5 tag of the postgis container image.
    /// </summary>
    /// <param name="builder">The <see cref="IDistributedApplicationBuilder"/>.</param>
    /// <param name="name">The name of the resource. This name will be used as the connection string name when referenced in a dependency.</param>
    /// <param name="userName">The parameter used to provide the username for the PostGIS resource. If null a default value will be used.</param>
    /// <param name="password">The administrator password used for the container during local development. If null a random password will be generated.</param>
    /// <param name="port">The host port used when launching the container. If null a random port will be assigned.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<PostGisServerResource> AddPostGis(this IDistributedApplicationBuilder builder, string name, IResourceBuilder<ParameterResource>? userName = null, IResourceBuilder<ParameterResource>? password = null, int? port = null)
    {
        const string AuthMethod = "scram-sha-256";
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(name);

        var passwordParameter = password?.Resource ?? ParameterResourceBuilderExtensions.CreateDefaultPasswordParameter(builder, $"{name}-password");

        var postgisServer = new PostGisServerResource(name, userName?.Resource, passwordParameter);

        string? connectionString = null;

        builder.Eventing.Subscribe<ConnectionStringAvailableEvent>(postgisServer, async (_, ct) =>
        {
            connectionString = await postgisServer.GetConnectionStringAsync(ct).ConfigureAwait(false)
                ?? throw new DistributedApplicationException($"{nameof(ConnectionStringAvailableEvent)} was published for the '{postgisServer.Name}' resource but the connection string was null.");
        });

        builder.Eventing.Subscribe<ResourceReadyEvent>(postgisServer, async (@event, ct) =>
        {
            if (connectionString is null)
            {
                throw new DistributedApplicationException($"{nameof(ResourceReadyEvent)} was published for the '{postgisServer.Name}' resource but the connection string was null.");
            }

            // Non-database scoped connection string
            var npgsqlConnection = new Npgsql.NpgsqlConnection(connectionString + ";Database=postgres;");

            await using (npgsqlConnection.ConfigureAwait(false))
            {
                await npgsqlConnection.OpenAsync(ct).ConfigureAwait(false);

                if (npgsqlConnection.State != System.Data.ConnectionState.Open)
                {
                    throw new InvalidOperationException($"Could not open connection to '{postgisServer.Name}'");
                }

                foreach (var databaseName in postgisServer.Databases.Keys)
                {
                    if (builder.Resources.FirstOrDefault(n => string.Equals(n.Name, databaseName, StringComparison.OrdinalIgnoreCase)) is PostgresDatabaseResource postgreDatabase)
                    {
                        await CreateDatabaseAsync(npgsqlConnection, postgreDatabase, @event.Services, ct).ConfigureAwait(false);
                    }
                }
            }
        });

        var healthCheckKey = $"{name}_check";
        builder.Services.AddHealthChecks().AddNpgSql(
            _ => connectionString ?? throw new InvalidOperationException("Connection string is unavailable"),
            configure: connection => connection.ConnectionString += ";Database=postgres;",
            name: healthCheckKey);

        return builder.AddResource(postgisServer)
                      .WithEndpoint(port: port, targetPort: 5432, name: PostGisServerResource.PrimaryEndpointName) // Internal port is always 5432.
                      .WithImage(PostGis.PostGisContainerImageTags.Image, PostGis.PostGisContainerImageTags.Tag)
                      .WithImageRegistry(PostGis.PostGisContainerImageTags.Registry)
                      .WithEnvironment("POSTGRES_HOST_AUTH_METHOD", AuthMethod)
                      .WithEnvironment("POSTGRES_INITDB_ARGS", $"--auth-host={AuthMethod} --auth-local={AuthMethod}")
                      .WithEnvironment(context =>
                      {
                          context.EnvironmentVariables[UserEnvVarName] = postgisServer.UserNameReference;
                          context.EnvironmentVariables[PasswordEnvVarName] = postgisServer.PasswordParameter;
                      })
                      .WithHealthCheck(healthCheckKey);
    }

    private static async Task CreateDatabaseAsync(Npgsql.NpgsqlConnection npgsqlConnection, PostgresDatabaseResource npgsqlDatabase, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var scriptAnnotation = GetScripAnnotation(npgsqlDatabase);

        try
        {
            var quotedDatabaseIdentifier = new Npgsql.NpgsqlCommandBuilder().QuoteIdentifier(npgsqlDatabase.DatabaseName);
            var command = npgsqlConnection.CreateCommand();
            await using (command.ConfigureAwait(false))
            {
                command.CommandText = GetScript(scriptAnnotation) ?? $"CREATE DATABASE {quotedDatabaseIdentifier}";
                await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
        }
        catch (Npgsql.PostgresException p) when (p.SqlState is "42P04")
        {
            // Ignore the error if the database already exists.
        }
        catch (Exception e)
        {
            LogCreateDatabaseFailed(
                serviceProvider.GetRequiredService<ResourceLoggerService>().GetLogger(npgsqlDatabase.Parent),
                e,
                npgsqlDatabase.DatabaseName);
        }

        static object? GetScripAnnotation(PostgresDatabaseResource npgsqlDatabase)
        {
            return typeof(PostgresDatabaseResource).Assembly.GetType("Aspire.Hosting.Postgres.PostgresCreateDatabaseScriptAnnotation") is { } type
                ? npgsqlDatabase.Annotations.LastOrDefault(a => type.IsInstanceOfType(a))
                : default;
        }

        static string? GetScript(object? annotation)
        {
            return annotation?.GetType().GetProperty("Script")?.GetValue(annotation) as string;
        }
    }

    [LoggerMessage(LogLevel.Error, "Failed to create database '{DatabaseName}'")]
    private static partial void LogCreateDatabaseFailed(ILogger logger, Exception exception, string databaseName);
}