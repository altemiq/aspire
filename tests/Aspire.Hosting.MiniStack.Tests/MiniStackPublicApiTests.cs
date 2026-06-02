// -----------------------------------------------------------------------
// <copyright file="MiniStackPublicApiTests.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting.MiniStack.Tests;

public class MiniStackPublicApiTests
{
    [Test]
    public async Task AddMiniStackContainerShouldThrowWhenBuilderIsNull()
    {
        IDistributedApplicationBuilder builder = null!;
        const string Name = "miniStack";

        IResourceBuilder<MiniStackServerResource> Action()
        {
            return builder.AddMiniStack(Name);
        }

        _ = await Assert.That(Action).Throws<ArgumentNullException>().WithParameterName(nameof(builder));
    }

    [Test]
    public async Task AddMiniStackContainerShouldThrowWhenNameIsNull()
    {
        IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder([]);
        string name = null!;

        IResourceBuilder<MiniStackServerResource> Action()
        {
            return builder.AddMiniStack(name);
        }

        _ = await Assert.That(Action).Throws<ArgumentNullException>().WithParameterName(nameof(name));
    }

    [Test]
    public async Task WithDataVolumeShouldThrowWhenBuilderIsNull()
    {
        IResourceBuilder<MiniStackServerResource> builder = null!;

        IResourceBuilder<MiniStackServerResource> Action()
        {
            return builder.WithDataVolume();
        }

        _ = await Assert.That(Action).Throws<ArgumentNullException>().WithParameterName(nameof(builder));
    }

    [Test]
    public async Task WithDataBindMountShouldThrowWhenBuilderIsNull()
    {
        IResourceBuilder<MiniStackServerResource> builder = null!;
        const string Source = "/miniStack/data";

        IResourceBuilder<MiniStackServerResource> Action()
        {
            return builder.WithDataBindMount(Source);
        }

        _ = await Assert.That(Action).Throws<ArgumentNullException>().WithParameterName(nameof(builder));
    }

    [Test]
    public async Task WithDataBindMountShouldThrowWhenSourceIsNull()
    {
        IDistributedApplicationBuilder builderResource = DistributedApplication.CreateBuilder();
        IResourceBuilder<MiniStackServerResource> miniStack = builderResource.AddMiniStack("miniStack");
        string source = null!;

        IResourceBuilder<MiniStackServerResource> Action()
        {
            return miniStack.WithDataBindMount(source);
        }

        _ = await Assert.That(Action).Throws<ArgumentNullException>().WithParameterName(nameof(source));
    }

    [Test]
    public async Task CtorMiniStackServerResourceShouldThrowWhenNameIsNull()
    {
        string name = null!;

        MiniStackServerResource Action()
        {
            return new(name: name, region: string.Empty);
        }

        _ = await Assert.That(Action).Throws<ArgumentNullException>().WithParameterName(nameof(name));
    }
}