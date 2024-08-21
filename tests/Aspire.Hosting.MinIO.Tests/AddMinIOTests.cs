// -----------------------------------------------------------------------
// <copyright file="AddMinIOTests.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting.MinIO.Tests;

public class AddMinIOTests
{
    [Fact]
    public void AddMinIOAddsGeneratedPasswordParameterWithUserSecretsParameterDefaultInRunMode()
    {
        var appBuilder = DistributedApplication.CreateBuilder();

        var minio = appBuilder.AddMinIO("minio");

        Assert.Equal("Aspire.Hosting.ApplicationModel.UserSecretsParameterDefault", minio.Resource.PasswordParameter.Default?.GetType().FullName);
    }

    [Fact]
    public void AddMinIODoesNotAddGeneratedPasswordParameterWithUserSecretsParameterDefaultInPublishMode()
    {
        var appBuilder = DistributedApplication.CreateBuilder(["Publishing:Publisher=manifest"]);

        var minio = appBuilder.AddMinIO("minio");

        Assert.NotEqual("Aspire.Hosting.ApplicationModel.UserSecretsParameterDefault", minio.Resource.PasswordParameter.Default?.GetType().FullName);
    }
}