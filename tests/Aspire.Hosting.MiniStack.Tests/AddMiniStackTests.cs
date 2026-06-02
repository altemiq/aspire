// -----------------------------------------------------------------------
// <copyright file="AddMiniStackTests.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting.MiniStack.Tests;

public class AddMiniStackTests
{
    [Test]
    public async Task AddMiniStackGetsCorrectServiceNames()
    {
        IDistributedApplicationBuilder appBuilder = DistributedApplication.CreateBuilder();

        IResourceBuilder<MiniStackServerResource> miniStack = appBuilder.AddMiniStack("miniStack", services: MiniStackServices.EventBridge | MiniStackServices.SimpleStorageService);

        _ = await Assert.That(miniStack.Resource.GetServiceNames()).IsEquivalentTo(["s3", "events"]);
    }
}