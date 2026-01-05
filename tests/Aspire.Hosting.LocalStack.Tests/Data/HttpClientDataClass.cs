namespace Aspire.Hosting.LocalStack.Tests.Data;

public class HttpClientDataClass : TUnit.Core.Interfaces.IAsyncInitializer, IAsyncDisposable
{
    public HttpClient HttpClient { get; private set; } = new();

    public async Task InitializeAsync()
    {
        HttpClient = (GlobalHooks.App ?? throw new NullReferenceException()).CreateHttpClient("localstack-apiservice");
        if (GlobalHooks.NotificationService is { } notificationService)
        {
            await notificationService.WaitForResourceAsync("localstack-apiservice", KnownResourceStates.Running).WaitAsync(TimeSpan.FromSeconds(300));
        }
    }

    public async ValueTask DisposeAsync()
    {
        await Console.Out.WriteLineAsync("And when the class is finished with, we can clean up any resources.");
        GC.SuppressFinalize(this);
    }
}