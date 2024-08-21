// -----------------------------------------------------------------------
// <copyright file="PostGisPublicApiTests.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting.PostGIS.Tests;

public class PostGISPublicApiTests
{
    [Fact]
    public void AddPostGISContainerShouldThrowWhenBuilderIsNull()
    {
        IDistributedApplicationBuilder builder = null!;
        const string name = "postGIS";

        IResourceBuilder<PostgresServerResource> Action()
        {
            return builder.AddPostGis(name);
        }

        var exception = Assert.Throws<ArgumentNullException>(Action);
        Assert.Equal(nameof(builder), exception.ParamName);
    }

    [Fact]
    public void AddPostGISContainerShouldThrowWhenNameIsNull()
    {
        var builder = DistributedApplication.CreateBuilder([]);
        const string name = null!;

        IResourceBuilder<PostgresServerResource> Action()
        {
            return builder.AddPostGis(name);
        }

        var exception = Assert.Throws<ArgumentNullException>(Action);
        Assert.Equal(nameof(name), exception.ParamName);
    }
}