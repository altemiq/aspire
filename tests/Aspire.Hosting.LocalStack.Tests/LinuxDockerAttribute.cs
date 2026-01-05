namespace Aspire.Hosting.LocalStack.Tests;

public class LinuxDockerAttribute() : SkipAttribute("This test is not supported on Windows when in GitHub actions")
{
    public override Task<bool> ShouldSkip(TestRegisteredContext context) => Task.FromResult(!GlobalHooks.LinuxDocker);
}