# AspNetCore.Profiler.Sample
Profiler sample code

# Database Migration

## Create migration

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
