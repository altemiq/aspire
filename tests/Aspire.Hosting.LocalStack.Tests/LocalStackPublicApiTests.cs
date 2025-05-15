// -----------------------------------------------------------------------
// <copyright file="LocalStackPublicApiTests.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting.LocalStack.Tests;

using TUnit.Assertions.AssertConditions.Throws;

public class LocalStackPublicApiTests
{
    [Test]
    public async Task AddLocalStackContainerShouldThrowWhenBuilderIsNull()
    {
        IDistributedApplicationBuilder builder = null!;
        const string Name = "localStack";

        IResourceBuilder<LocalStackServerResource> Action()
        {
            return builder.AddLocalStack(Name);
        }

        _ = await Assert.That(Action).Throws<ArgumentNullException>().WithParameterName(nameof(builder));
    }

    [Test]
    public async Task AddLocalStackContainerShouldThrowWhenNameIsNull()
    {
        IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder([]);
        string name = null!;

        IResourceBuilder<LocalStackServerResource> Action()
        {
            return builder.AddLocalStack(name);
        }

        _ = await Assert.That(Action).Throws<ArgumentNullException>().WithParameterName(nameof(name));
    }

    [Test]
    public async Task WithDataVolumeShouldThrowWhenBuilderIsNull()
    {
        IResourceBuilder<LocalStackServerResource> builder = null!;

        IResourceBuilder<LocalStackServerResource> Action()
        {
            return builder.WithDataVolume();
        }

        _ = await Assert.That(Action).Throws<ArgumentNullException>().WithParameterName(nameof(builder));
    }

    [Test]
    public async Task WithDataBindMountShouldThrowWhenBuilderIsNull()
    {
        IResourceBuilder<LocalStackServerResource> builder = null!;
        const string Source = "/localStack/data";

        IResourceBuilder<LocalStackServerResource> Action()
        {
            return builder.WithDataBindMount(Source);
        }

        _ = await Assert.That(Action).Throws<ArgumentNullException>().WithParameterName(nameof(builder));
    }

    [Test]
    public async Task WithDataBindMountShouldThrowWhenSourceIsNull()
    {
        IDistributedApplicationBuilder builderResource = DistributedApplication.CreateBuilder();
        IResourceBuilder<LocalStackServerResource> localStack = builderResource.AddLocalStack("localStack");
        string source = null!;

        IResourceBuilder<LocalStackServerResource> Action()
        {
            return localStack.WithDataBindMount(source);
        }

        _ = await Assert.That(Action).Throws<ArgumentNullException>().WithParameterName(nameof(source));
    }

    [Test]
    public async Task CtorLocalStackServerResourceShouldThrowWhenNameIsNull()
    {
        string name = null!;

        LocalStackServerResource Action()
        {
            return new(name: name, region: string.Empty);
        }

        _ = await Assert.That(Action).Throws<ArgumentNullException>().WithParameterName(nameof(name));
    }
}