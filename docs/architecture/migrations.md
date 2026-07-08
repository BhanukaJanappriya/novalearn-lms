# Database Migrations

The initial EF Core migration is generated once the .NET 9 SDK is installed (it cannot be created
without the SDK). The `ApplicationDbContextInitialiser` applies migrations automatically at startup
in Development.

## Create the initial migration

```bash
# from backend/
dotnet tool install --global dotnet-ef      # once
dotnet ef migrations add InitialIdentity \
  --project src/NovaLearn.Persistence \
  --startup-project src/NovaLearn.API \
  --output-dir Migrations
```

## Apply migrations manually

```bash
dotnet ef database update \
  --project src/NovaLearn.Persistence \
  --startup-project src/NovaLearn.API
```

> Integration tests do **not** use migrations — they build the schema from the model with
> `EnsureCreated()` against a disposable Postgres container, so they run before any migration exists.
