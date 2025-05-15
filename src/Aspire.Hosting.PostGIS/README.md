# Altemiq.Aspire.Hosting.PostGIS library

Provides extension methods and resource definitions for a .NET Aspire AppHost to configure a PostGIS resource.

## Getting started

### Install the package

In your AppHost project, install the Altemiq .NET Aspire PostGIS Hosting library with [NuGet](https://www.nuget.org):

```dotnetcli
dotnet add package Altemiq.Aspire.Hosting.PostGIS
```

## Usage example

Then, in the _AppHost.cs_ file of `AppHost`, add a PostGIS resource and consume the connection using the following methods:

```csharp
var db = builder.AddPostGis("pgsql").AddDatabase("mydb");

var myService = builder.AddProject<Projects.MyService>()
                       .WithReference(db);
```