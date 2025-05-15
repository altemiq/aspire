# Altemiq.Aspire.Hosting.MinIO Library

Adds MinIO as a local resource to simulate AWS S3.

## Getting started

### Install the package

In your AppHost project, install the Altemiq .NET Aspire MinIO Hosting library with [NuGet](https://www.nuget.org):

```dotnetcli
dotnet add package Altemiq.Aspire.Hosting.MinIO
```

## Usage examples

Then, in the _AppHost.cs_ file of `AppHost`, add a MinIO resource using the following methods:

```csharp
var region = Amazon.RegionEndpoint.APSoutheast2;

var profiles = builder.AddAWSProfileConfig()
    .AsConfigurationFile()
    .WithProfile(ProfileName);

var minio = builder
    .AddMinIO("minio")
    .WithReference(profiles)
    .WithDataVolume();
```

This also supports sending messages to a RabbitMQ resource for AWS S3 notifications

```csharp
var rabbitmq = builder
    .AddRabbitMQ("rabbitmq");

_ = minio
    .WithAmqpReference(rabbitmq);
```