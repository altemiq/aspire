// -----------------------------------------------------------------------
// <copyright file="ContainerResourceExtensions.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

/// <summary>
/// <see cref="ContainerResource"/> extensions.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Public API")]
public static partial class ContainerResourceExtensions
{
    private static readonly Dictionary<string, object?> EmptyDictionary = [];

    /// <summary>
    /// Causes .NET Aspire to build the specified container image from a Containerfile.
    /// </summary>
    /// <typeparam name="T">Type parameter specifying any type derived from <see cref="ContainerResource"/>.</typeparam>
    /// <param name="builder">The builder.</param>
    /// <param name="contextPath">Path to be used as the context for the container image build.</param>
    /// <param name="containerfilePath">Override path for the Containerfile if it is not in the <paramref name="contextPath"/>.</param>
    /// <param name="stage">The stage representing the image to be published in a multi-stage Containerfile.</param>
    /// <returns>The <see cref="IResourceBuilder{T}"/>.</returns>
    /// <remarks>
    /// <para>
    /// When this method is called an annotation is added to the <see cref="ContainerResource"/> that specifies the context path and
    /// Dockerfile path to be used when building the container image. These details are then used by the orchestrator to build the image
    /// before using that image to start the container.
    /// </para>
    /// <para>
    /// Both the <paramref name="contextPath"/> and <paramref name="containerfilePath"/> are relative to the AppHost directory unless
    /// they are fully qualified. If the <paramref name="containerfilePath"/> is not provided, the path is assumed to be Containerfile relative
    /// to the <paramref name="contextPath"/>.
    /// </para>
    /// <para>
    /// When generating the manifest for deployment tools, the <see cref="WithContainerfile{T}(IResourceBuilder{T}, string, string?, string?)"/>
    /// method results in an additional attribute being added to the `container.v0` resource type which contains the configuration
    /// necessary to allow the deployment tool to build the container image prior to deployment.
    /// </para>
    /// <example>
    /// Creates a container called <c>mycontainer</c> with an image called <c>myimage</c>.
    /// <code language="csharp">
    /// var builder = DistributedApplication.CreateBuilder(args);
    ///
    /// builder.AddContainer("mycontainer", "myimage")
    ///        .WithContainerfile("path/to/context");
    ///
    /// builder.Build().Run();
    /// </code>
    /// </example>
    /// </remarks>
    public static IResourceBuilder<T> WithContainerfile<T>(this IResourceBuilder<T> builder, string contextPath, string? containerfilePath = null, string? stage = null)
        where T : ContainerResource => builder.WithDockerfile(contextPath, containerfilePath ?? "Containerfile", stage);

    /// <summary>
    /// Executes the arguments against the container.
    /// </summary>
    /// <param name="containerResource">The container resource.</param>
    /// <param name="services">The services to get the container runtime.</param>
    /// <param name="args">The arguments.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the execution.</returns>
    public static Task<int> ExecAsync(
        this IResource containerResource,
        IServiceProvider services,
        IEnumerable<object> args,
        CancellationToken cancellationToken = default) => containerResource.ExecAsync(services, args, GetLoggerForResource(services, containerResource), cancellationToken);

    /// <summary>
    /// Executes the arguments against the container.
    /// </summary>
    /// <param name="containerResource">The container resource.</param>
    /// <param name="services">The services to get the container runtime.</param>
    /// <param name="args">The arguments.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the execution.</returns>
    public static Task<int> ExecAsync(
        this IResource containerResource,
        IServiceProvider services,
        IEnumerable<object> args,
        ILogger? logger = default,
        CancellationToken cancellationToken = default) => containerResource.ExecAsync(services, EmptyDictionary, args, logger, cancellationToken);

    /// <summary>
    /// Executes the arguments against the container.
    /// </summary>
    /// <param name="containerResource">The container resource.</param>
    /// <param name="services">The services to get the container runtime.</param>
    /// <param name="env">The environment variables.</param>
    /// <param name="args">The arguments.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the execution.</returns>
    public static async Task<int> ExecAsync(
        this IResource containerResource,
        IServiceProvider services,
        IDictionary<string, object?> env,
        IEnumerable<object> args,
        CancellationToken cancellationToken = default) => await containerResource.ExecAsync(services, env, args, GetLoggerForResource(services, containerResource), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);

