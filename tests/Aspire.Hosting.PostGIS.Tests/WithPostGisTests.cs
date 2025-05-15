// -----------------------------------------------------------------------
// <copyright file="WithPostGisTests.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting.PostGIS.Tests;

using TUnit.Assertions.AssertConditions.Throws;

public class WithPostGisTests
{
    [Test]
    public async Task WithPostGisChangesTheImage()
    {
        IDistributedApplicationBuilder appBuilder = DistributedApplication.CreateBuilder();

        IResourceBuilder<PostgresServerResource> postgres = appBuilder.AddPostgres("postgis");

        _ = await Assert.That(postgres.Resource.TryGetLastAnnotation(out ContainerImageAnnotation? annotation)).IsTrue();
        _ = await Assert.That(annotation!.Image).IsNotEqualTo("postgis/postgis");

        _ = postgres.WithPostGis();

        _ = await Assert.That(postgres.Resource.TryGetLastAnnotation(out annotation)).IsTrue();
        _ = await Assert.That(annotation?.Image).IsEqualTo("postgis/postgis");
    }

    [Test]
    [Arguments("17.2", $"17-{PostGis.PostGisContainerImageTags.PostGisTag}")]
    [Arguments("16.2", $"16-{PostGis.PostGisContainerImageTags.PostGisTag}")]
    [Arguments("15.2", $"15-{PostGis.PostGisContainerImageTags.PostGisTag}")]
    [Arguments("14.2", $"14-{PostGis.PostGisContainerImageTags.PostGisTag}")]
    [Arguments("13.2", $"13-{PostGis.PostGisContainerImageTags.PostGisTag}")]
    [Arguments("17-bullseye", $"17-{PostGis.PostGisContainerImageTags.PostGisTag}")]
    [Arguments("16-bullseye", $"16-{PostGis.PostGisContainerImageTags.PostGisTag}")]
    [Arguments("15-bullseye", $"15-{PostGis.PostGisContainerImageTags.PostGisTag}")]
    [Arguments("14-bullseye", $"14-{PostGis.PostGisContainerImageTags.PostGisTag}")]
    [Arguments("13-bullseye", $"13-{PostGis.PostGisContainerImageTags.PostGisTag}")]
    [Arguments("bullseye", PostGis.PostGisContainerImageTags.Tag)]
    [Arguments("17-alpine", $"17-{PostGis.PostGisContainerImageTags.PostGisTag}-alpine")]
    [Arguments("16-alpine", $"16-{PostGis.PostGisContainerImageTags.PostGisTag}-alpine")]
    [Arguments("15-alpine", $"15-{PostGis.PostGisContainerImageTags.PostGisTag}-alpine")]
    [Arguments("14-alpine", $"14-{PostGis.PostGisContainerImageTags.PostGisTag}-alpine")]
    [Arguments("13-alpine", $"13-{PostGis.PostGisContainerImageTags.PostGisTag}-alpine")]
    [Arguments("alpine", $"{PostGis.PostGisContainerImageTags.Tag}-alpine")]
    [Arguments("latest", "latest")]
    public async Task WithPostGisChangesTheTag(string postgresTag, string postgisTag)
    {
        IDistributedApplicationBuilder appBuilder = DistributedApplication.CreateBuilder();

        IResourceBuilder<PostgresServerResource> postgres = appBuilder.AddPostgres("postgis")
            .WithImageTag(postgresTag)
            .WithPostGis();

        _ = await Assert.That(postgres.Resource.TryGetLastAnnotation(out ContainerImageAnnotation? annotation)).IsTrue();
        _ = await Assert.That(annotation?.Tag).IsEqualTo(postgisTag);
    }

    [Test]
    [Arguments("17.2-bookworm")]
    [Arguments("17-bookworm")]
    [Arguments("bookworm⁠")]
    public async Task WithPostGisFailsOnIncorrectOS(string postgresTag)
    {
        IDistributedApplicationBuilder appBuilder = DistributedApplication.CreateBuilder();

        IResourceBuilder<PostgresServerResource> postgres = appBuilder.AddPostgres("postgis").WithImageTag(postgresTag);

        _ = await Assert.That(() => postgres.WithPostGis()).Throws<InvalidOperationException>();
    }
}
