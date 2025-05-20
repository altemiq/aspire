// -----------------------------------------------------------------------
// <copyright file="PostgresBuilderExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting;

extern alias core;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

/// <summary>
/// <c>postgres</c> extensions.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Public API")]
public static partial class PostgresBuilderExtensions
{
    /// <summary>
    /// Adds a PostgreSQL resource to the application model. A container is used for local development.
    /// </summary>
    /// <param name="builder">The <see cref="IDistributedApplicationBuilder" />.</param>
    /// <param name="name">The name of the resource. This name will be used as the connection string name when referenced in a dependency.</param>
    /// <param name="userName">The parameter used to provide the username for the PostgreSQL resource. If <see langword="null" /> a default value will be used.</param>
    /// <param name="password">The parameter used to provide the administrator password for the PostgreSQL resource. If <see langword="null" /> a random password will be generated.</param>
    /// <param name="port">The host port used when launching the container. If null a random port will be assigned.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}" />.</returns>
    /// <remarks>
    /// <para>
    /// This resource includes built-in health checks. When this resource is referenced as a dependency
    /// using the <see cref="ResourceBuilderExtensions.WaitFor{T}(IResourceBuilder{T}, IResourceBuilder{IResource})" />
    /// extension method then the dependent resource will wait until the Postgres resource is able to service
    /// requests.
    /// </para>
    /// This version of the package defaults to the <inheritdoc cref="Postgres.PostgresContainerImageTags.V13.Tag" /> tag.
    /// </remarks>
    public static IResourceBuilder<core::Aspire.Hosting.ApplicationModel.PostgresServerResource> AddPostgres13(this IDistributedApplicationBuilder builder, [ResourceName] string name, IResourceBuilder<ParameterResource>? userName = null, IResourceBuilder<ParameterResource>? password = null, int? port = null) => AddPostgresWithTag(builder, Postgres.PostgresContainerImageTags.V13.Tag, name, userName, password, port);

    /// <summary>
    /// Adds a PostgreSQL resource to the application model. A container is used for local development.
    /// </summary>
    /// <param name="builder">The <see cref="IDistributedApplicationBuilder" />.</param>
    /// <param name="name">The name of the resource. This name will be used as the connection string name when referenced in a dependency.</param>
    /// <param name="userName">The parameter used to provide the username for the PostgreSQL resource. If <see langword="null" /> a default value will be used.</param>
    /// <param name="password">The parameter used to provide the administrator password for the PostgreSQL resource. If <see langword="null" /> a random password will be generated.</param>
    /// <param name="port">The host port used when launching the container. If null a random port will be assigned.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}" />.</returns>
    /// <remarks>
    /// <para>
    /// This resource includes built-in health checks. When this resource is referenced as a dependency
    /// using the <see cref="ResourceBuilderExtensions.WaitFor{T}(IResourceBuilder{T}, IResourceBuilder{IResource})" />
    /// extension method then the dependent resource will wait until the Postgres resource is able to service
    /// requests.
    /// </para>
    /// This version of the package defaults to the <inheritdoc cref="Postgres.PostgresContainerImageTags.V14.Tag" /> tag.
    /// </remarks>
    public static IResourceBuilder<core::Aspire.Hosting.ApplicationModel.PostgresServerResource> AddPostgres14(this IDistributedApplicationBuilder builder, [ResourceName] string name, IResourceBuilder<ParameterResource>? userName = null, IResourceBuilder<ParameterResource>? password = null, int? port = null) => AddPostgresWithTag(builder, Postgres.PostgresContainerImageTags.V14.Tag, name, userName, password, port);

