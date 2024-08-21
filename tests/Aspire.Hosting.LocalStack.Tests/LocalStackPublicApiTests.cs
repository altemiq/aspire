// -----------------------------------------------------------------------
// <copyright file="LocalStackPublicApiTests.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting.LocalStack.Tests;

public class LocalStackPublicApiTests
{
    [Fact]
    public void AddLocalStackContainerShouldThrowWhenBuilderIsNull()
    {
        IDistributedApplicationBuilder builder = null!;
        const string name = "localStack";

        IResourceBuilder<LocalStackServerResource> Action()
        {
            return builder.AddLocalStack(name);
        }

        var exception = Assert.Throws<ArgumentNullException>(Action);
        Assert.Equal(nameof(builder), exception.ParamName);
    }

    [Fact]
    public void AddLocalStackContainerShouldThrowWhenNameIsNull()
    {
        var builder = DistributedApplication.CreateBuilder([]);
        const string name = null!;

        IResourceBuilder<LocalStackServerResource> Action()
        {
            return builder.AddLocalStack(name);
        }

        var exception = Assert.Throws<ArgumentNullException>(Action);
        Assert.Equal(nameof(name), exception.ParamName);
    }

    [Fact]
    public void WithDataVolumeShouldThrowWhenBuilderIsNull()
    {
        IResourceBuilder<LocalStackServerResource> builder = null!;

        IResourceBuilder<LocalStackServerResource> Action()
        {
            return builder.WithDataVolume();
        }

        var exception = Assert.Throws<ArgumentNullException>(Action);
        Assert.Equal(nameof(builder), exception.ParamName);
    }

    [Fact]
    public void WithDataBindMountShouldThrowWhenBuilderIsNull()
    {
        IResourceBuilder<LocalStackServerResource> builder = null!;
        const string source = "/localStack/data";

        IResourceBuilder<LocalStackServerResource> Action()
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
        var localStack = builderResource.AddLocalStack("localStack");
        const string source = null!;

        IResourceBuilder<LocalStackServerResource> Action()
        {
            return localStack.WithDataBindMount(source);
        }

        var exception = Assert.Throws<ArgumentNullException>(Action);
        Assert.Equal(nameof(source), exception.ParamName);
    }

    [Fact]
    public void CtorLocalStackServerResourceShouldThrowWhenNameIsNull()
    {
        const string name = null!;

        static LocalStackServerResource Action()
        {
            return new LocalStackServerResource(name: name, region: string.Empty);
        }

        var exception = Assert.Throws<ArgumentNullException>(Action);
        Assert.Equal(nameof(name), exception.ParamName);
    }
}