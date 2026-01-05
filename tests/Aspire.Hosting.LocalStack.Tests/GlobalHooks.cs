[assembly: Retry(3)]
[assembly: System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]

namespace Aspire.Hosting.LocalStack.Tests;

public class GlobalHooks
{
    private static readonly
#if NET9_0_OR_GREATER
        Lock
#else
        object
#endif
        LinuxDockerLock = new();

    private static bool hasLinuxDocker;

    public static bool LinuxDocker
    {
        get
        {
            lock (LinuxDockerLock)
            {
                if (hasLinuxDocker)
                {
                    return field;
                }
                
                hasLinuxDocker = true;
                return field = HasLinuxContainers();
            }
        }
    }

    public static DistributedApplication? App { get; private set; }

    [Before(TestSession)]
    public static async Task SetUp()
    {
        if (LinuxDocker)
        {
            // Arrange
            var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.LocalStack_AppHost>();
            appHost.Services.ConfigureHttpClientDefaults(clientBuilder => { clientBuilder.AddStandardResilienceHandler(); });

            App = await appHost.BuildAsync();
            await App.StartAsync();
        }
    }

    [After(TestSession)]
    public static void CleanUp()
    {
        App?.Dispose();
    }

    private static bool HasLinuxContainers()
    {
        return System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux)
               || string.Equals(GetOSType(), "linux", StringComparison.OrdinalIgnoreCase);

        static string? GetOSType()
        {
            var process = new System.Diagnostics.Process
            {
                StartInfo =
                {
                    FileName = "docker",
                    ArgumentList = { "info", "--format", "\"{{.OSType}}\"" },
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                },
            };

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

                var output = process.StandardOutput.ReadToEnd();

                process.WaitForExit();

                return output.Trim('\r', '\n', '\'');
            }
        }
    }
}