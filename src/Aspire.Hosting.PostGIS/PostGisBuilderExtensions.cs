// -----------------------------------------------------------------------
// <copyright file="PostGisBuilderExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting;

using Aspire.Hosting.ApplicationModel;

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

        if (builder.AddPostgres(name, userName: userName, password: password, port: port) is { Resource: { } postgresResource })
        {
            builder.Resources.Remove(postgresResource);

            // remove all the values
            var postgis = new ApplicationModel.PostGisServerResource(postgresResource.Name, postgresResource.UserNameParameter, postgresResource.PasswordParameter);

            var resourceBuilder = builder.AddResource(postgis);

            foreach (var annotation in postgresResource.Annotations)
            {
                if (annotation is ContainerImageAnnotation containerImageAnnotation)
                {
                    // update the container from POSTGRES to POSTGIS
                    containerImageAnnotation.Registry = PostGis.PostGisContainerImageTags.Registry;
                    containerImageAnnotation.Image = PostGis.PostGisContainerImageTags.Image;
                    containerImageAnnotation.Tag = PostGis.PostGisContainerImageTags.Tag;
                }

                resourceBuilder.WithAnnotation(annotation);
            }

            return resourceBuilder;
        }

        throw new InvalidOperationException();
    }
}