    /// <summary>
    /// Executes the arguments against the container.
    /// </summary>
    /// <param name="containerResource">The container resource.</param>
    /// <param name="services">The services to get the container runtime.</param>
    /// <param name="env">The environment variables.</param>
    /// <param name="args">The arguments.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the execution.</returns>
    public static async Task<int> ExecAsync(
        this IResource containerResource,
        IServiceProvider services,
        IDictionary<string, object?> env,
        IEnumerable<object> args,
        ILogger? logger = default,
        CancellationToken cancellationToken = default) => await containerResource.ExecAsync(await ContainerRuntime.GetNameAsync(services, cancellationToken).ConfigureAwait(continueOnCapturedContext: false), env, args, logger, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);

    /// <summary>
    /// Executes the arguments against the container.
    /// </summary>
    /// <param name="containerResource">The container resource.</param>
    /// <param name="containerRuntime">The container runtime.</param>
    /// <param name="args">The arguments.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the execution.</returns>
    public static Task<int> ExecAsync(
        this IResource containerResource,
        string containerRuntime,
        IEnumerable<object> args,
        ILogger? logger = default,
        CancellationToken cancellationToken = default) => ExecAsync(containerResource, containerRuntime, EmptyDictionary, args, logger, cancellationToken);

    /// <summary>
    /// Executes the arguments against the container.
    /// </summary>
    /// <param name="containerResource">The container resource.</param>
    /// <param name="containerRuntime">The container runtime.</param>
    /// <param name="env">The environment variables.</param>
    /// <param name="args">The arguments.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the execution.</returns>
    public static Task<int> ExecAsync(
        this IResource containerResource,
        string containerRuntime,
        IDictionary<string, object?> env,
        IEnumerable<object> args,
        ILogger? logger = default,
        CancellationToken cancellationToken = default)
    {
        var arguments = Create("container", "exec")
            .Concat(FromEnv(env))
            .Concat(Create(containerResource.GetContainerName()))
            .Concat(args)
            .ToList();

        return RunProcessAsync(containerRuntime, arguments, logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance, cancellationToken);

        static IEnumerable<object> Create(params IEnumerable<object> item)
        {
            return item;
        }

        static IEnumerable<object> FromEnv(IDictionary<string, object?> env)
        {
            foreach (var e in env)
            {
                yield return "--env";
                yield return $"{e.Key}={e.Value}";
            }
        }
    }

    /// <summary>
    /// Copy files/folders to a container from the local filesystem.
    /// </summary>
    /// <param name="containerResource">The container resource.</param>
    /// <param name="services">The services.</param>
    /// <param name="source">The source on the local filesystem.</param>
    /// <param name="destination">The destination on the container.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the command.</returns>
    public static Task<int> CopyToAsync(
        this IResource containerResource,
        IServiceProvider services,
        string source,
        string destination,
        CancellationToken cancellationToken = default) => containerResource.CopyToAsync(services, source, destination, GetLoggerForResource(services, containerResource), cancellationToken);

