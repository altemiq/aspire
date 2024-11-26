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
        var appBuilder = DistributedApplication.CreateBuilder();

        var localStack = appBuilder.AddLocalStack("localStack", services: LocalStackServices.Community.CloudWatch | LocalStackServices.Community.SimpleStorageService);

        await Assert.That(localStack.Resource.GetServiceNames()).IsEquivalentTo(["S3", "CloudWatch"]);
    }
}