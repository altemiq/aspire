// -----------------------------------------------------------------------
// <copyright file="LocalStackHealthCheck.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting.LocalStack;

using System.Net.Http.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;

/// <summary>
/// The <see cref="LocalStack"/> <see cref="IHealthCheck"/>.
/// </summary>
/// <param name="httpClientFactory">The <see cref="HttpClient"/> factory.</param>
internal sealed class LocalStackHealthCheck(Func<HttpClient> httpClientFactory) : IHealthCheck
{
    /// <summary>
    /// Gets the URI.
    /// </summary>
    public required Uri Uri { get; init; }

    /// <summary>
    /// Gets the services.
    /// </summary>
    public required LocalStackServices.Community Services { get; init; }

    /// <inheritdoc/>
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var builder = new UriBuilder(this.Uri) { Path = "_localstack/health" };

        var httpClient = httpClientFactory();

        using var response = await httpClient.GetAsync(builder.Uri, cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, description: $"Health endpoint is not responding with 200 OK, the current status is {response.StatusCode} and the content {await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false)}");
        }

        if (this.Services == default)
        {
            // this is enough, we just care that the end point worked.
            return HealthCheckResult.Healthy();
        }

        var healthCheck = await response.Content.ReadFromJsonAsync<LocalStackHealthResponse>(cancellationToken).ConfigureAwait(false);

        if (healthCheck is null)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, description: "Failed to read health status from local stack");
        }

        if (ApplicationModel.LocalStackServerResource
            .GetServiceNames(this.Services)
            .FirstOrDefault(s => !healthCheck.Services.TryGetValue(s, out var value) || value is not ("available" or "running")) is { } serviceName)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, description: $"Required service {serviceName} is not available");
        }

        return HealthCheckResult.Healthy();
    }

    private sealed class LocalStackHealthResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("edition")]
        public string? Edition { get; init; }

        [System.Text.Json.Serialization.JsonPropertyName("services")]
        public Dictionary<string, string> Services { get; init; } = [];

        [System.Text.Json.Serialization.JsonPropertyName("version")]
        public string? Version { get; init; }
    }
}