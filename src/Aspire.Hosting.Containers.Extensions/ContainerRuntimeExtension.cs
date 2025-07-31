// -----------------------------------------------------------------------
// <copyright file="ContainerRuntimeExtension.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

/// <summary>
/// The <see cref="ContainerRuntime"/> extensions.
/// </summary>
public static class ContainerRuntimeExtension
{
    /// <summary>
    /// Set the container runtime for the application model.
    /// </summary>
    /// <param name="builder">The <see cref="IDistributedApplicationBuilder"/>.</param>
    /// <returns>A reference to the <see cref="IDistributedApplicationBuilder"/>.</returns>
    public static IDistributedApplicationBuilder SetContainerRuntime(
        this IDistributedApplicationBuilder builder)
    {
        if (builder.ExecutionContext.IsRunMode
            || builder.Configuration.GetValue<string?>("DcpPublisher.ContainerRuntime") is not null
            || builder.Configuration.GetValue<string?>("ASPIRE_CONTAINER_RUNTIME") is not null
            || builder.Configuration.GetValue<string?>("DOTNET_ASPIRE_CONTAINER_RUNTIME") is not null)
        {
            return builder;
        }

        // ensure we set up the container runtime here, if it's not set
        builder.Services.TryAdd(
            new ServiceDescriptor(
                typeof(DistributedApplication).Assembly.GetType("Aspire.Hosting.Dcp.IDcpDependencyCheckService") ?? throw new InvalidOperationException(),
                typeof(DistributedApplication).Assembly.GetType("Aspire.Hosting.Dcp.DcpDependencyCheck") ?? throw new InvalidOperationException(),
                ServiceLifetime.Singleton));

        builder.Configuration.AddInMemoryCollection([new("DcpPublisher.ContainerRuntime", ContainerRuntime.GetNameAsync(builder.Services.BuildServiceProvider(), CancellationToken.None).GetAwaiter().GetResult())]);

        return builder;
    }
}