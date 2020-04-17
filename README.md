# AspNetCore.Profiler.Sample

## Tutorials

- [[ASP.NET Core] Profiling - MiniProfiler](https://karatejb.blogspot.com/2020/04/aspnet-core-profiling-miniprofiler.html)





## Database Migration

### Create migration

1. Create new migration file

```
$ cd src/AspNetCore.Profiler.Mvc
$ dotnet ef  --project ../AspNetCore.Profiler.Dal --startup-project . migrations add <migration_name>
```

2. Update database

```
$ cd src/AspNetCore.Profiler.Mvc
$ dotnet ef  --project ../AspNetCore.Profiler.Dal --startup-project . database update
```
