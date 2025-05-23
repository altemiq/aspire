# Altemiq.Aspire.Hosting.ZScaler library

Provides extension methods for a .NET Aspire AppHost to support the internet proxy ZScaler.

## Getting started

### Install the package

In your AppHost project, install the Altemiq .NET Aspire ZScaler Hosting library with [NuGet](https://www.nuget.org):

```dotnetcli
dotnet add package Altemiq.Aspire.Hosting.ZScaler
```

## Usage examples

In the _AppHost.cs_ file of `AppHost`, configure a container resource with certificates from ZScaler:

```csharp
var zscaler = ZScaler.IsRunning();

var certificates = ZScaler.ExportCertificateTo(Path.Combine(Path.GetTempPath(), "zscaler.{0}.crt"));

// insert certificates into container
```