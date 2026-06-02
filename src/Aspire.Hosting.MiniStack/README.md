# Altemiq.Aspire.Hosting.MiniStack Library

Adds MiniStack as a local resource to simulate AWS services.

## Getting started

### Install the package

In your AppHost project, install the Altemiq .NET Aspire MiniStack Hosting library with [NuGet](https://www.nuget.org):

```dotnetcli
dotnet add package Altemiq.Aspire.Hosting.MiniStack
```

## Usage examples

Then, in the _AppHost.cs_ file of `AppHost`, add a MiniStack resource using the following methods:

```csharp
var miniStack = builder
    .AddMiniStack("ministack", regionEndPoint: Amazon.RegionEndpoint.APSoutheast2, services: MiniStackServices.Community.SimpleStorageService)
    .WithDataVolume();
```

_*MiniStack logo from [miniStack](https://github.com/miniStack) github user._