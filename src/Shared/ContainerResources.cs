// -----------------------------------------------------------------------
// <copyright file="ContainerResources.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting.ApplicationModel;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

/// <summary>
/// Helpers for container resources.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Public API")]
internal static partial class ContainerResources
{
    private static readonly Dictionary<string, object?> EmptyDictionary = [];

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
    /// <param name="logger">The logger.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the execution.</returns>
    public static async Task<int> ExecAsync(
        this IResource containerResource,
        IServiceProvider services,
        IDictionary<string, object?> env,
        IEnumerable<object> args,
        ILogger? logger = default,
        CancellationToken cancellationToken = default) => await containerResource.ExecAsync(await GetContainerRuntimeAsync(services, cancellationToken).ConfigureAwait(continueOnCapturedContext: false), env, args, logger, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);

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
        var arguments = Create("exec")
            .Concat(FromEnv(env))
            .Concat(Create(containerResource.GetContainerName()))
            .Concat(args)
            .ToList();
        return RunProcessAsync(containerRuntime, arguments, logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance, cancellationToken);

        static IEnumerable<object> Create(object item)
        {
            yield return item;
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
    /// Gets the container runtime.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The asynchronous result.</returns>
    /// <exception cref="InvalidOperationException">The container runtime could not be found.</exception>
    public static async Task<string> GetContainerRuntimeAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        return await GetContainerRuntimeCoreAsync(serviceProvider, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException();

        static async Task<string?> GetContainerRuntimeCoreAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            return await GetDcpInfoAsync(serviceProvider, cancellationToken).ConfigureAwait(false) is { } dcpInfo
                && dcpInfo.GetType().GetProperty("Containers")?.GetValue(dcpInfo) is { } containers
                ? containers.GetType().GetProperty("Runtime")?.GetValue(containers) as string
                : default;

            static async Task<object?> GetDcpInfoAsync(IServiceProvider provider, CancellationToken cancellationToken)
            {
                var dcpDependencyCheckServiceType = typeof(DistributedApplication).Assembly.GetType("Aspire.Hosting.Dcp.IDcpDependencyCheckService") ?? throw new InvalidOperationException();
                var dcpDependencyCheckService = provider.GetRequiredService(dcpDependencyCheckServiceType);

                if (dcpDependencyCheckServiceType.GetMethod("GetDcpInfoAsync")?.Invoke(dcpDependencyCheckService, [false, cancellationToken]) is Task task)
                {
                    await task.ConfigureAwait(false);

                    return task.GetType().GetProperty("Result")?.GetValue(task);
                }

                return null;
            }
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

        foreach (var arg in GetArgs(args))
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

        LogStartingProcess(logger, process.StartInfo.FileName, string.Join(' ', GetArgs(args, hideSecrets: true)));

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

        return process.ExitCode;

        static IEnumerable<string> GetArgs(IEnumerable<object> args, bool hideSecrets = false)
        {
            foreach (var arg in NotNull(args.Select(arg => GetArgValue(arg, hideSecrets))))
            {
                yield return arg;
            }

            static string? GetArgValue(object arg, bool hideSecrets)
            {
                return arg switch
                {
                    ParameterResource { Secret: true } when hideSecrets => "********",
                    ParameterResource parameterResource => parameterResource.Value,
                    string stringValue => stringValue,
                    not null => arg.ToString(),
                    _ => null,
                };
            }

            static IEnumerable<string> NotNull(IEnumerable<string?> values)
            {
                foreach (var value in values.Where(static value => value is not null))
                {
                    yield return value!;
                }
            }
        }
    }

    [LoggerMessage(LogLevel.Information, Message = "{Data}")]
    private static partial void LogOutputData(ILogger logger, string data);

    [LoggerMessage(LogLevel.Error, Message = "{Data}")]
    private static partial void LogErrorData(ILogger logger, string data);

    [LoggerMessage(LogLevel.Debug, Message = "Starting process {FileName} {Arguments}")]
    private static partial void LogStartingProcess(ILogger logger, string fileName, string arguments);
}