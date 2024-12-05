// -----------------------------------------------------------------------
// <copyright file="AddMinIOTests.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting.MinIO.Tests;

public class AddMinIOTests
{
    [Test]
    public async Task AddMinIOAddsGeneratedPasswordParameterWithUserSecretsParameterDefaultInRunMode()
    {
        IDistributedApplicationBuilder appBuilder = DistributedApplication.CreateBuilder();

        IResourceBuilder<MinIOServerResource> minio = appBuilder.AddMinIO("minio");

        _ = await Assert.That(minio.Resource.PasswordParameter.Default?.GetType().FullName).IsEqualTo("Aspire.Hosting.ApplicationModel.UserSecretsParameterDefault");
    }

    [Test]
    public async Task AddMinIODoesNotAddGeneratedPasswordParameterWithUserSecretsParameterDefaultInPublishMode()
    {
        IDistributedApplicationBuilder appBuilder = DistributedApplication.CreateBuilder(["Publishing:Publisher=manifest"]);

        IResourceBuilder<MinIOServerResource> minio = appBuilder.AddMinIO("minio");

        _ = await Assert.That(minio.Resource.PasswordParameter.Default?.GetType().FullName).IsNotEqualTo("Aspire.Hosting.ApplicationModel.UserSecretsParameterDefault");
    }
}