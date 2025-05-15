# Altemiq.Aspire.Hosting.AWS.S3 library

Adds support for S3 interaction in the AppHost.

This supports creating buckets, including setting notifications.

## Getting started

### Install the package

In your AppHost project, install the Altemiq .NET Aspire AWS S3 Hosting library with [NuGet](https://www.nuget.org):

```dotnetcli
dotnet add package Altemiq.Aspire.Hosting.AWS.S3
```

## Usage examples

Then, in the _AppHost.cs_ file of `AppHost`, configure an AWS resource using the following methods:

```csharp
// resource is either AWS or a service that simulates S3 

// Add the client to the builder so that it can access the resource
_ = builder.AddAmazonS3(resource, "https");

// Ensure that the buckets exist
_ = resource.EnsureBucket(
    "aspire",
    Amazon.S3.EventType.ObjectCreatedAll,
    Amazon.S3.EventType.ObjectRemovedAll,
    Amazon.S3.EventType.ObjectRestoreAll);
```