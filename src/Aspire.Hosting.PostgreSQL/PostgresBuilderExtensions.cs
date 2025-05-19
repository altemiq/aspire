// -----------------------------------------------------------------------
// <copyright file="PostgresBuilderExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

/// <summary>
/// <c>postgres</c> extensions.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Public API")]
public static partial class PostgresBuilderExtensions
{
    /// <summary>
    /// Adds <c>tle</c> support for the database.
    /// </summary>
    /// <typeparam name="T">The type of resource.</typeparam>
    /// <param name="builder">The builder.</param>
    /// <param name="version">The version.</param>
    /// <returns>The input builder.</returns>
    public static IResourceBuilder<T> WithTle<T>(this IResourceBuilder<T> builder, string? version = default)
        where T : PostgresServerResource =>
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
        where T : PostgresServerResource => builder.SetupDockerfile().WithAnnotation(new RustAnnotation(branch ?? "v1.2.8"));

    /// <summary>
    /// Installs the TLE extension for the database.
    /// </summary>
    /// <typeparam name="T">The type of database.</typeparam>
    /// <param name="builder">The resource builder.</param>
    /// <param name="extension">The extension to install.</param>
    /// <returns>The input database.</returns>
    public static IResourceBuilder<T> WithTleExtension<T>(this IResourceBuilder<T> builder, string extension)
        where T : PostgresDatabaseResource => builder.WithTleExtensions(extension);

    /// <summary>
    /// Installs the TLE extensions for the database.
    /// </summary>
    /// <typeparam name="T">The type of database.</typeparam>
    /// <param name="builder">The resource builder.</param>
    /// <param name="extensions">The extensions to install.</param>
    /// <returns>The input database.</returns>
    public static IResourceBuilder<T> WithTleExtensions<T>(this IResourceBuilder<T> builder, params string[] extensions)
        where T : PostgresDatabaseResource
    {
        _ = builder.SetupTleExtensions();
        foreach (var extension in extensions)
        {
            builder.WithAnnotation(new TleExtensionAnnotation(extension));
        }

        return builder;
    }

    private static IResourceBuilder<T> SetupDockerfile<T>(this IResourceBuilder<T> builder)
        where T : PostgresServerResource
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
        builder.WithDockerfile(".", Path.Combine(Path.GetTempPath(), $"postgres-{suffix}.Dockerfile"))
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
                    }

                    // write out the docker file
                    await File.WriteAllLinesAsync(
                        dockerfileBuild.DockerfilePath,
                        GetDockerfileContents(tle, plrust),
                        cancellationToken).ConfigureAwait(false);
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

    private static IEnumerable<string> GetDockerfileContents(bool tle, bool plrust)
    {
        yield return "ARG IMAGE=postgres";
        yield return "ARG TAG=17";
        yield return string.Empty;
        yield return "FROM ${IMAGE}:${TAG}";
        yield return string.Empty;

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
        where T : PostgresDatabaseResource
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
                var env = new Dictionary<string, string?>(StringComparer.Ordinal)
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