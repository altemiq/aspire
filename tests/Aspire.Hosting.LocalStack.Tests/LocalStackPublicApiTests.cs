// -----------------------------------------------------------------------
// <copyright file="LocalStackPublicApiTests.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting.LocalStack.Tests;

public class LocalStackPublicApiTests
{
    [Test]
    public async Task AddLocalStackContainerShouldThrowWhenBuilderIsNull()
    {
        IDistributedApplicationBuilder builder = null!;
        const string name = "localStack";

        IResourceBuilder<LocalStackServerResource> Action()
        {
            return builder.AddLocalStack(name);
        }

        await Assert.That(Action).ThrowsExactly<ArgumentNullException>().And.HasMember(x => x.ParamName).EqualTo(nameof(builder));
    }

    [Test]
    public async Task AddLocalStackContainerShouldThrowWhenNameIsNull()
    {
        var builder = DistributedApplication.CreateBuilder([]);
        const string name = null!;

        IResourceBuilder<LocalStackServerResource> Action()
        {
            return builder.AddLocalStack(name);
        }

        await Assert.That(Action).ThrowsExactly<ArgumentNullException>().And.HasMember(x => x.ParamName).EqualTo(nameof(name));
    }

    [Test]
    public async Task WithDataVolumeShouldThrowWhenBuilderIsNull()
    {
        IResourceBuilder<LocalStackServerResource> builder = null!;

        IResourceBuilder<LocalStackServerResource> Action()
        {
            return builder.WithDataVolume();
        }

        await Assert.That(Action).ThrowsExactly<ArgumentNullException>().And.HasMember(x => x.ParamName).EqualTo(nameof(builder));
    }

    [Test]
    public async Task WithDataBindMountShouldThrowWhenBuilderIsNull()
    {
        IResourceBuilder<LocalStackServerResource> builder = null!;
        const string source = "/localStack/data";

        IResourceBuilder<LocalStackServerResource> Action()
        {
            return builder.WithDataBindMount(source);
        }

        await Assert.That(Action).ThrowsExactly<ArgumentNullException>().And.HasMember(x => x.ParamName).EqualTo(nameof(builder));
    }

    [Test]
    public async Task WithDataBindMountShouldThrowWhenSourceIsNull()
    {
        var builderResource = DistributedApplication.CreateBuilder();
        var localStack = builderResource.AddLocalStack("localStack");
        const string source = null!;

        IResourceBuilder<LocalStackServerResource> Action()
        {
            return localStack.WithDataBindMount(source);
        }

        await Assert.That(Action).ThrowsExactly<ArgumentNullException>().And.HasMember(x => x.ParamName).EqualTo(nameof(source));
    }

    [Test]
    public async Task CtorLocalStackServerResourceShouldThrowWhenNameIsNull()
    {
        const string name = null!;

        static LocalStackServerResource Action()
        {
            return new LocalStackServerResource(name: name, region: string.Empty);
        }

        await Assert.That(Action).ThrowsExactly<ArgumentNullException>().And.HasMember(x => x.ParamName).EqualTo(nameof(name));
    }
}