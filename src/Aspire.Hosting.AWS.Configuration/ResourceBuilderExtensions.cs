// -----------------------------------------------------------------------
// <copyright file="ResourceBuilderExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting;

using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

/// <summary>
/// The resource builder extensions.
/// </summary>
public static partial class ResourceBuilderExtensions
{
    /// <summary>
    /// Adds the AWS configuration to the application.
    /// </summary>
    /// <param name="builder">The application builder.</param>
    /// <param name="name">The name of the resource.</param>
    /// <returns>The resource containing the configuration.</returns>
    public static IResourceBuilder<AWS.IAWSProfileConfig> AddAWSProfileConfig(this IDistributedApplicationBuilder builder, string? name = default)
    {
        var profiles = builder
            .AddResource<AWS.IAWSProfileConfig>(new AWS.AWSProfileConfig { Name = name ?? "aws-config" })
            .WithInitialState(new CustomResourceSnapshot
            {
                ResourceType = "Configuration",
                Properties = [],
                State = new ResourceStateSnapshot("Configuring", KnownResourceStates.Starting),
            });

        // add the configuration to the resource
        _ = builder.Eventing.Subscribe<BeforeStartEvent>((_, _) =>
        {
            if (profiles.Resource.TryGetLastAnnotation<AWSConfigurationFileAnnotation>(out var annotation)
                && annotation.FileName is { } fileName)
            {
                // set the AWS Profiles location
                Amazon.AWSConfigs.AWSProfilesLocation = fileName;

                // set the environment variable
                Environment.SetEnvironmentVariable(Amazon.Runtime.CredentialManagement.SharedCredentialsFile.SharedCredentialsFileEnvVar, fileName, EnvironmentVariableTarget.Process);
                RefreshEnvironmentVariables(builder.Configuration);

                // set the profiles location for the .NET setup
                _ = builder.Configuration.AddInMemoryCollection([new KeyValuePair<string, string?>("AWS:ProfilesLocation", fileName)]);
            }

            return Task.CompletedTask;
        });

        return profiles;
    }

    /// <summary>
    /// Sets the AWS config for the builder application.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The application builder.</returns>
    public static IDistributedApplicationBuilder SetAWSConfig(this IDistributedApplicationBuilder builder, Aspire.Hosting.AWS.IAWSSDKConfig configuration)
    {
        var dictionary = new Dictionary<string, string?>(StringComparer.Ordinal);

        if (configuration.Profile is { } profile
            && !string.Equals(builder.Configuration.GetValue<string>("AWS:Profile"), profile, StringComparison.Ordinal))
        {
            dictionary.Add("AWS:Profile", profile);
            Amazon.AWSConfigs.AWSProfileName = profile;
            Environment.SetEnvironmentVariable("AWS_PROFILE", profile, EnvironmentVariableTarget.Process);
        }

        if (configuration.Region is { SystemName: var region }
            && !string.Equals(builder.Configuration.GetValue<string>("AWS:Region"), region, StringComparison.Ordinal))
        {
            dictionary.Add("AWS:Region", region);
            Amazon.AWSConfigs.AWSRegion = region;
            Environment.SetEnvironmentVariable("AWS_REGION", region, EnvironmentVariableTarget.Process);
        }

        if (dictionary.Count is > 0)
        {
            RefreshEnvironmentVariables(builder.Configuration);
            _ = builder.Configuration.AddInMemoryCollection(dictionary);
        }

        return builder;
    }

    /// <summary>
    /// Adds a profile to the <see cref="AWS.IAWSProfileConfig"/>.
    /// </summary>
    /// <typeparam name="T">The type of configuration.</typeparam>
    /// <param name="builder">The resource builder.</param>
    /// <param name="name">The name of the profile.</param>
    /// <param name="accessKeyId">The Access Key ID parameter.</param>
    /// <param name="secretAccessKey">The Secret Access Key parameter.</param>
    /// <param name="secretToken">The Secret Token parameter.</param>
    /// <returns>The resource builder for chaining.</returns>
    public static IResourceBuilder<T> WithProfile<T>(
        this IResourceBuilder<T> builder,
        string name,
        IResourceBuilder<ParameterResource>? accessKeyId = null,
        IResourceBuilder<ParameterResource>? secretAccessKey = null,
        IResourceBuilder<ParameterResource>? secretToken = null)
        where T : AWS.IAWSProfileConfig
    {
        var accessKeyIdParameter = accessKeyId?.Resource ?? ParameterResourceBuilderExtensions.CreateDefaultPasswordParameter(builder.ApplicationBuilder, $"{name}-access-key-id");
        var secretAccessKeyParameter = secretAccessKey?.Resource ?? ParameterResourceBuilderExtensions.CreateDefaultPasswordParameter(builder.ApplicationBuilder, $"{name}-secret-access-key");
        builder.Resource.Profiles.Add(new() { Name = name, AccessKeyId = accessKeyIdParameter, SecretAccessKey = secretAccessKeyParameter, SessionToken = secretToken?.Resource });
        return builder;
    }

