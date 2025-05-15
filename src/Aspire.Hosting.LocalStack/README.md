# Altemiq.Aspire.Hosting.LocalStack Library

Adds LocalStack as a local resource to simulate AWS services.

## Getting started

### Install the package

In your AppHost project, install the Altemiq .NET Aspire LocalStack Hosting library with [NuGet](https://www.nuget.org):

```dotnetcli
dotnet add package Altemiq.Aspire.Hosting.LocalStack
```

## Usage examples

Then, in the _AppHost.cs_ file of `AppHost`, add a LocalStack resource using the following methods:

```csharp
var localstack = builder
    .AddLocalStack("localstack", regionEndPoint: Amazon.RegionEndpoint.APSoutheast2, services: LocalStackServices.Community.SimpleStorageService)
    .WithDataVolume();
```