// -----------------------------------------------------------------------
// <copyright file="ResourceBuilderExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting;

using Aspire.Hosting.ApplicationModel;
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
    public static IResourceBuilder<AWS.IAWSProfileConfig> AddAWSProfileConfig(this IDistributedApplicationBuilder builder, string? name = default) => builder
        .AddResource<AWS.IAWSProfileConfig>(new AWS.AWSProfileConfig { Name = name ?? "aws-config" })
        .WithInitialState(new CustomResourceSnapshot
        {
            ResourceType = "Configuration",
            Properties = [],
            State = new ResourceStateSnapshot("Configuring", KnownResourceStates.Starting),
        });

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
        _ = builder.WithAnnotation(new AWSConfigurationFileAnnotation { FileName = GetFileName() }, ResourceAnnotationMutationBehavior.Replace);

        _ = builder.ApplicationBuilder.Eventing.Subscribe<BeforeStartEvent>((e, _) => ProcessProfiles(e.Services, builder.Resource));

        return builder;

        async Task ProcessProfiles(IServiceProvider services, T configuration)
        {
            // get the annotation
            if (configuration.TryGetAnnotationsOfType<AWSConfigurationFileAnnotation>(out var fileAnnotaions)
                && fileAnnotaions.FirstOrDefault() is { } fileAnnotation)
            {
                var rns = services.GetRequiredService<ResourceNotificationService>();
                var rls = services.GetRequiredService<ResourceLoggerService>();
                var logger = rls.GetLogger(configuration);

                LogCreatingAwsConfiguration(logger, fileAnnotation.FileName);
                var sharedCredentialsFile = new Amazon.Runtime.CredentialManagement.SharedCredentialsFile(fileAnnotation.FileName);
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

                await rns.PublishUpdateAsync(configuration, s => s with
                {
                    State = new ResourceStateSnapshot(KnownResourceStates.Finished, KnownResourceStateStyles.Success),
                    Properties = [
                        .. s.Properties,
                        new ResourcePropertySnapshot(CustomResourceKnownProperties.Source, fileAnnotation.FileName),
                    ],
                }).ConfigureAwait(false);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Critical Vulnerability", "S5445:Insecure temporary file creation methods should not be used", Justification = "This is fine")]
        static string GetFileName()
        {
            return Path.GetTempFileName();
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

    [LoggerMessage(Level = LogLevel.Information, Message = "Creating AWS configuration at {FileName}")]
    private static partial void LogCreatingAwsConfiguration(ILogger logger, string fileName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Registering Profile {Name}")]
    private static partial void LogRegisteringProfile(ILogger logger, string name);

    [LoggerMessage(Level = LogLevel.Information, Message = "AWS configuration completed")]
    private static partial void LogCompleted(ILogger logger);

    private sealed class AWSConfigurationFileAnnotation : ApplicationModel.IResourceAnnotation
    {
        public required string FileName { get; init; }
    }
}