# Altemiq.Aspire.Hosting.AWS.Configuration library

Adds support for AWS configuration interaction in the AppHost.

This writes out a temporary config file with the specified profiles.

When using as a reference, it injects this configuration as the default configuration.

## Getting started

### Install the package

In your AppHost project, install the Altemiq .NET Aspire AWS Configuration Hosting library with [NuGet](https://www.nuget.org):

```dotnetcli
dotnet add package Altemiq.Aspire.Hosting.AWS.Configuration
```

## Usage examples

Then, in the _AppHost.cs_ file of `AppHost`, add AWS profiles using the following methods:

```csharp
const string ProfileName = "minio";
const string BucketProfileName = "bucket";

var awsProfiles = builder.AddAWSProfileConfig()
                         .AsConfigurationFile()
                         .WithProfile(ProfileName)
                         .WithProfile(BucketProfileName);

var awsConfig = builder.AddAWSSDKConfig()
                       .WithRegion(region)
                       .WithProfile(ProfileName);

var myService = builder.AddProject<Projects.MyService>()
                       .WithReference(awsProfiles)
                       .WithReference(awsConfig);
```