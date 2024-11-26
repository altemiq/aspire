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
        var appBuilder = DistributedApplication.CreateBuilder();

        var minio = appBuilder.AddMinIO("minio");

        await Assert.That(minio.Resource.PasswordParameter.Default?.GetType().FullName).IsEqualTo("Aspire.Hosting.ApplicationModel.UserSecretsParameterDefault");
    }

    [Test]
    public async Task AddMinIODoesNotAddGeneratedPasswordParameterWithUserSecretsParameterDefaultInPublishMode()
    {
        var appBuilder = DistributedApplication.CreateBuilder(["Publishing:Publisher=manifest"]);

        var minio = appBuilder.AddMinIO("minio");

        await Assert.That(minio.Resource.PasswordParameter.Default?.GetType().FullName).IsNotEqualTo("Aspire.Hosting.ApplicationModel.UserSecretsParameterDefault");
    }
}