// -----------------------------------------------------------------------
// <copyright file="MinIOPublicApiTests.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting.MinIO.Tests;

public class MinIOPublicApiTests
{
    [Fact]
    public void AddMinIOContainerShouldThrowWhenBuilderIsNull()
    {
        IDistributedApplicationBuilder builder = null!;
        const string name = "minIO";

        IResourceBuilder<MinIOServerResource> Action()
        {
            return builder.AddMinIO(name);
        }

        var exception = Assert.Throws<ArgumentNullException>(Action);
        Assert.Equal(nameof(builder), exception.ParamName);
    }

    [Fact]
    public void AddMinIOContainerShouldThrowWhenNameIsNull()
    {
        var builder = DistributedApplication.CreateBuilder([]);
        const string name = null!;

        IResourceBuilder<MinIOServerResource> Action()
        {
            return builder.AddMinIO(name);
        }

        var exception = Assert.Throws<ArgumentNullException>(Action);
        Assert.Equal(nameof(name), exception.ParamName);
    }

    [Fact]
    public void WithDataVolumeShouldThrowWhenBuilderIsNull()
    {
        IResourceBuilder<MinIOServerResource> builder = null!;

        IResourceBuilder<MinIOServerResource> Action()
        {
            return builder.WithDataVolume();
        }

        var exception = Assert.Throws<ArgumentNullException>(Action);
        Assert.Equal(nameof(builder), exception.ParamName);
    }

    [Fact]
    public void WithDataBindMountShouldThrowWhenBuilderIsNull()
    {
        IResourceBuilder<MinIOServerResource> builder = null!;
        const string source = "/minIO/data";

        IResourceBuilder<MinIOServerResource> Action()
        {
            return builder.WithDataBindMount(source);
        }

        var exception = Assert.Throws<ArgumentNullException>(Action);
        Assert.Equal(nameof(builder), exception.ParamName);
    }

    [Fact]
    public void WithDataBindMountShouldThrowWhenSourceIsNull()
    {
        var builderResource = DistributedApplication.CreateBuilder();
        var minIO = builderResource.AddMinIO("minIO");
        const string source = null!;

        IResourceBuilder<MinIOServerResource> Action()
        {
            return minIO.WithDataBindMount(source);
        }

        var exception = Assert.Throws<ArgumentNullException>(Action);
        Assert.Equal(nameof(source), exception.ParamName);
    }

    [Fact]
    public void CtorMinIOServerResourceShouldThrowWhenNameIsNull()
    {
        var distributedApplicationBuilder = DistributedApplication.CreateBuilder([]);
        const string name = null!;
        var password = ParameterResourceBuilderExtensions.CreateDefaultPasswordParameter(distributedApplicationBuilder, "password", special: false);

        MinIOServerResource Action()
        {
            return new MinIOServerResource(name: name, userName: null, password: password, region: string.Empty);
        }

        var exception = Assert.Throws<ArgumentNullException>(Action);
        Assert.Equal(nameof(name), exception.ParamName);
    }

    [Fact]
    public void CtorMinIOServerResourceShouldThrowWhenPasswordIsNull()
    {
        const string name = "minIO";
        ParameterResource password = null!;

        MinIOServerResource Action()
        {
            return new MinIOServerResource(name: name, userName: null, password: password, region: string.Empty);
        }

        var exception = Assert.Throws<ArgumentNullException>(Action);
        Assert.Equal(nameof(password), exception.ParamName);
    }
}