    /// <summary>
    /// Adds a PostgreSQL resource to the application model. A container is used for local development.
    /// </summary>
    /// <param name="builder">The <see cref="IDistributedApplicationBuilder" />.</param>
    /// <param name="name">The name of the resource. This name will be used as the connection string name when referenced in a dependency.</param>
    /// <param name="userName">The parameter used to provide the username for the PostgreSQL resource. If <see langword="null" /> a default value will be used.</param>
    /// <param name="password">The parameter used to provide the administrator password for the PostgreSQL resource. If <see langword="null" /> a random password will be generated.</param>
    /// <param name="port">The host port used when launching the container. If null a random port will be assigned.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}" />.</returns>
    /// <remarks>
    /// <para>
    /// This resource includes built-in health checks. When this resource is referenced as a dependency
    /// using the <see cref="ResourceBuilderExtensions.WaitFor{T}(IResourceBuilder{T}, IResourceBuilder{IResource})" />
    /// extension method then the dependent resource will wait until the Postgres resource is able to service
    /// requests.
    /// </para>
    /// This version of the package defaults to the <inheritdoc cref="Postgres.PostgresContainerImageTags.V15.Tag" /> tag.
    /// </remarks>
    public static IResourceBuilder<core::Aspire.Hosting.ApplicationModel.PostgresServerResource> AddPostgres15(this IDistributedApplicationBuilder builder, [ResourceName] string name, IResourceBuilder<ParameterResource>? userName = null, IResourceBuilder<ParameterResource>? password = null, int? port = null) => AddPostgresWithTag(builder, Postgres.PostgresContainerImageTags.V15.Tag, name, userName, password, port);

    /// <summary>
    /// Adds a PostgreSQL resource to the application model. A container is used for local development.
    /// </summary>
    /// <param name="builder">The <see cref="IDistributedApplicationBuilder" />.</param>
    /// <param name="name">The name of the resource. This name will be used as the connection string name when referenced in a dependency.</param>
    /// <param name="userName">The parameter used to provide the username for the PostgreSQL resource. If <see langword="null" /> a default value will be used.</param>
    /// <param name="password">The parameter used to provide the administrator password for the PostgreSQL resource. If <see langword="null" /> a random password will be generated.</param>
    /// <param name="port">The host port used when launching the container. If null a random port will be assigned.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}" />.</returns>
    /// <remarks>
    /// <para>
    /// This resource includes built-in health checks. When this resource is referenced as a dependency
    /// using the <see cref="ResourceBuilderExtensions.WaitFor{T}(IResourceBuilder{T}, IResourceBuilder{IResource})" />
    /// extension method then the dependent resource will wait until the Postgres resource is able to service
    /// requests.
    /// </para>
    /// This version of the package defaults to the <inheritdoc cref="Postgres.PostgresContainerImageTags.V16.Tag" /> tag.
    /// </remarks>
    public static IResourceBuilder<core::Aspire.Hosting.ApplicationModel.PostgresServerResource> AddPostgres16(this IDistributedApplicationBuilder builder, [ResourceName] string name, IResourceBuilder<ParameterResource>? userName = null, IResourceBuilder<ParameterResource>? password = null, int? port = null) => AddPostgresWithTag(builder, Postgres.PostgresContainerImageTags.V16.Tag, name, userName, password, port);

    /// <summary>
    /// Adds a PostgreSQL resource to the application model. A container is used for local development.
    /// </summary>
    /// <param name="builder">The <see cref="IDistributedApplicationBuilder" />.</param>
    /// <param name="name">The name of the resource. This name will be used as the connection string name when referenced in a dependency.</param>
    /// <param name="userName">The parameter used to provide the username for the PostgreSQL resource. If <see langword="null" /> a default value will be used.</param>
    /// <param name="password">The parameter used to provide the administrator password for the PostgreSQL resource. If <see langword="null" /> a random password will be generated.</param>
    /// <param name="port">The host port used when launching the container. If null a random port will be assigned.</param>
    /// <returns>A reference to the <see cref="IResourceBuilder{T}" />.</returns>
    /// <remarks>
    /// <para>
    /// This resource includes built-in health checks. When this resource is referenced as a dependency
    /// using the <see cref="ResourceBuilderExtensions.WaitFor{T}(IResourceBuilder{T}, IResourceBuilder{IResource})" />
    /// extension method then the dependent resource will wait until the Postgres resource is able to service
    /// requests.
    /// </para>
    /// This version of the package defaults to the <inheritdoc cref="Postgres.PostgresContainerImageTags.V17.Tag" /> tag.
    /// </remarks>
    public static IResourceBuilder<core::Aspire.Hosting.ApplicationModel.PostgresServerResource> AddPostgres17(this IDistributedApplicationBuilder builder, [ResourceName] string name, IResourceBuilder<ParameterResource>? userName = null, IResourceBuilder<ParameterResource>? password = null, int? port = null) => AddPostgresWithTag(builder, Postgres.PostgresContainerImageTags.V17.Tag, name, userName, password, port);

