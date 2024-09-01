# AspNetCore.Profiler.Sample

## Features

- [MiniProfiler](https://github.com/MiniProfiler)
    - storage provider: Microsoft SQL Server 
    - authorization with JWT (optional)
- [OpenTelemetry](https://github.com/open-telemetry)
    - export: console


## Database Migration

### Install Entity Framework Core CLI

```s
dotnet tool install --global dotnet-ef
```

> See [Entity Framework Core tools reference - .NET Core CLI](https://learn.microsoft.com/en-us/ef/core/cli/dotnet#installing-the-tools) and [Managing Migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/managing?tabs=dotnet-core-cli)


### Create migration

#### 1. Create new migration file

```
$ cd src/AspNetCore.Profiler.Mvc
$ dotnet ef  --project ../AspNetCore.Profiler.Dal --startup-project . migrations add <migration_name>
```

#### 2. Update database

Use either the **environment variable** or dotnet-ef argument `--connection` to specify the DB connection string for executing the migration. 


`export ConnectionStrings__DefaultConnection`

```s
$ cd src/AspNetCore.Profiler.Mvc
$ export ConnectionStrings__DefaultConnection="Server=localhost\\SQLEXPRESS;Database=demo;Trusted_Connection=True;TrustServerCertificate=True;"
$ dotnet ef  --project ../AspNetCore.Profiler.Dal --startup-project . database update
```

`--connection <your_connection_string>`

```s
$ cd src/AspNetCore.Profiler.Mvc
$ dotnet ef  --project ../AspNetCore.Profiler.Dal --startup-project . database update --connection "Server=localhost\\SQLEXPRESS;Database=demo;Trusted_Connection=True;TrustServerCertificate=True;"
```

## Endpoints

|   | URL | Method | Description |
|:-:|:----|:------:|:------------|
| 1 | https://host/profiler/results                      | GET | Profiling result for the latest request. |
| 2 | https://host/profiler/results-index                | GET | Profiling results for stored requests. |
| 3 | https://host/profiler/results-list                 | GET | Profiling results in JSON for stored requests. |
| 4 | https://host/api/PaymentApi/TestOopeTelemetry/{id} | GET | Test Open Telemetry to show Trace and Span ID. |


## Reference

- [[ASP.NET Core] Profiling - MiniProfiler](https://karatejb.blogspot.com/2020/04/aspnet-core-profiling-miniprofiler.html)

