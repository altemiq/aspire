# Altemiq.Aspire.Hosting.PostgreSQL library

This integration contains extensions for the [PostgreSQL hosting package](https://nuget.org/packages/Aspire.Hosting.PostgreSQL) for .NET Aspire.

## Getting started

### Install the package

In your AppHost project, install the Altemiq .NET Aspire PostgreSQL Hosting library with [NuGet](https://www.nuget.org):

```dotnetcli
dotnet add package Altemiq.Aspire.Hosting.PostgreSQL.Extensions
```

## Usage examples

### Adding [Trusted Language Extensions for PostgreSQL (pg_tle)](https://github.com/aws/pg_tle)

In the _AppHost.cs_ file of `AppHost`, configure a PostgreSQL resource with TLE using the following methods:

```csharp
var db = builder
    .AddPostgres("pgsql")
    .WithTle()
    .AddDatabase("mydb");

var myService = builder.AddProject<Projects.MyService>()
                       .WithReference(db);
```

#### Adding a TLE extension to a database

This can compile an example extension from the `pg_tle` repository for use in the database.

The available extensions are
* [client_lockout](https://github.com/aws/pg_tle/tree/main/examples/client_lockout)
* [enforce_password_expiration](https://github.com/aws/pg_tle/tree/main/examples/enforce_password_expiration)
* [uuid_v7](https://github.com/aws/pg_tle/tree/main/examples/uuid_v7)

In the _AppHost.cs_ file of `AppHost`, configure a PostgreSQL database resource to use a TLE extension using the following method:

```csharp
_ = db.WithTleExtension("uuid_v7");
```

### Adding [PL/Rust](https://github.com/tcdi/plrust)

In the _AppHost.cs_ file of `AppHost`, configure a PostgreSQL resource with PL/Rust using the following methods:

```csharp
var db = builder
    .AddPostgres("pgsql")
    .WithPlRust()
    .AddDatabase("mydb");

var myService = builder.AddProject<Projects.MyService>()
                       .WithReference(db);
```

### Adding [PL/.NET](https://github.com/Brick-Abode/pldotnet)

In the _AppHost.cs_ file of `AppHost`, configure a PostgreSQL resource with PL/.NET using the following methods:

```csharp
var db = builder
    .AddPostgres("pgsql")
    .WithPlDotnet()
    .AddDatabase("mydb");

var myService = builder.AddProject<Projects.MyService>()
                       .WithReference(db);
```

_*Postgres, PostgreSQL and the Slonik Logo are trademarks or registered trademarks of the PostgreSQL Community Association of Canada, and used with their permission._