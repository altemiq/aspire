// -----------------------------------------------------------------------
// <copyright file="AmazonServiceExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// <see cref="Amazon.Runtime.IAmazonService"/> extensions.
/// </summary>
public static class AmazonServiceExtensions
{
    /// <summary>
    /// Get AWS service of type <typeparamref name="T"/> from the <see cref="IServiceProvider"/>.
    /// </summary>
    /// <typeparam name="T">The type of service object to get.</typeparam>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> to retrieve the service object from.</param>
    /// <param name="profile">The optional profile.</param>
    /// <returns>A service object of type <typeparamref name="T"/> or <see langword="null"/> if there is not such service.</returns>
    public static T? GetAwsService<T>(this IServiceProvider serviceProvider, string? profile = default)
        where T : Amazon.Runtime.IAmazonService => serviceProvider.GetService<T>().UpdateProfile(profile);

    /// <summary>
    /// Get AWS service of type <typeparamref name="T"/> from the <see cref="IServiceProvider"/>.
    /// </summary>
    /// <typeparam name="T">The type of service object to get.</typeparam>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> to retrieve the service object from.</param>
    /// <param name="serviceKey">An object that specifies the key of service object to get.</param>
    /// <param name="profile">The optional profile.</param>
    /// <returns>A service object of type <typeparamref name="T"/> or <see langword="null"/> if there is not such service.</returns>
    public static T? GetKeyedAwsService<T>(this IServiceProvider serviceProvider, object? serviceKey, string? profile = default)
        where T : Amazon.Runtime.IAmazonService => serviceProvider.GetKeyedService<T>(serviceKey).UpdateProfile(profile);

    /// <summary>
    /// Get AWS service of type <typeparamref name="T"/> from the <see cref="IServiceProvider"/>.
    /// </summary>
    /// <typeparam name="T">The type of service object to get.</typeparam>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> to retrieve the service object from.</param>
    /// <param name="profile">The optional profile.</param>
    /// <returns>A service object of type <typeparamref name="T"/>.</returns>
    public static T GetRequiredAwsService<T>(this IServiceProvider serviceProvider, string? profile = default)
        where T : Amazon.Runtime.IAmazonService => serviceProvider.GetRequiredService<T>().UpdateProfile(profile);

    /// <summary>
    /// Get AWS service of type <typeparamref name="T"/> from the <see cref="IServiceProvider"/>.
    /// </summary>
    /// <typeparam name="T">The type of service object to get.</typeparam>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> to retrieve the service object from.</param>
    /// <param name="serviceKey">An object that specifies the key of service object to get.</param>
    /// <param name="profile">The optional profile.</param>
    /// <returns>A service object of type <typeparamref name="T"/>.</returns>
    public static T GetRequiredKeyedAwsService<T>(this IServiceProvider serviceProvider, object? serviceKey, string? profile = default)
        where T : Amazon.Runtime.IAmazonService => serviceProvider.GetRequiredKeyedService<T>(serviceKey).UpdateProfile(profile);

    /// <summary>
    /// Updates the profile for the <see cref="Amazon.Runtime.IAmazonService"/>.
    /// </summary>
    /// <typeparam name="T">The type of service.</typeparam>
    /// <param name="service">The service to update.</param>
    /// <param name="profile">The profile to update with.</param>
    /// <returns>The update service if <paramref name="profile"/> represented valid credentials.</returns>
    /// <exception cref="InvalidOperationException">Could not create a new instance of <paramref name="service"/>.</exception>
    [return: System.Diagnostics.CodeAnalysis.NotNullIfNotNull(nameof(service))]
    public static T? UpdateProfile<T>(this T? service, string? profile)
        where T : Amazon.Runtime.IAmazonService =>
        service is null || profile is null
            ? service // just return the service directly
            : UpdateProfile(service, profile, dispose: false); // normally AWS services are singleton, so we do NOT want to dispose of them.

    private static T UpdateProfile<T>(T service, string profile, bool dispose)
        where T : Amazon.Runtime.IAmazonService
    {
        // get the profile
        var chain = new Amazon.Runtime.CredentialManagement.CredentialProfileStoreChain();
        if (!chain.TryGetAWSCredentials(profile, out var profileCredentials))
        {
            return service;
        }

        var config = service.Config;
        var serviceType = service.GetType();
        if (dispose && service is IDisposable disposable)
        {
            disposable.Dispose();
        }

        T? value = default;
        if (config is null)
        {
            // do a simple create
            value = (T?)Activator.CreateInstance(serviceType, profileCredentials);
        }
        else if (serviceType.GetConstructor([typeof(Amazon.Runtime.AWSCredentials), config.GetType()]) is { } constructor)
        {
            value = (T?)constructor.Invoke([profileCredentials, config]);
        }

        return value ?? throw new InvalidOperationException();
    }
}