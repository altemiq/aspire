// -----------------------------------------------------------------------
// <copyright file="MinIOPublicApiTests.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting.MinIO.Tests;

using TUnit.Assertions.AssertConditions.Throws;

public class MinIOPublicApiTests
{
    [Test]
    public async Task AddMinIOContainerShouldThrowWhenBuilderIsNull()
    {
        IDistributedApplicationBuilder builder = null!;
        const string name = "minIO";

        IResourceBuilder<MinIOServerResource> Action()
        {
            return builder.AddMinIO(name);
        }

        _ = await Assert.That(Action).Throws<ArgumentNullException>().WithParameterName(nameof(builder));
    }

    [Test]
    public async Task AddMinIOContainerShouldThrowWhenNameIsNull()
    {
        IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder([]);
        const string name = null!;

        IResourceBuilder<MinIOServerResource> Action()
        {
            return builder.AddMinIO(name);
        }

        _ = await Assert.That(Action).Throws<ArgumentNullException>().WithParameterName(nameof(name));
    }

    [Test]
    public async Task WithDataVolumeShouldThrowWhenBuilderIsNull()
    {
        IResourceBuilder<MinIOServerResource> builder = null!;

        IResourceBuilder<MinIOServerResource> Action()
        {
            return builder.WithDataVolume();
        }

        _ = await Assert.That(Action).Throws<ArgumentNullException>().WithParameterName(nameof(builder));
    }

    [Test]
    public async Task WithDataBindMountShouldThrowWhenBuilderIsNull()
    {
        IResourceBuilder<MinIOServerResource> builder = null!;
        const string source = "/minIO/data";

        IResourceBuilder<MinIOServerResource> Action()
        {
            return builder.WithDataBindMount(source);
        }

        _ = await Assert.That(Action).Throws<ArgumentNullException>().WithParameterName(nameof(builder));
    }

    [Test]
    public async Task WithDataBindMountShouldThrowWhenSourceIsNull()
    {
        IDistributedApplicationBuilder builderResource = DistributedApplication.CreateBuilder();
        IResourceBuilder<MinIOServerResource> minIO = builderResource.AddMinIO("minIO");
        const string source = null!;

        IResourceBuilder<MinIOServerResource> Action()
        {
            return minIO.WithDataBindMount(source);
        }

        _ = await Assert.That(Action).Throws<ArgumentNullException>().WithParameterName(nameof(source));
    }

    [Test]
    public async Task CtorMinIOServerResourceShouldThrowWhenNameIsNull()
    {
        IDistributedApplicationBuilder distributedApplicationBuilder = DistributedApplication.CreateBuilder([]);
        const string name = null!;
        ParameterResource password = ParameterResourceBuilderExtensions.CreateDefaultPasswordParameter(distributedApplicationBuilder, "password", special: false);

        MinIOServerResource Action()
        {
            return new MinIOServerResource(name: name, userName: null, password: password, region: string.Empty);
        }

        _ = await Assert.That(Action).Throws<ArgumentNullException>().WithParameterName(nameof(name));
    }

    [Test]
    public async Task CtorMinIOServerResourceShouldThrowWhenPasswordIsNull()
    {
        const string name = "minIO";
        ParameterResource password = null!;

        MinIOServerResource Action()
        {
            return new MinIOServerResource(name: name, userName: null, password: password, region: string.Empty);
        }

        _ = await Assert.That(Action).Throws<ArgumentNullException>().WithParameterName(nameof(password));
    }
}