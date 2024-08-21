// -----------------------------------------------------------------------
// <copyright file="AddLocalStackTests.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting.LocalStack.Tests;

public class AddLocalStackTests
{
    [Fact]
    public void AddLocalStackGetsCorrectServiceNames()
    {
        var appBuilder = DistributedApplication.CreateBuilder();

        var localStack = appBuilder.AddLocalStack("localStack", services: LocalStackServices.Community.CloudWatch | LocalStackServices.Community.SimpleStorageService);

        Assert.Equal(["S3", "CloudWatch"], localStack.Resource.GetServiceNames());
    }
}