    /// <summary>
    /// Sets the <see cref="AWS.IAWSProfileConfig"/> to be written out as a config file.
    /// </summary>
    /// <typeparam name="T">The type of resource.</typeparam>
    /// <param name="builder">The resource builder.</param>
    /// <returns>The resource builder for chaining.</returns>
    public static IResourceBuilder<T> AsConfigurationFile<T>(this IResourceBuilder<T> builder)
        where T : AWS.IAWSProfileConfig
    {
        _ = builder.WithAnnotation(new AWSConfigurationFileAnnotation(builder.Resource), ResourceAnnotationMutationBehavior.Replace);

        _ = builder.ApplicationBuilder.Eventing.Subscribe<BeforeStartEvent>((e, _) => ProcessProfiles(e.Services, builder.Resource));

        return builder;

        async Task ProcessProfiles(IServiceProvider services, T configuration)
        {
            // get the annotation
            if (configuration.TryGetLastAnnotation<AWSConfigurationFileAnnotation>(out var fileAnnotation))
            {
                var rns = services.GetRequiredService<ResourceNotificationService>();
                var rls = services.GetRequiredService<ResourceLoggerService>();
                var logger = rls.GetLogger(configuration);

                var fileName = fileAnnotation.FileName;
                if (!System.IO.Path.Exists(fileName))
                {
                    LogCreatingAwsConfiguration(logger, fileName);
                    var sharedCredentialsFile = new Amazon.Runtime.CredentialManagement.SharedCredentialsFile(fileName);
                    foreach (var profile in configuration.Profiles)
                    {
                        LogRegisteringProfile(logger, profile.Name);
                        sharedCredentialsFile.RegisterProfile(
                            new Amazon.Runtime.CredentialManagement.CredentialProfile(
                                profile.Name,
                                new Amazon.Runtime.CredentialManagement.CredentialProfileOptions
                                {
                                    AccessKey = profile.AccessKeyId.Value,
                                    SecretKey = profile.SecretAccessKey.Value,
                                    Token = profile.SessionToken?.Value,
                                }));
                    }

                    LogCompleted(logger);
                }

                await rns.PublishUpdateAsync(configuration, s => s with
                {
                    State = new ResourceStateSnapshot(KnownResourceStates.Finished, KnownResourceStateStyles.Success),
                    Properties = [
                        .. s.Properties,
                        new ResourcePropertySnapshot(CustomResourceKnownProperties.Source, fileName),
                    ],
                }).ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// Adds the reference to the builder.
    /// </summary>
    /// <typeparam name="T">The type of reference.</typeparam>
    /// <param name="builder">The builder.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The input builder.</returns>
    public static IResourceBuilder<T> WithReference<T>(this IResourceBuilder<T> builder, IResourceBuilder<AWS.IAWSProfileConfig> configuration)
        where T : IResourceWithEnvironment => builder.WithReference(configuration.Resource);

    /// <summary>
    /// Adds the reference to the builder.
    /// </summary>
    /// <typeparam name="T">The type of reference.</typeparam>
    /// <param name="builder">The builder.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The input builder.</returns>
    public static IResourceBuilder<T> WithReference<T>(this IResourceBuilder<T> builder, AWS.IAWSProfileConfig configuration)
        where T : IResourceWithEnvironment
    {
        // add the configuration to the resource
        if (configuration.Annotations.OfType<AWSConfigurationFileAnnotation>().FirstOrDefault() is { } fileAnnotaion)
        {
            _ = builder.WithEnvironment(callback => callback.EnvironmentVariables[Amazon.Runtime.CredentialManagement.SharedCredentialsFile.SharedCredentialsFileEnvVar] = fileAnnotaion.FileName);
        }

        return builder;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Creating AWS configuration at '{FileName}'")]
    private static partial void LogCreatingAwsConfiguration(ILogger logger, string fileName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Registering Profile '{Name}'")]
    private static partial void LogRegisteringProfile(ILogger logger, string name);

    [LoggerMessage(Level = LogLevel.Information, Message = "AWS configuration completed")]
    private static partial void LogCompleted(ILogger logger);

    private static void RefreshEnvironmentVariables(IConfigurationRoot configurationRoot)
    {
        foreach (var provider in configurationRoot.Providers.OfType<Microsoft.Extensions.Configuration.EnvironmentVariables.EnvironmentVariablesConfigurationProvider>())
        {
            provider.Load();
        }
    }

    private sealed class AWSConfigurationFileAnnotation(AWS.IAWSProfileConfig profileConfig) : ApplicationModel.IResourceAnnotation
    {
        private string? fileName;

        public string FileName => this.fileName ??= this.GetFileName();

        private string GetFileName()
        {
            return Path.Combine(Path.GetTempPath(), $"{profileConfig.Name}-{ConvertHashToString(profileConfig.GetHashCode())}");

            static string ConvertHashToString(int hash)
            {
                var bytes = BitConverter.GetBytes(hash);
                return System.Convert.ToHexString(bytes).Replace("-", string.Empty, StringComparison.Ordinal);
            }
        }
    }
}