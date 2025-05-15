# Altemiq.Aspire.Hosting.Grpc library

Adds support for gRPCui interaction in the AppHost.

## Getting started

### Install the package

In your AppHost project, install the Altemiq .NET Aspire gRPC Hosting library with [NuGet](https://www.nuget.org):

```dotnetcli
dotnet add package Altemiq.Aspire.Hosting.Grpc
```

## Usage examples

Then, in the _AppHost.cs_ file of `AppHost`, adds gRPCui to a reference, either using a container or executable.

```csharp
_ = builder.AddProject<Projects.MyService>(Name, Uri.UriSchemeHttp)
    .WithGrpcHealthCheck(Uri.UriSchemeHttp, Uri.UriSchemeHttp)
    .WithGrpcUI(
        (api, grpc) => grpc
            .WaitFor(api)
            .WithBasePath("/debug/grpcui"),
        executableName: $"{Name}-exe")
    .WithGrpcUI(
        (api, grpc) => grpc
            .WaitFor(api)
            .WithBasePath("/debug/grpcui"),
        containerName: $"{Name}-container");
```