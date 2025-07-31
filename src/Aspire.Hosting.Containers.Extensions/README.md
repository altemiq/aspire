# Altemiq.Aspire.Hosting.Containers.Extensions library

This integration contains extensions for containers for .NET Aspire.

## Getting started

### Install the package

In your AppHost project, install the Altemiq .NET Aspire Containers Hosting library with [NuGet](https://www.nuget.org):

```dotnetcli
dotnet add package Altemiq.Aspire.Hosting.Containers.Extensions
```

## Usage examples

Then, in the _AppHost.cs_ file of `AppHost`, add the container build environment.

```csharp
_ = builder.AddContainer("mycontainer", "myimage")
    .WithContainerfile("Context");

_ = builder.AddContainerBuildEnvironment("container-build");

await builder.Build().RunAsync(CancellationToken.None).ConfigureAwait(false);
```