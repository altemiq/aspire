// -----------------------------------------------------------------------
// <copyright file="AddPostGisTests.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting.PostGIS.Tests;

public class AddPostGisTests
{
    [Test]
    public async Task AddPostGisAddsGeneratedPasswordParameterWithUserSecretsParameterDefaultInRunMode()
    {
        IDistributedApplicationBuilder appBuilder = DistributedApplication.CreateBuilder();

        IResourceBuilder<PostGisServerResource> postGis = appBuilder.AddPostGis("postgis");

        _ = await Assert.That(postGis.Resource.PasswordParameter.Default?.GetType().FullName).IsEqualTo("Aspire.Hosting.ApplicationModel.UserSecretsParameterDefault");
    }

    [Test]
    public async Task AddPostGisDoesNotAddGeneratedPasswordParameterWithUserSecretsParameterDefaultInPublishMode()
    {
        IDistributedApplicationBuilder appBuilder = DistributedApplication.CreateBuilder(["--publisher", "manifest"]);

        IResourceBuilder<PostGisServerResource> postGis = appBuilder.AddPostGis("postgis");

        _ = await Assert.That(postGis.Resource.PasswordParameter.Default?.GetType().FullName).IsNotEqualTo("Aspire.Hosting.ApplicationModel.UserSecretsParameterDefault");
    }

    [Test]
    public async Task AddPostGisChangesTheImage()
    {
        IDistributedApplicationBuilder appBuilder = DistributedApplication.CreateBuilder();

        IResourceBuilder<PostGisServerResource> postGis = appBuilder.AddPostGis("postgis");

        _ = await Assert.That(postGis.Resource.TryGetLastAnnotation<ContainerImageAnnotation>(out ContainerImageAnnotation? annotation)).IsTrue();
        _ = await Assert.That(annotation?.Image).IsEqualTo("postgis/postgis");
    }
}