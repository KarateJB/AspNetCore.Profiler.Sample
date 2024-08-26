# AspNetCore.Profiler.Sample

## Tutorials

- [[ASP.NET Core] Profiling - MiniProfiler](https://karatejb.blogspot.com/2020/04/aspnet-core-profiling-miniprofiler.html)




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
$ export ConnectionStrings__DefaultConnection="Server=xxx\\SQLEXPRESS;Database=demo;Trusted_Connection=True;TrustServerCertificate=True;"
$ dotnet ef  --project ../AspNetCore.Profiler.Dal --startup-project . database update
```

`--connection <your_connection_string>`

```s
$ cd src/AspNetCore.Profiler.Mvc
$ dotnet ef  --project ../AspNetCore.Profiler.Dal --startup-project . database update --connection "Server=xxx\\SQLEXPRESS;Database=demo;Trusted_Connection=True;TrustServerCertificate=True;"
```
