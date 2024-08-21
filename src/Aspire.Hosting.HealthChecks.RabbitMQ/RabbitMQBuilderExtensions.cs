// -----------------------------------------------------------------------
// <copyright file="RabbitMQBuilderExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting;

using Aspire.Hosting.ApplicationModel;

/// <summary>
/// <see cref="global::RabbitMQ"/> extensions.
/// </summary>
public static class RabbitMQBuilderExtensions
{
    /// <summary>
    /// Adds a health check to the <see cref="global::RabbitMQ"/> server resource.
    /// </summary>
    /// <param name="builder">The resource builder.</param>
    /// <returns>A resource builder with the health check annotation added.</returns>
    public static IResourceBuilder<RabbitMQServerResource> WithHealthCheck(this IResourceBuilder<RabbitMQServerResource> builder) =>
        builder.WithAnnotation(
            HealthCheckAnnotation.Create(
                builder.Resource,
                connectionString => new HealthChecks.RabbitMQ.RabbitMQHealthCheck(
                    new HealthChecks.RabbitMQ.RabbitMQHealthCheckOptions
                    {
                        ConnectionFactory = new global::RabbitMQ.Client.ConnectionFactory
                        {
                            Uri = new(connectionString),
                            AutomaticRecoveryEnabled = true,
                        },
                    })));
}