    /// <summary>
    /// Adds <c>tle</c> support for the database.
    /// </summary>
    /// <typeparam name="T">The type of resource.</typeparam>
    /// <param name="builder">The builder.</param>
    /// <param name="version">The version.</param>
    /// <returns>The input builder.</returns>
    public static IResourceBuilder<T> WithTle<T>(this IResourceBuilder<T> builder, string? version = default)
        where T : core::Aspire.Hosting.ApplicationModel.PostgresServerResource =>
        builder
            .SetupDockerfile()
            .WithAnnotation(new TleAnnotation(version ?? "v1.5.0"))
            .WithContainerFiles(
                "/pg_tle/examples",
                (_, _) =>
                {
                    var postgresInstance = builder.Resource;
                    var endpoint = postgresInstance.PrimaryEndpoint;
                    IEnumerable<ContainerFileSystemItem> items =
                    [
                        new ContainerFile
                        {
                            Name = "env.ini",
                            Contents = $"""
                                        PGHOST=localhost
                                        PGPORT={(int)endpoint.TargetPort!}
                                        PGUSER={postgresInstance.UserNameParameter?.Value ?? "postgres"}
                                        """,
                        },
                    ];

                    return Task.FromResult(items);
                });

    /// <summary>
    /// Adds <c>plrust</c> support for the database.
    /// </summary>
    /// <typeparam name="T">The type of resource.</typeparam>
    /// <param name="builder">The builder.</param>
    /// <param name="branch">The branch.</param>
    /// <returns>The input builder.</returns>
    public static IResourceBuilder<T> WithPlRust<T>(this IResourceBuilder<T> builder, string? branch = default)
        where T : core::Aspire.Hosting.ApplicationModel.PostgresServerResource => builder.SetupDockerfile().WithAnnotation(new RustAnnotation(branch ?? "v1.2.8"));

    /// <summary>
    /// Installs the TLE extension for the database.
    /// </summary>
    /// <typeparam name="T">The type of database.</typeparam>
    /// <param name="builder">The resource builder.</param>
    /// <param name="extension">The extension to install.</param>
    /// <returns>The input database.</returns>
    public static IResourceBuilder<T> WithTleExtension<T>(this IResourceBuilder<T> builder, string extension)
        where T : core::Aspire.Hosting.ApplicationModel.PostgresDatabaseResource => builder.WithTleExtensions(extension);

    /// <summary>
    /// Installs the TLE extensions for the database.
    /// </summary>
    /// <typeparam name="T">The type of database.</typeparam>
    /// <param name="builder">The resource builder.</param>
    /// <param name="extensions">The extensions to install.</param>
    /// <returns>The input database.</returns>
    public static IResourceBuilder<T> WithTleExtensions<T>(this IResourceBuilder<T> builder, params string[] extensions)
        where T : core::Aspire.Hosting.ApplicationModel.PostgresDatabaseResource
    {
        _ = builder.SetupTleExtensions();
        foreach (var extension in extensions)
        {
            builder.WithAnnotation(new TleExtensionAnnotation(extension));
        }

        return builder;
    }

