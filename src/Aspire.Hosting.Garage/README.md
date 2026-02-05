# Altemiq.Aspire.Hosting.Garage Library

Adds Garage as a local resource to simulate AWS S3.

## Getting started

### Install the package

In your AppHost project, install the Altemiq .NET Aspire Garage Hosting library with [NuGet](https://www.nuget.org):

```dotnetcli
dotnet add package Altemiq.Aspire.Hosting.Garage
```

## Usage examples

Then, in the _AppHost.cs_ file of `AppHost`, add a Garage resource using the following methods:

```csharp
var region = Amazon.RegionEndpoint.APSoutheast2;

var profiles = builder.AddAWSProfileConfig()
    .AsConfigurationFile()
    .WithProfile(ProfileName);

var garage = builder
    .AddGarage("garage")
    .WithReference(profiles)
    .WithDataVolume();
```