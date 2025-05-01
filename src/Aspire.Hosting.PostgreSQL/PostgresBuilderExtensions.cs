// -----------------------------------------------------------------------
// <copyright file="PostgresBuilderExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting;

/// <summary>
/// <c>postgres</c> extensions.
/// </summary>
public static class PostgresBuilderExtensions
{
    /// <summary>
    /// Adds <c>tle</c> support for the database.
    /// </summary>
    /// <typeparam name="T">The type of resource.</typeparam>
    /// <param name="builder">The builder.</param>
    /// <param name="version">The version.</param>
    /// <param name="extensions">The extensions to load.</param>
    /// <returns>The input builder.</returns>
    public static IResourceBuilder<T> WithTle<T>(this IResourceBuilder<T> builder, string? version = default, params string[] extensions)
        where T : PostgresServerResource
    {
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
                                        PGDB=postgres
                                        """,
                        },
                    ];

                    return Task.FromResult(items);
                });

        foreach (var extension in extensions)
        {
            builder.WithAnnotation(new TleExtensionAnnotation(extension));
        }

        return builder;
    }

    /// <summary>
    /// Adds <c>plrust</c> support for the database.
    /// </summary>
    /// <typeparam name="T">The type of resource.</typeparam>
    /// <param name="builder">The builder.</param>
    /// <param name="branch">The branch.</param>
    /// <returns>The input builder.</returns>
    public static IResourceBuilder<T> WithPlRust<T>(this IResourceBuilder<T> builder, string? branch = default)
        where T : PostgresServerResource
    {
        SetupDockerfile(builder);
        builder.WithAnnotation(new RustAnnotation(branch ?? "v1.2.8"));
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
                    var plrust = false;
                    if (evt.Resource.TryGetLastAnnotation<TleAnnotation>(out var tleAnnotation))
                    {
                        tle = true;
                        dockerfileBuild.BuildArguments["TLE_BRANCH"] = tleAnnotation.Branch;
                    }

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
            yield return "USER root";
            yield return "RUN mv /bin/sh /bin/sh.original && ln -s /bin/bash /bin/sh";
            yield return "USER postgres";
        }
    }

    private record PostgresAnnotation : IResourceAnnotation;

    private sealed record TleAnnotation(string Branch) : PostgresAnnotation;

    private sealed record TleExtensionAnnotation(string Name) : IResourceAnnotation;

    private sealed record RustAnnotation(string Branch) : PostgresAnnotation;
}