    private static IResourceBuilder<T> SetupDockerfile<T>(this IResourceBuilder<T> builder)
        where T : core::Aspire.Hosting.ApplicationModel.PostgresServerResource
    {
        // see if this has been set up already
        if (builder.Resource.HasAnnotationOfType<PostgresAnnotation>())
        {
            // this has already been initialised
            return builder;
        }

        // get the versions
        var image = "library/postgres";
        var tag = "17.4";
        if (builder.Resource.TryGetLastAnnotation<ContainerImageAnnotation>(out var imageAnnotation))
        {
            image = imageAnnotation.Image;
            if (imageAnnotation.Tag is { } t)
            {
                tag = t;
            }
        }

        var suffix = GenerateImageSuffix(builder)[..8];
        var contextDirectory = Path.Combine(Path.GetTempPath(), "postgres-" + suffix);
        builder.WithDockerfile(contextDirectory, Path.Combine(contextDirectory, $"postgres-{suffix}.Dockerfile"))
            .WithImage($"{image}/{suffix}")
            .WithImageTag(tag)
            .WithBuildArg("IMAGE", image)
            .WithBuildArg("TAG", tag)
            .WithArgs(
                context =>
                {
                    context.Args.Add("postgres");

                    var tle = builder.Resource.HasAnnotationOfType<TleAnnotation>();
                    var plrust = builder.Resource.HasAnnotationOfType<RustAnnotation>();

                    // shared libraries
                    ICollection<string> sharedPreloadLibraries = [];
                    if (tle)
                    {
                        sharedPreloadLibraries.Add("pg_tle");
                    }

                    if (plrust)
                    {
                        sharedPreloadLibraries.Add("plrust");
                    }

                    if (sharedPreloadLibraries.Count is not 0)
                    {
                        context.Args.Add("-c");
                        context.Args.Add($"shared_preload_libraries={string.Join(',', sharedPreloadLibraries)}");
                    }

                    if (plrust)
                    {
                        context.Args.Add("-c");
                        context.Args.Add("plrust.work_dir=/tmp");
                    }
                });

        builder.ApplicationBuilder.Eventing.Subscribe<BeforeResourceStartedEvent>(
            builder.Resource,
            async (evt, cancellationToken) =>
            {
                if (evt.Resource.TryGetLastAnnotation<DockerfileBuildAnnotation>(out var dockerfileBuild))
                {
                    bool zscaler = ZScaler.IsRunning();

                    var tle = false;
                    if (evt.Resource.TryGetLastAnnotation<TleAnnotation>(out var tleAnnotation))
                    {
                        tle = true;
                        dockerfileBuild.BuildArguments["TLE_BRANCH"] = tleAnnotation.Branch;
                    }

                    var plrust = false;
                    if (evt.Resource.TryGetLastAnnotation<RustAnnotation>(out var rustAnnotation))
                    {
                        plrust = true;
                        dockerfileBuild.BuildArguments["RUST_BRANCH"] = rustAnnotation.Branch;

                        await WriteManifestResource("llvm.sh", dockerfileBuild.ContextPath, cancellationToken).ConfigureAwait(false);
                        await WriteManifestResource("rust.sh", dockerfileBuild.ContextPath, cancellationToken).ConfigureAwait(false);
                    }

                    // write out the docker file
                    await File.WriteAllLinesAsync(
                        dockerfileBuild.DockerfilePath,
                        GetDockerfileContents(tle, plrust, zscaler),
                        cancellationToken).ConfigureAwait(false);

                    static async Task WriteManifestResource(string name, string destination, CancellationToken cancellationToken)
                    {
                        Directory.CreateDirectory(destination);

                        var stream = typeof(PgAdminTheme).Assembly.GetManifestResourceStream(typeof(PgAdminTheme), name) ?? throw new InvalidOperationException();
                        await using (stream.ConfigureAwait(continueOnCapturedContext: false))
                        {
                            var output = new FileStream(Path.Combine(destination, name), FileMode.Create);
                            await using (output.ConfigureAwait(continueOnCapturedContext: false))
                            {
                                await stream.CopyToAsync(output, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
                            }
                        }
                    }
                }
            });

        return builder;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA5350:Do Not Use Weak Cryptographic Algorithms", Justification = "This is not required to be secure")]
        static string GenerateImageSuffix(IResourceBuilder<T> builder)
        {
            var data = System.Text.Encoding.UTF8.GetBytes(builder.ApplicationBuilder.AppHostDirectory);
            var hash = System.Security.Cryptography.SHA1.HashData(data);
            return Convert.ToHexString(hash).ToLowerInvariant();
        }
    }

    private static IEnumerable<string> GetDockerfileContents(bool tle, bool plrust, bool zscaler)
    {
        yield return "ARG IMAGE=postgres";
        yield return "ARG TAG=17";
        yield return string.Empty;
        yield return "FROM ${IMAGE}:${TAG}";
        yield return string.Empty;

        if (zscaler)
        {
            yield return string.Empty;
            foreach (string dockerfileLine in ZScaler.GetDockerfileLines())
            {
                yield return dockerfileLine;
            }
        }

        if (tle)
        {
            using var stream = typeof(PgAdminTheme).Assembly.GetManifestResourceStream(typeof(PgAdminTheme), $"{nameof(tle)}.Dockerfile") ?? throw new InvalidOperationException();
            using var reader = new StreamReader(stream);

            yield return string.Empty;
            while (reader.ReadLine() is { } line)
            {
                yield return line;
            }
        }

        if (plrust)
        {
            using var stream = typeof(PgAdminTheme).Assembly.GetManifestResourceStream(typeof(PgAdminTheme), $"{nameof(plrust)}.Dockerfile") ?? throw new InvalidOperationException();
            using var reader = new StreamReader(stream);

            yield return string.Empty;
            while (reader.ReadLine() is { } line)
            {
                yield return line;
            }
        }

        if (tle)
        {
            // make sure that installing PL_TLE extensions works
            yield return "USER root";
            yield return "RUN mv /bin/sh /bin/sh.original && ln -s /bin/bash /bin/sh";
            yield return "USER postgres";
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3011:Reflection should not be used to increase accessibility of classes, methods, or fields", Justification = "Checked")]
    private static IResourceBuilder<T> SetupTleExtensions<T>(this IResourceBuilder<T> builder)
        where T : core::Aspire.Hosting.ApplicationModel.PostgresDatabaseResource
    {
        if (builder.Resource.HasAnnotationOfType<TleExtensionAnnotation>())
        {
            return builder;
        }

        builder.ApplicationBuilder.Eventing.Subscribe<ResourceReadyEvent>(
            builder.Resource,
            async (evt, cancellationToken) =>
            {
                // get the docker image name
                var containerResource = GetParentOfType<ContainerResource>(evt.Resource as IResourceWithParent);

                var rls = evt.Services.GetRequiredService<ResourceLoggerService>();
                var logger = rls.GetLogger(evt.Resource);

                // get name
                string? database = null;
                string? password = null;
                if (evt.Resource is IResourceWithConnectionString databaseResource)
                {
                    var connectionString = new Npgsql.NpgsqlConnectionStringBuilder(await databaseResource.GetConnectionStringAsync(cancellationToken).ConfigureAwait(false));
                    database = connectionString.Database;
                    password = connectionString.Password;
                }

                var containerRuntime = await ContainerResources.GetContainerRuntimeAsync(evt.Services, cancellationToken).ConfigureAwait(false);
                var env = new Dictionary<string, object?>(StringComparer.Ordinal)
                {
                    { "PGPASSWORD", password },
                    { "PGDB", database },
                };

                foreach (var extension in evt.Resource.Annotations.OfType<TleExtensionAnnotation>())
                {
                    await containerResource
                        .ExecAsync(containerRuntime, env, ["make", $"--directory=/pg_tle/examples/{extension.Name}", "install"], logger, cancellationToken)
                        .ConfigureAwait(false);
                }

                static TParent GetParentOfType<TParent>(IResourceWithParent? resource)
                    where TParent : IResource
                {
                    while (true)
                    {
                        switch (resource)
                        {
                            case IResourceWithParent<TParent> { Parent: { } typedParent }:
                                return typedParent;
                            case { Parent: TParent untypedParent }:
                                return untypedParent;
                            case { Parent: IResourceWithParent parentResource }:
                                resource = parentResource;
                                continue;
                            default:
                                throw new InvalidOperationException();
                        }
                    }
                }
            });

        return builder;
    }

    private static IResourceBuilder<core::Aspire.Hosting.ApplicationModel.PostgresServerResource> AddPostgresWithTag(IDistributedApplicationBuilder builder, string tag, [ResourceName] string name, IResourceBuilder<ParameterResource>? userName = null, IResourceBuilder<ParameterResource>? password = null, int? port = null) => core::Aspire.Hosting.PostgresBuilderExtensions.AddPostgres(builder, name, userName, password, port).WithImageTag(tag);

    [LoggerMessage(LogLevel.Information, Message = "{Data}")]
    private static partial void LogOutputData(ILogger logger, string data);

    [LoggerMessage(LogLevel.Error, Message = "{Data}")]
    private static partial void LogErrorData(ILogger logger, string data);

    [LoggerMessage(LogLevel.Debug, Message = "Starting process {FileName} {Arguments}")]
    private static partial void LogStartingProcess(ILogger logger, string fileName, string arguments);

    private record PostgresAnnotation : IResourceAnnotation;

    private sealed record TleAnnotation(string Branch) : PostgresAnnotation;

    private sealed record TleExtensionAnnotation(string Name) : IResourceAnnotation;

    private sealed record RustAnnotation(string Branch) : PostgresAnnotation;
}