// -----------------------------------------------------------------------
// <copyright file="PostGisBuilderExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting;

extern alias core;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extensions for <c>PostGIS</c>.
/// </summary>
public static class PostGisBuilderExtensions
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
        where T : core::Aspire.Hosting.ApplicationModel.PostgresServerResource => builder.WithPostGis((PostGisVersion)(-1));

    /// <summary>
    /// Configures the Postgres container resource to enable the PostGIS extension.
    /// </summary>
    /// <typeparam name="T">The type of postgres container.</typeparam>
    /// <param name="builder">The <see cref="IDistributedApplicationBuilder"/>.</param>
    /// <param name="version">The version of PostGIS to insert.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<T> WithPostGis<T>(this IResourceBuilder<T> builder, PostGisVersion version)
        where T : core::Aspire.Hosting.ApplicationModel.PostgresServerResource
    {
        var tag = builder.Resource.TryGetLastAnnotation<ContainerImageAnnotation>(out var containerImage)
            && containerImage is { Tag: { } containerImageTag }
            ? GetTagCore(containerImageTag, version, System.Globalization.CultureInfo.InvariantCulture)
            : GetTag(PostgresVersion.V18, version);

        _ = builder
            .WithImage(PostGis.PostGisContainerImageTags.Image, tag)
            .WithImageRegistry(PostGis.PostGisContainerImageTags.Registry);

        return builder;

        static string GetTagCore(string tag, PostGisVersion postGisVersion, IFormatProvider? formatProvider)
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
                var postgresVersion = (int)double.Truncate(version) switch
                {
                    13 => PostgresVersion.V13,
                    14 => PostgresVersion.V14,
                    15 => PostgresVersion.V15,
                    16 => PostgresVersion.V16,
                    17 => PostgresVersion.V17,
                    18 => PostgresVersion.V18,
                    _ => throw new ArgumentOutOfRangeException(nameof(tag)),
                };

                prefix = GetTag(postgresVersion, postGisVersion);
                suffix = values is 1
                    ? string.Empty
                    : GetSuffix(tagSpan[ranges[1]]);
            }
            else
            {
                prefix = GetTag(PostgresVersion.V18, postGisVersion);
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
    /// <param name="postgresVersion">The major PostgreSQL version for the resource.</param>
    /// <param name="postGisVersion">The PostGIS version for the resource.</param>
    /// <param name="userName">The parameter used to provide the username for the PostGIS resource. If null a default value will be used.</param>
    /// <param name="password">The administrator password used for the container during local development. If null a random password will be generated.</param>
    /// <param name="port">The host port used when launching the container. If null a random port will be assigned.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<PostGisServerResource> AddPostGis(this IDistributedApplicationBuilder builder, string name, PostgresVersion postgresVersion, PostGisVersion postGisVersion, IResourceBuilder<ParameterResource>? userName = null, IResourceBuilder<ParameterResource>? password = null, int? port = null) =>
        builder.AddPostGis(name, userName, password, port)
            .WithImageTag(GetTag(postgresVersion, postGisVersion));

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
        ArgumentException.ThrowIfNullOrEmpty(name);

        var passwordParameter = password?.Resource ?? ParameterResourceBuilderExtensions.CreateDefaultPasswordParameter(builder, $"{name}-password");

        var postgisServer = new PostGisServerResource(name, userName?.Resource, passwordParameter);

        string? connectionString = null;

        builder.Eventing.Subscribe<ConnectionStringAvailableEvent>(postgisServer, async (_, ct) =>
            connectionString = await postgisServer.GetConnectionStringAsync(ct).ConfigureAwait(false)
                ?? throw new DistributedApplicationException($"{nameof(ConnectionStringAvailableEvent)} was published for the '{postgisServer.Name}' resource but the connection string was null."));

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

                if (npgsqlConnection.State is not System.Data.ConnectionState.Open)
                {
                    throw new InvalidOperationException($"Could not open connection to '{postgisServer.Name}'");
                }

                foreach (var databaseName in postgisServer.Databases.Keys)
                {
                    if (builder.Resources.FirstOrDefault(n => string.Equals(n.Name, databaseName, StringComparison.OrdinalIgnoreCase)) is core::Aspire.Hosting.ApplicationModel.PostgresDatabaseResource postgresDatabase)
                    {
                        await CreateDatabaseAsync(npgsqlConnection, postgresDatabase, @event.Services, ct).ConfigureAwait(false);
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

    private static string GetTag(PostgresVersion postgres, PostGisVersion postGis)
    {
        var postgresVersion = postgres switch
        {
            PostgresVersion.V13 => "13",
            PostgresVersion.V14 => "14",
            PostgresVersion.V15 => "15",
            PostgresVersion.V16 => "16",
            PostgresVersion.V17 => "17",
            PostgresVersion.V18 => "18",
            _ => throw new ArgumentOutOfRangeException(nameof(postgres), postgres, message: null),
        };

        if (!Enum.IsDefined(postGis))
        {
            // determine this from the best version that the postgres can handle
            postGis = postgres switch
            {
                PostgresVersion.V13 or PostgresVersion.V14 or PostgresVersion.V15 or PostgresVersion.V16 => PostGisVersion.V3_5,
                PostgresVersion.V17 or PostgresVersion.V18 => PostGisVersion.V3_6,
                _ => throw new ArgumentOutOfRangeException(nameof(postgres), postgres, message: null),
            };
        }

        var postGisVersion = postGis switch
        {
            PostGisVersion.V2_5 => "2.5",
            PostGisVersion.V3_0 => "3.0",
            PostGisVersion.V3_1 => "3.1",
            PostGisVersion.V3_2 => "3.2",
            PostGisVersion.V3_3 => "3.3",
            PostGisVersion.V3_4 => "3.4",
            PostGisVersion.V3_5 => "3.5",
            PostGisVersion.V3_6 => "3.6",
            _ => throw new ArgumentOutOfRangeException(nameof(postGis), postGis, message: null),
        };

        return $"{postgresVersion}-{postGisVersion}";
    }

    private static async Task CreateDatabaseAsync(Npgsql.NpgsqlConnection npgsqlConnection, core::Aspire.Hosting.ApplicationModel.PostgresDatabaseResource npgsqlDatabase, IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        if (GetCreateDatabaseAsyncMethod(typeof(Npgsql.NpgsqlConnection), typeof(core::Aspire.Hosting.ApplicationModel.PostgresDatabaseResource), typeof(IServiceProvider), typeof(CancellationToken)).Invoke(obj: null, [npgsqlConnection, npgsqlDatabase, serviceProvider, cancellationToken]) is Task task)
        {
            await task.ConfigureAwait(false);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3011:Reflection should not be used to increase accessibility of classes, methods, or fields", Justification = "Checked")]
        static System.Reflection.MethodInfo GetCreateDatabaseAsyncMethod(params Type[] types)
        {
            return typeof(core::Aspire.Hosting.PostgresBuilderExtensions)
               .GetMethod(
                   nameof(CreateDatabaseAsync),
                   System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic,
                   types)
                ?? throw new TypeLoadException();
        }
    }
}