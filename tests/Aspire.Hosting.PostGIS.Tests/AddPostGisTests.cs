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
        var appBuilder = DistributedApplication.CreateBuilder();

        var postGis = appBuilder.AddPostGis("postgis");

        await Assert.That(postGis.Resource.PasswordParameter.Default?.GetType().FullName).IsEqualTo("Aspire.Hosting.ApplicationModel.UserSecretsParameterDefault");
    }

    [Test]
    public async Task AddPostGisDoesNotAddGeneratedPasswordParameterWithUserSecretsParameterDefaultInPublishMode()
    {
        var appBuilder = DistributedApplication.CreateBuilder(["--publisher", "manifest"]);

        var postGis = appBuilder.AddPostGis("postgis");

        await Assert.That(postGis.Resource.PasswordParameter.Default?.GetType().FullName).IsNotEqualTo("Aspire.Hosting.ApplicationModel.UserSecretsParameterDefault");
    }

    [Test]
    public async Task AddPostGisChangesTheImage()
    {
        var appBuilder = DistributedApplication.CreateBuilder();

        var postGis = appBuilder.AddPostGis("postgis");

        await Assert.That(postGis.Resource.Annotations.OfType<ContainerImageAnnotation>().Single().Image).IsEqualTo("postgis/postgis");
    }
}