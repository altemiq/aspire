namespace Aspire.Hosting.MiniStack.Tests.Data;

public class S3ClientDataClass : TUnit.Core.Interfaces.IAsyncInitializer, IAsyncDisposable
{
    public Amazon.S3.IAmazonS3? S3Client { get; private set; }

    public async Task InitializeAsync()
    {

        if (GlobalHooks.App is { ResourceNotifications: { } notificationService } app)
        {
            await notificationService.WaitForResourceAsync("ministack", KnownResourceStates.Running).WaitAsync(TimeSpan.FromSeconds(300));

            // get the actual resource once started
            this.S3Client = app.Services.GetRequiredKeyedAwsService<Amazon.S3.IAmazonS3>("ministack");
        }
    }

    public ValueTask DisposeAsync()
    {
        this.S3Client?.Dispose();
        GC.SuppressFinalize(this);
        return default;
    }
}