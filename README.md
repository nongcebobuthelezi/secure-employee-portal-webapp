<!-- Secure Employee Portal portfolio README -->

# Secure Employee Portal

A security-focused employee identity and access-management portal built with **C#**, **.NET 10**, **Blazor**, **ASP.NET Core Identity**, **Entity Framework Core**, and **SQL Server**.

![Blazor](https://img.shields.io/badge/Blazor-512BD4?logo=blazor&logoColor=white)
![C%23](https://img.shields.io/badge/C%23-239120?logo=csharp&logoColor=white)
![.NET](https://img.shields.io/badge/.NET_10-512BD4?logo=dotnet&logoColor=white)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-512BD4?logo=dotnet&logoColor=white)
![EF Core](https://img.shields.io/badge/Entity_Framework_Core-512BD4)
![Status](https://img.shields.io/badge/Status-Portfolio_Candidate-2F855A)


## Why This Project Exists

Secure Employee Portal represents protected internal software for a fictional organisation. It is deliberately **not** a complete HR platform. The project is centred on a smaller, more technically coherent question:

> Who is the user, have they proved their identity, and what are they allowed to access or change?

The result combines an employee self-service experience with a protected security-administration console while preserving the original MyNIF-inspired visual direction.

## Portfolio Features

### Identity and account security

- database-backed ASP.NET Core Identity users and roles;
- employee registration with validation and duplicate-account prevention;
- Identity-managed password hashing;
- secure login, logout and authenticated sessions;
- failed-login lockout support;
- neutral login and forgot-password responses;
- token-backed password reset;
- account-status checks for Active, Suspended and Disabled accounts;
- protected Blazor routes and periodic session revalidation;
- safe access-denied and production-error experiences.

### Employee self-service

- authenticated MyNIF-inspired dashboard with real employee identity;
- database-backed profile information and permitted profile editing;
- password change and personal security history;
- clock in / clock out;
- persisted attendance history and weekly summary;
- narrow protected-resource access-request workflow;
- employee-safe account activity timeline;
- functioning in-dashboard Secure Assistant for account, security, attendance, profile and access-request guidance, launchable from the dashboard or top-bar help control;
- concise Help & Support area.

### Roles and administration

- fixed `Employee`, `Manager` and `Administrator` roles;
- limited Manager team view;
- administrator overview with account/security metrics;
- user search and filtering;
- administrator-created employee accounts;
- account activation/restoration, suspension and disabling;
- role assignment and role changes;
- administrator self-lockout protection;
- access-request approval/rejection with confirmation;
- system security-event console;
- administrator audit-log console.

### Engineering quality

- Entity Framework Core migrations;
- short-lived database contexts through `IDbContextFactory`;
- explicit business-rule classes for account access, attendance and access requests;
- deterministic, testable C# support-assistant service with portal deep links and no external AI dependency;
- security and audit records for important workflows;
- loading, empty, success, validation and error states;
- automated MSTest business-rule, validation and source-regression tests;
- GitHub Actions CI for Release build and tests;
- deployment-aware configuration, health endpoint and documented Azure/SQL strategy;
- architecture, security, role-permission, testing and deployment documentation.

## Role Model

| Capability | Employee | Manager | Administrator |
| --- | :---: | :---: | :---: |
| Own dashboard/profile/security | ✓ | ✓ | ✓ |
| Own attendance and access requests | ✓ | ✓ | ✓ |
| Limited team view | — | ✓ | ✓ |
| User/account administration | — | — | ✓ |
| Role management | — | — | ✓ |
| Access-request review | — | — | ✓ |
| System security events / audit logs | — | — | ✓ |

See [`docs/role-permissions.md`](docs/role-permissions.md) for the full matrix.

## Security Decisions

- Passwords are created and verified by ASP.NET Core Identity; the application does not implement custom password hashing.
- Public registration receives the least-privileged `Employee` role.
- Authorization is enforced by ASP.NET Core rather than relying on hidden navigation controls.
- Suspended and disabled accounts are rejected at sign-in and revalidated during long-lived Blazor sessions.
- Logout is a POST operation protected by an antiforgery token.
- Reset tokens are not written to application logs.
- High-impact administrator actions create audit records.
- Production cookies require HTTPS.
- Production rejects the repository's LocalDB development connection string, forcing an explicit hosted SQL configuration.
- HSTS and defensive response headers are enabled for hosted environments.

See [`docs/security.md`](docs/security.md) for the rationale and deliberate limitations.

## Technology Stack

- C# 14
- .NET 10 / ASP.NET Core
- Blazor Web App and Razor components
- ASP.NET Core Identity
- Entity Framework Core 10
- SQL Server / SQL Server LocalDB for Windows development
- MSTest
- CSS
- GitHub Actions

The repository currently targets .NET SDK `10.0.301` and the `10.0.10` ASP.NET Core Identity / Entity Framework Core patch line.

## Local Development

### Prerequisites

- .NET SDK `10.0.301` or a compatible .NET 10 SDK
- SQL Server LocalDB on Windows for the included development connection string

### Restore and run

```powershell
# Restore the repository's local EF Core tool.
dotnet tool restore

# Restore and build the application.
dotnet restore
dotnet build

# Run the portal.
dotnet run
```

Development applies EF Core migrations automatically so the local schema can be reproduced from source.

## Development Administrator

Administrator credentials are intentionally not committed. Configure a local administrator with .NET user secrets:

```powershell
# Configure a local-only administrator identity.
dotnet user-secrets set "SeedAdmin:Email" "admin@secureportal.local"
dotnet user-secrets set "SeedAdmin:Password" "YOUR-STRONG-LOCAL-PASSWORD"
```

On application start, the seed service ensures the fixed roles exist and creates/promotes the configured administrator account.

## Password Reset in Development

When SMTP is not configured in Development, reset messages are written to:

`App_Data/development-mail/`

The directory is ignored by Git because each message contains a temporary reset link. Hosted environments require SMTP configuration through secure environment-specific settings.

## Testing

Run the final automated verification gate:

```powershell
# Restore, Release-build, test and check EF Core model/migration alignment.
# The process-scoped bypass avoids changing the machine-wide PowerShell policy.
powershell -ExecutionPolicy Bypass -File .\scripts\verify-portfolio.ps1
```

The manual end-to-end security/workflow checklist is in [`docs/testing.md`](docs/testing.md).

## Continuous Integration

`.github/workflows/ci.yml` restores the pinned EF Core tool, runs a warnings-as-errors Release build, executes the automated test project, and verifies that the EF Core model still matches the committed migrations for pushes and pull requests to `main`.

## Deployment

The application is prepared for an ASP.NET Core host such as Azure App Service plus a SQL Server-compatible hosted database such as Azure SQL. Production secrets and connection strings are external configuration, never repository values.

See [`docs/deployment.md`](docs/deployment.md) for the deployment gate, required environment variables, migration strategy and `/health` check.

## Architecture and Documentation

- [`docs/architecture.md`](docs/architecture.md)
- [`docs/security.md`](docs/security.md)
- [`docs/role-permissions.md`](docs/role-permissions.md)
- [`docs/testing.md`](docs/testing.md)
- [`docs/deployment.md`](docs/deployment.md)
- [`docs/release-checklist.md`](docs/release-checklist.md)
- [`docs/portfolio-handoff.md`](docs/portfolio-handoff.md)
- [`docs/portfolio-case-study.md`](docs/portfolio-case-study.md)
- [`docs/demo-script.md`](docs/demo-script.md)

## Current Interface

The final interface keeps the MyNIF-inspired navy, white and turquoise visual system while focusing the product on authenticated identity, account security, attendance, access requests and security administration. Stale pre-functionality screenshots are intentionally excluded from this release so the repository does not present UI that no longer matches the running application.

## Scope Boundary

The project deliberately excludes payroll, tax calculations, recruitment, performance management, complete benefits administration, complex shift scheduling, geographic/biometric tracking and a custom enterprise permission builder. That keeps the portfolio story centred on **identity, account access, authorization and auditable business workflows**.

## Verification Status

Two earlier Windows gates established the runtime baseline: the first verified package passed 14/14 tests, and the subsequent polished package passed 31/31 tests, matched the EF Core migration model, applied successfully to SQL Server LocalDB, and started locally. Manual testing then exposed a static-SSR login form-binding defect, which was repaired before this release.

The current release candidate contains **74 expected automated test executions** covering business rules, validation, authorization boundaries, malformed return URLs, transactional workflow guards, privacy boundaries, role-state recovery, credential/logging protections, internal navigation and the frozen authentication layout. The local verification script and CI both assert that exactly 74 executions are discovered, so deleting or accidentally disabling tests cannot silently reduce the release gate. This count describes the suite represented in source; it is not a claim that the newest candidate has already passed 74 runtime tests.

Run `scripts/verify-portfolio.ps1` and the manual workflow checklist in [`docs/testing.md`](docs/testing.md) on this exact package before deployment. Until that exact-package gate passes, the repository remains a **portfolio candidate** rather than claiming unverified production readiness.
