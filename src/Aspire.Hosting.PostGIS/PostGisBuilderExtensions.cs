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
    /// Adds a PostGIS resource to the application model. A container is used for local development. This version the package defaults to the 16-3.4 tag of the postgis container image.
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

        builder.Eventing.Subscribe<ConnectionStringAvailableEvent>(postgisServer, async (_, ct) =>
        {
            connectionString = await postgisServer.GetConnectionStringAsync(ct).ConfigureAwait(false);

            if (connectionString is null)
            {
                throw new DistributedApplicationException($"{nameof(ConnectionStringAvailableEvent)} was published for the '{postgisServer.Name}' resource but the connection string was null.");
            }
        });

        var healthCheckKey = $"{name}_check";
        builder.Services.AddHealthChecks().AddNpgSql(
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