    /// <summary>
    /// Copy files/folders to a container from the local filesystem.
    /// </summary>
    /// <param name="containerResource">The container resource.</param>
    /// <param name="services">The services.</param>
    /// <param name="source">The source on the local filesystem.</param>
    /// <param name="destination">The destination on the container.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the command.</returns>
    public static async Task<int> CopyToAsync(
        this IResource containerResource,
        IServiceProvider services,
        string source,
        string destination,
        ILogger? logger = default,
        CancellationToken cancellationToken = default) => await containerResource.CopyToAsync(await ContainerRuntime.GetNameAsync(services, cancellationToken).ConfigureAwait(false), source, destination, logger, cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Copy files/folders to a container from the local filesystem.
    /// </summary>
    /// <param name="containerResource">The container resource.</param>
    /// <param name="containerRuntime">The container runtime.</param>
    /// <param name="source">The source on the local filesystem.</param>
    /// <param name="destination">The destination on the container.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the command.</returns>
    public static Task<int> CopyToAsync(
        this IResource containerResource,
        string containerRuntime,
        string source,
        string destination,
        ILogger? logger = default,
        CancellationToken cancellationToken = default) => RunProcessAsync(containerRuntime, ["cp", source, $"{containerResource.GetContainerName()}:{destination}"], logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance, cancellationToken);

    /// <summary>
    /// Copy files/folders from a container to the local filesystem.
    /// </summary>
    /// <param name="containerResource">The container resource.</param>
    /// <param name="services">The services.</param>
    /// <param name="source">The source on the container.</param>
    /// <param name="destination">The destination on the local filesystem.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the command.</returns>
    public static Task<int> CopyFromAsync(
        this IResource containerResource,
        IServiceProvider services,
        string source,
        string destination,
        CancellationToken cancellationToken = default) => containerResource.CopyFromAsync(services, source, destination, GetLoggerForResource(services, containerResource), cancellationToken);

    /// <summary>
    /// Copy files/folders from a container to the local filesystem.
    /// </summary>
    /// <param name="containerResource">The container resource.</param>
    /// <param name="services">The services.</param>
    /// <param name="source">The source on the container.</param>
    /// <param name="destination">The destination on the local filesystem.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the command.</returns>
    public static async Task<int> CopyFromAsync(
        this IResource containerResource,
        IServiceProvider services,
        string source,
        string destination,
        ILogger? logger = default,
        CancellationToken cancellationToken = default) => await containerResource.CopyFromAsync(await ContainerRuntime.GetNameAsync(services, cancellationToken).ConfigureAwait(false), source, destination, logger, cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Copy files/folders from a container to the local filesystem.
    /// </summary>
    /// <param name="containerResource">The container resource.</param>
    /// <param name="containerRuntime">The container runtime.</param>
    /// <param name="source">The source on the container.</param>
    /// <param name="destination">The destination on the local filesystem.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the command.</returns>
    public static Task<int> CopyFromAsync(
        this IResource containerResource,
        string containerRuntime,
        string source,
        string destination,
        ILogger? logger = default,
        CancellationToken cancellationToken = default) => RunProcessAsync(containerRuntime, ["copy", $"{containerResource.GetContainerName()}:{source}", destination], logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance, cancellationToken);

    /// <summary>
    /// Gets the container name.
    /// </summary>
    /// <param name="resource">The container resource.</param>
    /// <returns>The container name.</returns>
    public static string GetContainerName(this IResource resource) => resource.GetContainerNames().First();

    /// <summary>
    /// Gets the container names.
    /// </summary>
    /// <param name="resource">The container resource.</param>
    /// <returns>The container names.</returns>
    public static IEnumerable<string> GetContainerNames(this IResource resource)
    {
        return (GetResolvedResourceNamesMethodInfo().Invoke(null, [resource]) as IEnumerable<string>) ?? [];

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3011:Reflection should not be used to increase accessibility of classes, methods, or fields", Justification = "Checked")]
        static System.Reflection.MethodInfo GetResolvedResourceNamesMethodInfo()
        {
            return typeof(ResourceExtensions).GetMethod("GetResolvedResourceNames", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic) ?? throw new KeyNotFoundException();
        }
    }

    /// <summary>
    /// Runs the build.
    /// </summary>
    /// <param name="application">The application.</param>
    public static void RunBuild(this DistributedApplication application) => RunBuildAsync(application, CancellationToken.None).Wait();

    /// <summary>
    /// Runs the build.
    /// </summary>
    /// <param name="application">The application.</param>
    /// <param name="logger">The logger.</param>
    public static void RunBuild(this DistributedApplication application, ILogger? logger) => RunBuildAsync(application, logger, CancellationToken.None).Wait();

    /// <summary>
    /// Runs this instance asynchronously.
    /// </summary>
    /// <param name="application">The application.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The async task.</returns>
    public static Task RunBuildAsync(this DistributedApplication application, CancellationToken cancellationToken = default) => RunBuildAsync(application, application.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Build"), cancellationToken);

    /// <summary>
    /// Runs this instance asynchronously.
    /// </summary>
    /// <param name="application">The application.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The async task.</returns>
    public static async Task RunBuildAsync(this DistributedApplication application, ILogger? logger, CancellationToken cancellationToken = default)
    {
        logger ??= Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;

        var model = application.Services.GetRequiredService<DistributedApplicationModel>();
        foreach (var annotations in model.GetContainerResources().Select(resource => resource.Annotations))
        {
            // ensure we pull all the images.
            if (annotations.OfType<ContainerImagePullPolicyAnnotation>().SingleOrDefault() is { } existingAnnotation)
            {
                annotations.Remove(existingAnnotation);
            }

            annotations.Add(new ContainerImagePullPolicyAnnotation { ImagePullPolicy = ImagePullPolicy.Always });
        }

        var eventing = application.Services.GetRequiredService<Eventing.IDistributedApplicationEventing>();
        await eventing.PublishAsync(new BeforeStartEvent(application.Services, model), cancellationToken).ConfigureAwait(false);

        string? containerRuntime = default;
        foreach (var resource in model.Resources.Where(r => r.HasAnnotationOfType<DockerfileBuildAnnotation>()))
        {
            await eventing.PublishAsync(new BeforeResourceStartedEvent(resource, application.Services), cancellationToken).ConfigureAwait(false);

            containerRuntime ??= await ContainerRuntime.GetNameAsync(application.Services, cancellationToken).ConfigureAwait(false);

            if (resource is IResourceWithEnvironment env)
            {
                _ = await env.GetEnvironmentVariableValuesAsync().ConfigureAwait(false);
            }

            // get the docker information
            if (!resource.TryGetLastAnnotation<DockerfileBuildAnnotation>(out var dockerfileBuildAnnotation))
            {
                continue;
            }

            // get the build args
            var arguments = new List<object> { "build" };
            foreach (var buildArgument in dockerfileBuildAnnotation.BuildArguments)
            {
                arguments.Add("--build-arg");
                arguments.Add($"{buildArgument.Key}={buildArgument.Value}");
            }

            foreach (var buildSecret in dockerfileBuildAnnotation.BuildSecrets)
            {
                arguments.Add("--secret");
                arguments.Add($"{buildSecret.Key}={buildSecret.Value}");
            }

            arguments.Add("--file");
            arguments.Add(dockerfileBuildAnnotation.DockerfilePath);

            if (dockerfileBuildAnnotation.Stage is { } stage)
            {
                arguments.Add("--target");
                arguments.Add(stage);
            }

            arguments.Add("--pull");

            // get the tag information
            var tag = GetTag(resource);
            if (tag is not null)
            {
                arguments.Add("--tag");
                arguments.Add(tag);
            }

            arguments.Add(dockerfileBuildAnnotation.ContextPath);

            LogBuilding(logger, tag);

            _ = await RunProcessAsync(containerRuntime, arguments, logger, cancellationToken).ConfigureAwait(false);

            static string? GetTag(IResource resource)
            {
                if (!resource.TryGetLastAnnotation<ContainerImageAnnotation>(out var containerImageAnnotation))
                {
                    return default;
                }

                var tagBuilder = new System.Text.StringBuilder();
                if (containerImageAnnotation.Registry is { } registry)
                {
                    tagBuilder.Append(registry).Append('/');
                }

                tagBuilder.Append(containerImageAnnotation.Image);

                if (containerImageAnnotation.Tag is { } tag)
                {
                    tagBuilder.Append(':').Append(tag);
                }

                return tagBuilder.ToString();
            }
        }
    }

    /// <summary>
    /// Sets the build policy for the container resource.
    /// </summary>
    /// <typeparam name="T">The resource type.</typeparam>
    /// <param name="builder">Builder for the container resource.</param>
    /// <param name="buildPolicy">The build policy behavior for the container resource.</param>
    /// <returns>The <see cref="IResourceBuilder{T}"/>.</returns>
    public static IResourceBuilder<T> WithImageBuildPolicy<T>(this IResourceBuilder<T> builder, ImageBuildPolicy buildPolicy)
        where T : ContainerResource
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.WithAnnotation(new ContainerImageBuildPolicyAnnotation { ImageBuildPolicy = buildPolicy }, ResourceAnnotationMutationBehavior.Replace);
    }

    /// <summary>
    /// Gets a value indicating whether the container should be built.
    /// </summary>
    /// <param name="resource">The resource.</param>
    /// <param name="services">The services.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><see langword="true"/> if the resource should be built; otherwise <see langword="false"/>.</returns>
    public static async ValueTask<bool> ShouldBuildAsync(this IResource resource, IServiceProvider services, CancellationToken cancellationToken = default)
    {
        var imageBuildPolicy = ImageBuildPolicy.Default;
        if (resource.TryGetLastAnnotation<ContainerImageBuildPolicyAnnotation>(out var imageBuildPolicyAnnotation))
        {
            imageBuildPolicy = imageBuildPolicyAnnotation.ImageBuildPolicy;
        }

        // if we're always building, or publishing
        if (imageBuildPolicy is not ImageBuildPolicy.Missing || services.GetService<DistributedApplicationExecutionContext>() is { IsPublishMode: true })
        {
            return true;
        }

        // if the container doesn't exist
        return resource.TryGetLastAnnotation<ContainerImageAnnotation>(out var containerImageAnnotation)
               && !await ContainerRuntime.ImageExistsAsync(services, containerImageAnnotation.Image, containerImageAnnotation.Tag, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Updates the <see cref="DockerfileBuildAnnotation"/> paths to follow symlinks.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>The application builder.</returns>
    public static IDistributedApplicationBuilder UpdateDockerfileBuildSymLinks(this IDistributedApplicationBuilder builder)
    {
        foreach (var resourceBuilder in builder.Resources
                     .Where(static resource => resource.HasAnnotationOfType<DockerfileBuildAnnotation>())
                     .Select(builder.CreateResourceBuilder))
        {
            _ = resourceBuilder.UpdateDockerfileBuildSymLinks();
        }

        return builder;
    }

    /// <summary>
    /// Updates the <see cref="DockerfileBuildAnnotation"/> paths to follow symlinks.
    /// </summary>
    /// <typeparam name="T">The type of resource.</typeparam>
    /// <param name="builder">The builder.</param>
    /// <returns>The application builder.</returns>
    public static IResourceBuilder<T> UpdateDockerfileBuildSymLinks<T>(this IResourceBuilder<T> builder)
        where T : IResource
    {
        if (!builder.Resource.TryGetLastAnnotation<DockerfileBuildAnnotation>(out var annotation))
        {
            return builder;
        }

        var resolvedContextPath = ResolveJunctionOrSymlink(annotation.ContextPath);
        var resolvedDockerfilePath = ResolveJunctionOrSymlink(annotation.DockerfilePath);
        if (!PathsEqual(annotation.ContextPath, resolvedContextPath) || !PathsEqual(annotation.DockerfilePath, resolvedDockerfilePath))
        {
            _ = builder.WithAnnotation(CreateAnnotation(annotation, resolvedContextPath, resolvedDockerfilePath), ResourceAnnotationMutationBehavior.Replace);
        }

        return builder;

        static string ResolveJunctionOrSymlink(string path)
        {
            return GetFileSystemInfo(path) switch
            {
                { LinkTarget: { } linkTarget } => linkTarget,
                DirectoryInfo { Name: { } name, Parent.FullName: { } parentName } => Path.Combine(ResolveJunctionOrSymlink(parentName), name),
                FileInfo { Name: { } name, Directory.FullName: { } parentName } => Path.Combine(ResolveJunctionOrSymlink(parentName), name),
                _ => path,
            };

            static FileSystemInfo? GetFileSystemInfo(string path)
            {
                if (Directory.Exists(path))
                {
                    return new DirectoryInfo(path);
                }

                return File.Exists(path)
                    ? new FileInfo(path)
                    : default;
            }
        }

        static bool PathsEqual(string first, string second)
        {
            return string.Equals(first, second, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
        }

        static DockerfileBuildAnnotation CreateAnnotation(DockerfileBuildAnnotation annotation, string resolvedContextPath, string resolvedDockerfilePath)
        {
            var newAnnotation = new DockerfileBuildAnnotation(resolvedContextPath, resolvedDockerfilePath, annotation.Stage);
            foreach (var buildArgument in annotation.BuildArguments)
            {
                newAnnotation.BuildArguments.Add(buildArgument.Key, buildArgument.Value);
            }

            foreach (var buildSecrets in annotation.BuildSecrets)
            {
                newAnnotation.BuildSecrets.Add(buildSecrets.Key, buildSecrets.Value);
            }

            return newAnnotation;
        }
    }

    private static async Task<int> RunProcessAsync(string containerRuntime, ICollection<object> args, ILogger logger, CancellationToken cancellationToken)
    {
        var process = new System.Diagnostics.Process
        {
            StartInfo = new(containerRuntime)
            {
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
            },
        };

        await foreach (var arg in GetArgsAsync(args, cancellationToken: cancellationToken).ConfigureAwait(false))
        {
            process.StartInfo.ArgumentList.Add(arg);
        }

        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data is { } data)
            {
                LogOutputData(logger, data);
            }
        };

        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data is { } data)
            {
                LogErrorData(logger, data);
            }
        };

        LogStartingProcess(logger, process.StartInfo.FileName, await JoinAsync(' ', GetArgsAsync(args, hideSecrets: true, cancellationToken), cancellationToken).ConfigureAwait(false));

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

        return process.ExitCode;

        static async IAsyncEnumerable<string> GetArgsAsync(IEnumerable<object> args, bool hideSecrets = false, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            foreach (var arg in args)
            {
                if (await GetArgValueAsync(arg, hideSecrets, cancellationToken).ConfigureAwait(false) is { } value)
                {
                    yield return value;
                }
            }

            static async ValueTask<string?> GetArgValueAsync(object arg, bool hideSecrets, CancellationToken cancellationToken)
            {
                return arg switch
                {
                    ParameterResource { Secret: true } when hideSecrets => "********",
                    IValueProvider valueProvider => await valueProvider.GetValueAsync(cancellationToken).ConfigureAwait(false),
                    string stringValue => stringValue,
                    not null => arg.ToString(),
                    _ => null,
                };
            }
        }

        static async Task<string> JoinAsync(char separator, IAsyncEnumerable<string> values, CancellationToken cancellationToken)
        {
            var enumerator = values.GetAsyncEnumerator(cancellationToken);
            if (!await enumerator.MoveNextAsync().ConfigureAwait(false))
            {
                return string.Empty;
            }

            var stringBuilder = new System.Text.StringBuilder(enumerator.Current);
            while (await enumerator.MoveNextAsync().ConfigureAwait(false))
            {
                stringBuilder.Append(separator);
                stringBuilder.Append(enumerator.Current);
            }

            return stringBuilder.ToString();
        }
    }

    [LoggerMessage(LogLevel.Information, Message = "Building {Tag}")]
    private static partial void LogBuilding(ILogger logger, string? tag);

    [LoggerMessage(LogLevel.Information, Message = "{Data}")]
    private static partial void LogOutputData(ILogger logger, string data);

    [LoggerMessage(LogLevel.Error, Message = "{Data}")]
    private static partial void LogErrorData(ILogger logger, string data);

    [LoggerMessage(LogLevel.Debug, Message = "Starting process {FileName} {Arguments}")]
    private static partial void LogStartingProcess(ILogger logger, string fileName, string arguments);

    private static ILogger GetLoggerForResource(IServiceProvider services, IResource resource) => services.GetRequiredService<ResourceLoggerService>().GetLogger(resource);
}