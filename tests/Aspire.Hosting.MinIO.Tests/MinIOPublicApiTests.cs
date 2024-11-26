// -----------------------------------------------------------------------
// <copyright file="MinIOPublicApiTests.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting.MinIO.Tests;

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

        await Assert.That(Action).ThrowsExactly<ArgumentNullException>().And.HasMember(x => x.ParamName).EqualTo(nameof(builder));
    }

    [Test]
    public async Task AddMinIOContainerShouldThrowWhenNameIsNull()
    {
        var builder = DistributedApplication.CreateBuilder([]);
        const string name = null!;

        IResourceBuilder<MinIOServerResource> Action()
        {
            return builder.AddMinIO(name);
        }

        await Assert.That(Action).ThrowsExactly<ArgumentNullException>().And.HasMember(x => x.ParamName).EqualTo(nameof(name));
    }

    [Test]
    public async Task WithDataVolumeShouldThrowWhenBuilderIsNull()
    {
        IResourceBuilder<MinIOServerResource> builder = null!;

        IResourceBuilder<MinIOServerResource> Action()
        {
            return builder.WithDataVolume();
        }

        await Assert.That(Action).ThrowsExactly<ArgumentNullException>().And.HasMember(x => x.ParamName).EqualTo(nameof(builder));
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

        await Assert.That(Action).ThrowsExactly<ArgumentNullException>().And.HasMember(x => x.ParamName).EqualTo(nameof(builder));
    }

    [Test]
    public async Task WithDataBindMountShouldThrowWhenSourceIsNull()
    {
        var builderResource = DistributedApplication.CreateBuilder();
        var minIO = builderResource.AddMinIO("minIO");
        const string source = null!;

        IResourceBuilder<MinIOServerResource> Action()
        {
            return minIO.WithDataBindMount(source);
        }

        await Assert.That(Action).ThrowsExactly<ArgumentNullException>().And.HasMember(x => x.ParamName).EqualTo(nameof(source));
    }

    [Test]
    public async Task CtorMinIOServerResourceShouldThrowWhenNameIsNull()
    {
        var distributedApplicationBuilder = DistributedApplication.CreateBuilder([]);
        const string name = null!;
        var password = ParameterResourceBuilderExtensions.CreateDefaultPasswordParameter(distributedApplicationBuilder, "password", special: false);

        MinIOServerResource Action()
        {
            return new MinIOServerResource(name: name, userName: null, password: password, region: string.Empty);
        }

        await Assert.That(Action).ThrowsExactly<ArgumentNullException>().And.HasMember(x => x.ParamName).EqualTo(nameof(name));
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

        await Assert.That(Action).ThrowsExactly<ArgumentNullException>().And.HasMember(x => x.ParamName).EqualTo(nameof(password));
    }
}