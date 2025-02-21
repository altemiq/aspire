// -----------------------------------------------------------------------
// <copyright file="AddLocalStackTests.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting.LocalStack.Tests;

public class AddLocalStackTests
{
    [Test]
    public async Task AddLocalStackGetsCorrectServiceNames()
    {
        IDistributedApplicationBuilder appBuilder = DistributedApplication.CreateBuilder();

        IResourceBuilder<LocalStackServerResource> localStack = appBuilder.AddLocalStack("localStack", services: LocalStackServices.Community.CloudWatch | LocalStackServices.Community.SimpleStorageService);

        _ = await Assert.That(localStack.Resource.GetServiceNames()).IsEquivalentTo(["s3", "cloudwatch"]);
    }
}