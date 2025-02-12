// -----------------------------------------------------------------------
// <copyright file="PostGisBuilderExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting;

using Aspire.Hosting.ApplicationModel;
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
        where T : PostgresServerResource
    {
        var tag = PostGis.PostGisContainerImageTags.Tag;
        if (builder.Resource.TryGetLastAnnotation<ContainerImageAnnotation>(out var containerImage)
            && containerImage is { Tag: { } containerImageTag })
        {
            tag = GetTag(containerImageTag, System.Globalization.CultureInfo.InvariantCulture);
        }

        builder.WithImage(PostGis.PostGisContainerImageTags.Image, tag)
               .WithImageRegistry(PostGis.PostGisContainerImageTags.Registry);

        return builder;

        static string GetTag(string tag, IFormatProvider? formatProvider)
        {
            if (tag is "latest")
            {
                return tag;
            }

            var split = tag.Split('-');
            string prefix;
            string suffix;
            if (double.TryParse(split[0], formatProvider, out var version))
            {
                prefix = $"{double.Truncate(version).ToString(formatProvider)}-{PostGis.PostGisContainerImageTags.PostGisTag}";
                suffix = split switch
                {
                    { Length: 1 } or [_, "bullseye"] => string.Empty,
                    [_, "alpine"] => "-alpine",
                    _ => throw new InvalidOperationException("Invalid OS for PostGIS"),
                };
            }
            else
            {
                prefix = PostGis.PostGisContainerImageTags.Tag;
                suffix = split[0] switch
                {
                    "bullseye" => string.Empty,
                    "alpine" => "-alpine",
                    _ => throw new InvalidOperationException("Invalid OS for PostGIS"),
                };
            }

            return prefix + suffix;
        }
    }

    /// <summary>
    /// Adds a PostGIS resource to the application model. A container is used for local development. This version the package defaults to the 17-3.5 tag of the postgis container image.
    /// </summary>
    /// <param name="builder">The <see cref="IDistributedApplicationBuilder"/>.</param>
    /// <param name="name">The name of the resource. This name will be used as the connection string name when referenced in a dependency.</param>
    /// <param name="userName">The parameter used to provide the user name for the PostGIS resource. If null a default value will be used.</param>
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

        _ = builder.Eventing.Subscribe<ConnectionStringAvailableEvent>(postgisServer, async (_, ct) =>
        {
            connectionString = await postgisServer.GetConnectionStringAsync(ct).ConfigureAwait(false);

            if (connectionString is null)
            {
                throw new DistributedApplicationException($"{nameof(ConnectionStringAvailableEvent)} was published for the '{postgisServer.Name}' resource but the connection string was null.");
            }
        });

        var healthCheckKey = $"{name}_check";
        _ = builder.Services.AddHealthChecks().AddNpgSql(
            _ => connectionString ?? throw new InvalidOperationException("Connection string is unavailable"),
            configure: (connection) => connection.ConnectionString += ";Database=postgres;",
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
}