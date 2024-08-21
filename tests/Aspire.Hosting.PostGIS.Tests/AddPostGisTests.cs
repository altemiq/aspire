// -----------------------------------------------------------------------
// <copyright file="AddPostGisTests.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting.PostGIS.Tests;

public class AddPostGisTests
{
    [Fact]
    public void AddPostGisAddsGeneratedPasswordParameterWithUserSecretsParameterDefaultInRunMode()
    {
        var appBuilder = DistributedApplication.CreateBuilder();

        var postGis = appBuilder.AddPostGis("postgis");

        Assert.Equal("Aspire.Hosting.ApplicationModel.UserSecretsParameterDefault", postGis.Resource.PasswordParameter.Default?.GetType().FullName);
    }

    [Fact]
    public void AddPostGisDoesNotAddGeneratedPasswordParameterWithUserSecretsParameterDefaultInPublishMode()
    {
        var appBuilder = DistributedApplication.CreateBuilder(["Publishing:Publisher=manifest"]);

        var postGis = appBuilder.AddPostGis("postgis");

        Assert.NotEqual("Aspire.Hosting.ApplicationModel.UserSecretsParameterDefault", postGis.Resource.PasswordParameter.Default?.GetType().FullName);
    }

    [Fact]
    public void AddPostGisChangesTheImage()
    {
        var appBuilder = DistributedApplication.CreateBuilder();

        var postGis = appBuilder.AddPostGis("postgis");

        Assert.Equal("postgis/postgis", postGis.Resource.Annotations.OfType<ContainerImageAnnotation>().Single().Image);
    }
}