namespace Aspire.Hosting.LocalStack.Tests;

[LinuxDocker]
public class IntegrationTests
{
    [ClassDataSource<Data.S3ClientDataClass>]
    [Test]
    public async Task GetMirrorData(Data.S3ClientDataClass s3ClientData)
    {
        // Arrange
        var s3Client = await Assert.That(s3ClientData.S3Client).IsNotNull();
        // Act
        var response = await s3Client.ListObjectsV2Async(new() { BucketName = "test-data", Prefix = "textfile.txt" });
        // Assert
        await Assert.That(response.HttpStatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(response.KeyCount).IsEqualTo(1);
    }
}