// -----------------------------------------------------------------------
// <copyright file="PostGisPublicApiTests.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting.PostGIS.Tests;

using TUnit.Assertions.AssertConditions.Throws;

public class PostGISPublicApiTests
{
    [Test]
    public async Task AddPostGISContainerShouldThrowWhenBuilderIsNull()
    {
        IDistributedApplicationBuilder builder = null!;
        const string name = "postGIS";

        IResourceBuilder<PostgresServerResource> Action()
        {
            return builder.AddPostGis(name);
        }

        _ = await Assert.That(Action)
            .ThrowsExactly<ArgumentNullException>()
            .WithParameterName(nameof(builder));
    }

    [Test]
    public async Task AddPostGISContainerShouldThrowWhenNameIsNull()
    {
        IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder([]);
        const string name = null!;

        IResourceBuilder<PostgresServerResource> Action()
        {
            return builder.AddPostGis(name);
        }

        _ = await Assert.That(Action)
            .ThrowsExactly<ArgumentNullException>()
            .WithParameterName(nameof(name));
    }
}