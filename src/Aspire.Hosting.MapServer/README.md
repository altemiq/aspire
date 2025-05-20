# Altemiq.Aspire.Hosting.MapServer library

Provides extension methods and resource definitions for a .NET Aspire AppHost to configure a MapServer resource.

## Getting started

### Install the package

In your AppHost project, install the Altemiq .NET Aspire MapServer Hosting library with [NuGet](https://www.nuget.org):

```dotnetcli
dotnet add package Altemiq.Aspire.Hosting.MapServer
```

## Usage examples

Then, in the _AppHost.cs_ file of `AppHost`, add a MapServer resource and consume the connection using the following methods:

```csharp
var postgres = builder.AddPostgres("db1");

var database = postgres.AddDatabase("db1-map-server");

var mapserver = builder.AddMapServer("map-server", "../../map/mapserver")
    .WithReference(database)
    .WaitFor(database);
```