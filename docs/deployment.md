# Deployment Readiness

The application is prepared for an ASP.NET Core host such as Azure App Service with a SQL Server-compatible hosted database such as Azure SQL.

## Required hosted configuration

The production environment must provide these values securely rather than commit them to Git:

```text
ConnectionStrings__DefaultConnection
SeedAdmin__Email
SeedAdmin__Password
Smtp__Host
Smtp__Port
Smtp__EnableSsl
Smtp__From
Smtp__Username
Smtp__Password
```

`SeedAdmin` is optional after an administrator account has been established through a controlled process. Do not leave reusable bootstrap credentials configured unnecessarily.

## Database migrations

Production defaults to:

```text
Database__ApplyMigrationsOnStartup=false
```

Recommended release process:

1. Back up the hosted database when appropriate.
2. Set the production connection string securely.
3. Apply migrations as a deployment step with `dotnet ef database update`.
4. Start/restart the application.
5. Check `/health`.
6. Complete the smoke-test flows in `docs/testing.md`.

If a deliberately controlled environment needs startup migrations, set `Database__ApplyMigrationsOnStartup=true`. This is an opt-in rather than the production default.

## LocalDB guard

The repository's development connection string uses SQL Server LocalDB because the original project was built on Windows. LocalDB is explicitly rejected outside the Development environment, preventing an accidental hosted deployment from silently using the wrong database configuration.

## Build command

```powershell
# Create a Release build before deployment.
dotnet publish SecureEmployeePortal.csproj --configuration Release --output .\publish
```

## Health check

The app exposes:

`/health`

Use that endpoint for a simple post-deployment availability check. Authentication and database-backed workflow tests still need the smoke-test checklist; a healthy process alone does not prove authorization behaviour.

## GitHub Actions

`.github/workflows/ci.yml` performs tool/dependency restore, a warnings-as-errors Release build, automated tests, and an EF Core pending-model-change check for pushes and pull requests to `main`. It intentionally does not deploy automatically because the final Azure subscription, application name and credentials belong to the repository owner and should not be guessed or embedded in source control.
