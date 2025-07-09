// -----------------------------------------------------------------------
// <copyright file="PostGisPublicApiTests.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting.PostGIS.Tests;

using TUnit.Assertions.AssertConditions.Throws;

public class PostGisPublicApiTests
{
    [Test]
    public async Task AddPostGisContainerShouldThrowWhenBuilderIsNull()
    {
        IDistributedApplicationBuilder builder = null!;
        const string Name = "postGIS";

        IResourceBuilder<PostgresServerResource> Action()
        {
            return builder.AddPostGis(Name);
        }

        _ = await Assert.That(Action)
            .Throws<ArgumentNullException>()
            .WithParameterName(nameof(builder));
    }

    [Test]
    public async Task AddPostGisContainerShouldThrowWhenNameIsNull()
    {
        IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder([]);
        string name = null!;

        IResourceBuilder<PostgresServerResource> Action()
        {
            return builder.AddPostGis(name);
        }

        _ = await Assert.That(Action)
            .Throws<ArgumentNullException>()
            .WithParameterName(nameof(name));
    }
}