// -----------------------------------------------------------------------
// <copyright file="ContainerRuntime.cs" company="Altemiq">
// Copyright (c) Altemiq. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Aspire.Hosting;

using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Container helpers.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Public API")]
public static class ContainerRuntime
{
    private static string? containerRuntimeName;

    /// <summary>
    /// Tests whether the specified image exists.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <param name="image">The image.</param>
    /// <param name="tag">The tag.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><see langwor="true"/> if the image with the specified tag exists; otherwise <see langword="false"/>.</returns>
    public static async Task<bool> ImageExistsAsync(IServiceProvider services, string image, string? tag, CancellationToken cancellationToken = default) => await ImageExistsAsync(await GetNameAsync(services, cancellationToken).ConfigureAwait(continueOnCapturedContext: false), image, tag, cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Tests whether the specified image exists.
    /// </summary>
    /// <param name="containerRuntime">The container runtime.</param>
    /// <param name="image">The image.</param>
    /// <param name="tag">The tag.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><see langwor="true"/> if the image with the specified tag exists; otherwise <see langword="false"/>.</returns>
    public static async Task<bool> ImageExistsAsync(string containerRuntime, string image, string? tag, CancellationToken cancellationToken = default)
    {
        if (System.Diagnostics.Process.Start(
            new System.Diagnostics.ProcessStartInfo(containerRuntime)
            {
                ArgumentList = { "image", "inspect", tag is null ? image : $"{image}:{tag}" },
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
            }) is not { } process)
        {
            return false;
        }

        // ensure we listen to the output/error
        process.OutputDataReceived += (_, _) => { };
        process.ErrorDataReceived += (_, _) => { };
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

        return process.ExitCode is 0;
    }

    /// <summary>
    /// Gets the container runtime socket.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The container runtime socket if found; otherwise <see langword="null"/>.</returns>
    public static async Task<string?> GetSockAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default) => await GetSockAsync(await GetNameAsync(serviceProvider, cancellationToken).ConfigureAwait(false), cancellationToken).ConfigureAwait(false);

    /// <summary>
    /// Gets the container runtime socket.
    /// </summary>
    /// <param name="containerRuntime">The container runtime.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The container runtime socket if found; otherwise <see langword="null"/>.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S3267:Loops should be simplified with \"LINQ\" expressions", Justification = "These are async enumerable")]
    public static async Task<string?> GetSockAsync(string containerRuntime, CancellationToken cancellationToken = default)
    {
        await foreach (var sock in GetCompatibleSocksAsync(containerRuntime, cancellationToken).ConfigureAwait(false))
        {
            if (CheckSock(sock))
            {
                return sock;
            }
        }

        return default;

        static async IAsyncEnumerable<string?> GetCompatibleSocksAsync(string containerRuntime, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
        {
            switch (OperatingSystem.IsWindows(), containerRuntime)
            {
                case (true, "podman"):
                    yield return "/var/run/podman/podman.sock";
                    yield break;
                case (true, _):
                    yield return "/var/run/docker.sock";
                    yield break;
                case (_, "podman"):
                    yield return await GetPodmanSocket(cancellationToken).ConfigureAwait(false);
                    yield return await GetPodmanMachineSock(cancellationToken).ConfigureAwait(false);
                    yield break;
                case (_, _):
                    yield return GetDockerHost();
                    yield return await GetDockerSocket(cancellationToken).ConfigureAwait(false);
                    yield break;
            }

            static string? GetDockerHost()
            {
                return Environment.GetEnvironmentVariable("DOCKER_HOST") is { } value
                       && Uri.TryCreate(value, UriKind.Absolute, out var uri)
                       && uri is { Scheme: "unix" }
                    ? uri.LocalPath
                    : default;
            }

            static Task<string?> GetDockerSocket(CancellationToken cancellationToken)
            {
                return RunAsync("docker", ["context", "inspect", "--format", "'{{.Endpoints.docker.Host}}'"], cancellationToken);
            }

            static Task<string?> GetPodmanSocket(CancellationToken cancellationToken)
            {
                return RunAsync("podman", ["info", "--format", "'{{.Host.RemoteSocket.Path}}'"], cancellationToken);
            }

            static Task<string?> GetPodmanMachineSock(CancellationToken cancellationToken)
            {
                return RunAsync("podman", ["machine", "inspect", "--format", "'{{.ConnectionInfo.PodmanSocket.Path}}'"], cancellationToken);
            }

            static async Task<string?> RunAsync(string fileName, IEnumerable<string> args, CancellationToken cancellationToken)
            {
                var process = new System.Diagnostics.Process
                {
                    StartInfo =
                    {
                        FileName = fileName,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                    },
                };

                foreach (var arg in args)
                {
                    process.StartInfo.ArgumentList.Add(arg);
                }

                using (process)
                {
                    try
                    {
                        process.Start();
                    }
                    catch (System.ComponentModel.Win32Exception)
                    {
                        return default;
                    }

                    var output = await process.StandardOutput.ReadToEndAsync(cancellationToken).ConfigureAwait(false);

                    await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

                    return output.Trim('\r', '\n', '\'');
                }
            }
        }

        static bool CheckSock([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] string? path)
        {
            if (path is null)
            {
                return false;
            }

            if (OperatingSystem.IsWindows())
            {
                return true;
            }

            var endpoint = new System.Net.Sockets.UnixDomainSocketEndPoint(path);
            var socket = new System.Net.Sockets.Socket(
                System.Net.Sockets.AddressFamily.Unix,
                System.Net.Sockets.SocketType.Stream,
                System.Net.Sockets.ProtocolType.Unspecified);

            try
            {
                socket.Connect(endpoint);
                return true;
            }
            catch (System.Net.Sockets.SocketException)
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Gets the container runtime.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The asynchronous result.</returns>
    /// <exception cref="InvalidOperationException">The container runtime could not be found.</exception>
    public static async Task<string> GetNameAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        return containerRuntimeName ??= await GetNameCoreAsync(serviceProvider, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException();

        static async Task<string?> GetNameCoreAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            return await GetDcpInfoAsync(serviceProvider, cancellationToken).ConfigureAwait(false) is { } dcpInfo && dcpInfo.GetType().GetProperty("Containers")?.GetValue(dcpInfo) is { } containers
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
}