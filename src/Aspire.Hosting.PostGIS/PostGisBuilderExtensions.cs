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
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(name);

        if (builder.AddPostgres(name, userName: userName, password: password, port: port) is { Resource: { } postgresServer })
        {
            _ = builder.Resources.Remove(postgresServer);

            // remove all the values
            var postgisServer = new ApplicationModel.PostGisServerResource(postgresServer.Name, postgresServer.UserNameParameter, postgresServer.PasswordParameter);

            var resourceBuilder = builder.AddResource(postgisServer);

            string? healthCheckKey = default;
            foreach (var annotation in postgresServer.Annotations)
            {
                if (annotation is ContainerImageAnnotation containerImageAnnotation)
                {
                    // update the container from POSTGRES to POSTGIS
                    containerImageAnnotation.Registry = PostGis.PostGisContainerImageTags.Registry;
                    containerImageAnnotation.Image = PostGis.PostGisContainerImageTags.Image;
                    containerImageAnnotation.Tag = PostGis.PostGisContainerImageTags.Tag;
                }

                if (annotation is HealthCheckAnnotation healthCheckAnnotation)
                {
                    healthCheckKey = healthCheckAnnotation.Key;
                }

                _ = resourceBuilder.WithAnnotation(annotation);
            }

            if (healthCheckKey is not null)
            {
                string? connectionString = null;

                _ = builder.Eventing.Subscribe<ConnectionStringAvailableEvent>(postgisServer, async (_, cancellationToken) =>
                {
                    connectionString = await postgisServer.GetConnectionStringAsync(cancellationToken).ConfigureAwait(false);

                    if (connectionString is null)
                    {
                        throw new DistributedApplicationException($"{nameof(ConnectionStringAvailableEvent)} was published for the '{postgisServer.Name}' resource but the connection string was null.");
                    }
                });

                // remove any before we add the new one
                _ = builder.Services.Configure<Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckServiceOptions>(options =>
                {
                    // remove the current health check
                    if (options.Registrations.FirstOrDefault(registration => string.Equals(registration.Name, healthCheckKey, StringComparison.OrdinalIgnoreCase)) is { } registration)
                    {
                        _ = options.Registrations.Remove(registration);
                    }
                });

                // add the new health check
                _ = builder.Services
                    .AddHealthChecks()
                    .AddNpgSql(
                        _ => connectionString ?? throw new InvalidOperationException("Connection string is unavailable"),
                        configure: (connection) => connection.ConnectionString += ";Database=postgres;",
                        name: healthCheckKey);
            }

            return resourceBuilder;
        }

        throw new InvalidOperationException();
    }
}