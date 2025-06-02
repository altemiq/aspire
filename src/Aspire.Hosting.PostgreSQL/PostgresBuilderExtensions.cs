// -----------------------------------------------------------------------
// <copyright file="PostgresBuilderExtensions.cs" company="Altavec">
// Copyright (c) Altavec. All rights reserved.
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
    private const string DefaultRegistry = "docker.io";
    private const string DefaultImage = "library/postgres";
    private const string DefaultTag = "17.4";

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
            .SetupContainerfile()
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
        where T : core::Aspire.Hosting.ApplicationModel.PostgresServerResource => builder.SetupContainerfile().WithAnnotation(new RustAnnotation(branch ?? "v1.2.8"));

    /// <summary>
    /// Adds <c>pldotnet</c> support for the database.
    /// </summary>
    /// <typeparam name="T">The type of resource.</typeparam>
    /// <param name="builder">The builder.</param>
    /// <param name="branch">The branch.</param>
    /// <returns>The input builder.</returns>
    public static IResourceBuilder<T> WithPlDotnet<T>(this IResourceBuilder<T> builder, string? branch = default)
        where T : core::Aspire.Hosting.ApplicationModel.PostgresServerResource => builder.SetupContainerfile().WithAnnotation(new DotnetAnnotation(branch ?? "tag-v0.99-rc1"));

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

    private static IResourceBuilder<T> SetupContainerfile<T>(this IResourceBuilder<T> builder)
        where T : core::Aspire.Hosting.ApplicationModel.PostgresServerResource
    {
        // see if this has been set up already
        if (builder.Resource.HasAnnotationOfType<PostgresAnnotation>())
        {
            // this has already been initialised
            return builder;
        }

        builder.ApplicationBuilder.Eventing.Subscribe<BeforeStartEvent>(async (evt, cancellationToken) =>
        {
            // get the versions
            var baseImage = DefaultImage;
            var tag = DefaultTag;
            var registry = DefaultRegistry;
            if (builder.Resource.TryGetLastAnnotation<ContainerImageAnnotation>(out var imageAnnotation))
            {
                baseImage = imageAnnotation.Image;
                tag = imageAnnotation.Tag ?? DefaultTag;
                registry = imageAnnotation.Registry ?? DefaultRegistry;
            }

            var suffix = GenerateImageSuffix(builder)[..8];
            var image = $"{baseImage}/{suffix}";

            var imagePullPolicy = ImagePullPolicy.Default;
            if (builder.Resource.TryGetLastAnnotation<ContainerImagePullPolicyAnnotation>(out var imagePullPolicyAnnotation))
            {
                imagePullPolicy = imagePullPolicyAnnotation.ImagePullPolicy;
            }

            // if we're always pulling or if the container doesn't exist
            if (imagePullPolicy is ImagePullPolicy.Always || !await ContainerResources.ImageExistsAsync(evt.Services, image, tag, cancellationToken).ConfigureAwait(false))
            {
                // add the docker file
                var name = "postgres-" + suffix;

                // isolate this into its own context
                var contextDirectory = Path.Combine(Path.GetTempPath(), name);
                _ = builder
                    .WithDockerfile(contextDirectory, Path.Combine(contextDirectory, $"{name}.Dockerfile"))
                    .WithBuildArg("REGISTRY", registry)
                    .WithBuildArg("IMAGE", baseImage)
                    .WithBuildArg("TAG", tag);
            }

            // reset the registry/image/tag
            _ = builder
                .WithImageRegistry(registry: null)
                .WithImage(image)
                .WithImageTag(tag);
        });

        _ = builder.WithArgs(context =>
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
                        dockerfileBuild.BuildArguments["PL_RUST_BRANCH"] = rustAnnotation.Branch;

                        // downloaded from https://apt.llvm.org/llvm.sh
                        await WriteManifestResource("llvm.sh", dockerfileBuild.ContextPath, cancellationToken).ConfigureAwait(false);

                        // downloaded from https://sh.rustup.rs
                        await WriteManifestResource("rust.sh", dockerfileBuild.ContextPath, cancellationToken).ConfigureAwait(false);

                        // git patch for versions not compatible with the PL/Rust toolchain
                        await WriteManifestResource("0001-fix-version.patch", dockerfileBuild.ContextPath, cancellationToken).ConfigureAwait(false);
                    }

                    var pldotnet = false;
                    if (evt.Resource.TryGetLastAnnotation<DotnetAnnotation>(out var dotnetAnnotation))
                    {
                        pldotnet = true;
                        dockerfileBuild.BuildArguments["PL_DOTNET_BRANCH"] = dotnetAnnotation.Branch;
                    }

                    // write out the docker file
                    if (Path.GetDirectoryName(dockerfileBuild.DockerfilePath) is { } dockerfileDirectory)
                    {
                        Directory.CreateDirectory(dockerfileDirectory);
                    }

                    await File.WriteAllLinesAsync(
                        dockerfileBuild.DockerfilePath,
                        GetContainerfileContents(tle, plrust, pldotnet),
                        cancellationToken).ConfigureAwait(false);

                    static async Task WriteManifestResource(string name, string destination, CancellationToken cancellationToken)
                    {
                        Directory.CreateDirectory(destination);

                        var stream = GetManifestResourceStream(name);
                        await using (stream.ConfigureAwait(false))
                        {
                            // write to a temp file
                            var outputPath = Path.Combine(destination, name);
                            if (File.Exists(outputPath))
                            {
                                // make sure the files have UNIX line endings
                                var outputBytes = await ReadAsUnixAsync(stream, cancellationToken).ConfigureAwait(false);

                                // check the contents
                                if (!Equal(
                                    await ComputeArrayHashAsync(outputBytes, cancellationToken).ConfigureAwait(false),
                                    await ComputeFileHashAsync(outputPath, cancellationToken).ConfigureAwait(false)))
                                {
                                    await File.WriteAllBytesAsync(outputPath, outputBytes, cancellationToken).ConfigureAwait(false);
                                }

                                static async Task<byte[]> ReadAsUnixAsync(Stream stream, CancellationToken cancellationToken)
                                {
                                    var memoryStream = new MemoryStream();
                                    await using (memoryStream.ConfigureAwait(false))
                                    {
                                        const byte CarriageReturn = 0x0D;
                                        const byte LineFeed = 0x0A;
                                        var data = System.Buffers.ArrayPool<byte>.Shared.Rent((int)Math.Min(short.MaxValue, stream.Length));
                                        var position = 0;
                                        int count;
                                        while ((count = await stream.ReadAsync(data.AsMemory(position, data.Length), cancellationToken).ConfigureAwait(false)) is not 0)
                                        {
                                            // reset the position
                                            position = 0;
                                            int index;
                                            do
                                            {
                                                index = Array.IndexOf(data, CarriageReturn, position, count - position);
                                                if (index >= 0)
                                                {
                                                    if (count >= index && (data[index + 1] is LineFeed))
                                                    {
                                                        // next item is a LF, so copy before the CR.
                                                        await memoryStream.WriteAsync(data.AsMemory(position, index - position), cancellationToken).ConfigureAwait(false);
                                                    }
                                                    else
                                                    {
                                                        // next item is not a LF, so copy the CR.
                                                        await memoryStream.WriteAsync(data.AsMemory(position, index - position + 1), cancellationToken).ConfigureAwait(false);
                                                    }

                                                    position = index + 1;
                                                }
                                                else
                                                {
                                                    await memoryStream.WriteAsync(data.AsMemory(position, count - position), cancellationToken).ConfigureAwait(false);
                                                    position = count;
                                                }
                                            }
                                            while (index >= 0);

                                            if (count > position)
                                            {
                                                // copy the last bytes to the start of the array
                                                Array.Copy(data, position, data, 0, count - position);
                                                position = count - position;
                                            }
                                            else
                                            {
                                                position = 0;
                                            }
                                        }

                                        System.Buffers.ArrayPool<byte>.Shared.Return(data);

                                        return memoryStream.ToArray();
                                    }
                                }

                                static bool Equal(byte[] first, byte[] second)
                                {
                                    return first.Length == second.Length && first.Zip(second).All(item => item.First == item.Second);
                                }

                                static async Task<byte[]> ComputeFileHashAsync(string fileName, CancellationToken cancellationToken)
                                {
                                    var stream = File.OpenRead(fileName);
                                    await using (stream.ConfigureAwait(false))
                                    {
                                        return await ComputeHashAsync(stream, cancellationToken).ConfigureAwait(false);
                                    }
                                }

                                static async Task<byte[]> ComputeArrayHashAsync(byte[] data, CancellationToken cancellationToken)
                                {
                                    var stream = new MemoryStream(data);
                                    await using (stream.ConfigureAwait(false))
                                    {
                                        return await ComputeHashAsync(stream, cancellationToken).ConfigureAwait(false);
                                    }
                                }

                                [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA5351:Do Not Use Broken Cryptographic Algorithms", Justification = "This is a file hash")]
                                static Task<byte[]> ComputeHashAsync(Stream stream, CancellationToken cancellationToken)
                                {
                                    return System.Security.Cryptography.MD5.Create().ComputeHashAsync(stream, cancellationToken);
                                }
                            }
                            else
                            {
                                var output = File.OpenWrite(outputPath);
                                await using (output.ConfigureAwait(false))
                                {
                                    await stream.CopyToAsync(output, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
                                }
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

    private static Stream GetManifestResourceStream(string name) => typeof(PostgresBuilderExtensions).Assembly.GetManifestResourceStream(typeof(PostgresBuilderExtensions), name) ?? throw new InvalidOperationException();

    private static IEnumerable<string> GetContainerfileContents(bool tle, bool plrust, bool pldotnet)
    {
        return GetArguments(tle, plrust, pldotnet)
            .Concat(GetBuildInstructions(tle, plrust, pldotnet))
            .Concat(GetInstructions(tle, plrust, pldotnet));

        static IEnumerable<string> GetContainerfileLines(string name)
        {
            using var reader = new StreamReader(GetManifestResourceStream($"{name}.Containerfile"));

            while (reader.ReadLine() is { } line)
            {
                yield return line;
            }
        }

        static IEnumerable<string> GetArguments(bool tle, bool plrust, bool pldotnet)
        {
            yield return $"ARG REGISTRY={DefaultRegistry}";
            yield return $"ARG IMAGE={DefaultImage}";
            yield return $"ARG TAG={DefaultTag}";

            if (tle)
            {
                yield return "ARG TLE_BRANCH=main";
            }

            if (plrust)
            {
                yield return "ARG PL_RUST_BRANCH=main";
            }

            if (pldotnet)
            {
                yield return "ARG PL_DOTNET_BRANCH=master";
            }
        }

        static IEnumerable<string> GetBuildInstructions(bool tle, bool plrust, bool pldotnet)
        {
            var zscaler = ZScaler.IsRunning;

            // build the TLE extension
            if (tle)
            {
                yield return string.Empty;
                foreach (var line in GetContainerfileLines($"{nameof(tle)}.build"))
                {
                    yield return line;

                    if (zscaler && line.StartsWith("FROM", StringComparison.OrdinalIgnoreCase))
                    {
                        // insert ZScaler lines
                        yield return string.Empty;
                        foreach (var zscalerLine in ZScaler.GetContainerfileLines())
                        {
                            yield return zscalerLine;
                        }
                    }
                }
            }

            // build PL/Rust
            if (plrust)
            {
                yield return string.Empty;
                foreach (var line in GetContainerfileLines($"{nameof(plrust)}.build"))
                {
                    yield return line;

                    if (zscaler && line.StartsWith("FROM", StringComparison.OrdinalIgnoreCase))
                    {
                        // insert ZScaler lines
                        yield return string.Empty;
                        foreach (var zscalerLine in ZScaler.GetContainerfileLines())
                        {
                            yield return zscalerLine;
                        }
                    }
                }
            }

            // build PL/Dotnet
            if (pldotnet)
            {
                yield return string.Empty;
                foreach (var line in GetContainerfileLines($"{nameof(pldotnet)}.build"))
                {
                    yield return line;

                    if (zscaler && line.StartsWith("FROM", StringComparison.OrdinalIgnoreCase))
                    {
                        // insert ZScaler lines
                        yield return string.Empty;
                        foreach (var zscalerLine in ZScaler.GetContainerfileLines())
                        {
                            yield return zscalerLine;
                        }
                    }
                }
            }
        }

        static IEnumerable<string> GetInstructions(bool tle, bool plrust, bool pldotnet)
        {
            // build the actual container
            yield return string.Empty;
            yield return "FROM ${REGISTRY}/${IMAGE}:${TAG}";

            if (ZScaler.IsRunning)
            {
                // insert ZScaler lines
                yield return string.Empty;
                foreach (var line in ZScaler.GetContainerfileLines())
                {
                    yield return line;
                }
            }

            if (tle)
            {
                yield return string.Empty;
                foreach (var line in GetContainerfileLines(nameof(tle)))
                {
                    yield return line;
                }
            }

            if (plrust)
            {
                yield return string.Empty;
                foreach (var line in GetContainerfileLines(nameof(plrust)))
                {
                    yield return line;
                }
            }

            if (pldotnet)
            {
                yield return string.Empty;
                foreach (var line in GetContainerfileLines(nameof(pldotnet)))
                {
                    yield return line;
                }
            }
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

    private sealed record DotnetAnnotation(string Branch) : PostgresAnnotation;
}