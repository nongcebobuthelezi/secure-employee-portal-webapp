# Secure Employee Portal

A security-focused employee identity and access-management web application built with **C#**, **.NET 10**, **Blazor**, **ASP.NET Core Identity**, **Entity Framework Core** and **SQL Server**.

![CI](https://github.com/nongcebobuthelezi/secure-employee-portal-webapp/actions/workflows/ci.yml/badge.svg)
![Runtime E2E](https://github.com/nongcebobuthelezi/secure-employee-portal-webapp/actions/workflows/runtime-e2e.yml/badge.svg)
![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-14-239120?logo=csharp&logoColor=white)
![Blazor](https://img.shields.io/badge/Blazor-Web_App-512BD4?logo=blazor&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL_Server-2022-CC2927?logo=microsoftsqlserver&logoColor=white)
![Status](https://img.shields.io/badge/Status-Runtime_Verified-2F855A)

<table>
<tr>
<td width="50%"><img src="docs/images/runtime/04-employee-dashboard-desktop.png" alt="Employee dashboard"></td>
<td width="50%"><img src="docs/images/runtime/12-admin-dashboard-desktop.png" alt="Security administration dashboard"></td>
</tr>
<tr>
<td align="center"><strong>Employee self-service</strong></td>
<td align="center"><strong>Protected security administration</strong></td>
</tr>
</table>

## What this project demonstrates

Secure Employee Portal is a focused internal business application for employee identity, account access and auditable administration. It deliberately avoids becoming a full HR platform and instead centres the engineering story on one question:

> Who is the user, have they proved their identity, and what are they allowed to access or change?

The application combines real authentication, role-based authorization, account-state controls, employee self-service, persisted business workflows and protected administrator actions in one coherent .NET system.

## Verified release status

The current application was verified on **22 September 2026** using GitHub Actions, .NET 10, an isolated SQL Server 2022 instance and Playwright Chromium.

| Gate | Result |
| --- | --- |
| Release build | **Passed** |
| Compiler/analyzer warnings | **0** |
| Build errors | **0** |
| Automated tests | **74 passed / 0 failed / 0 skipped** |
| Enforced test inventory | **74 / 74** |
| EF Core pending model changes | **None** |
| SQL-backed browser journey | **Passed** |
| Runtime screenshots | **26** |
| Captured-route horizontal overflow | **0 failures** |

See [Runtime Verification](docs/runtime-verification.md) for the exact gate and [Complete Interface Gallery](docs/interface-gallery.md) for the full application screenshot set.

## Interface highlights

<table>
<tr>
<td width="50%"><img src="docs/images/runtime/01-login-desktop.png" alt="Secure sign-in"></td>
<td width="50%"><img src="docs/images/runtime/26-secure-assistant-desktop.png" alt="Secure Assistant"></td>
</tr>
<tr>
<td align="center">Secure authentication</td>
<td align="center">Built-in Secure Assistant</td>
</tr>
<tr>
<td><img src="docs/images/runtime/07-attendance-clocked-in-desktop.png" alt="Attendance workflow"></td>
<td><img src="docs/images/runtime/17-access-review-approved-desktop.png" alt="Administrator access review"></td>
</tr>
<tr>
<td align="center">Persisted attendance</td>
<td align="center">Auditable access review</td>
</tr>
</table>

The repository contains **all 26 verified runtime screenshots**, including authentication, employee, manager, administrator and mobile views.

## Core functionality

### Identity and account security

- ASP.NET Core Identity-backed users, roles, password hashing and lockout;
- employee registration with validation and duplicate-account prevention;
- secure login, POST/antiforgery logout and authenticated sessions;
- neutral login and forgot-password responses;
- token-backed password reset;
- Active, Suspended and Disabled account states;
- periodic session revalidation;
- fresh database-backed authorization checks on sensitive writes;
- safe access-denied and production-error experiences.

### Employee self-service

- authenticated employee dashboard;
- database-backed profile and permitted profile editing;
- password change and personal security activity;
- clock in / clock out with persisted attendance history;
- weekly attendance summary;
- protected-resource access requests;
- personal account activity timeline;
- Help & Support;
- a deterministic C# Secure Assistant with bounded portal guidance and real deep links.

### Roles and administration

- fixed Employee, Manager and Administrator roles;
- limited Manager team view;
- security-administration dashboard;
- user search and filtering;
- administrator-created employee accounts;
- account activation/restoration, suspension and disabling;
- role assignment and role changes;
- administrator self-lockout protection;
- access-request approval/rejection with confirmation;
- security-event console;
- audit-log console.

## Role model

| Capability | Employee | Manager | Administrator |
| --- | :---: | :---: | :---: |
| Own dashboard/profile/security | ✓ | ✓ | ✓ |
| Own attendance and access requests | ✓ | ✓ | ✓ |
| Limited team view | — | ✓ | ✓ |
| User/account administration | — | — | ✓ |
| Role management | — | — | ✓ |
| Access-request review | — | — | ✓ |
| System security events / audit logs | — | — | ✓ |

See [Role Permissions](docs/role-permissions.md) for the full matrix.

## Security decisions

- Password handling is delegated to ASP.NET Core Identity rather than custom cryptography.
- Public registration receives the least-privileged Employee role.
- Authorization is enforced on the server; hidden navigation is never treated as the security boundary.
- Account state is separate from role, so a valid password does not override suspension or disablement.
- Sensitive writes re-check the active user/administrator state against the database.
- Protected role/status changes invalidate the target user's security stamp.
- Logout uses POST plus antiforgery protection.
- Reset tokens and passwords are excluded from audit/security records.
- Production cookies require HTTPS.
- HSTS and defensive response headers are enabled outside Development.
- Production rejects the repository's LocalDB development connection string.

See [Security Decisions](docs/security.md) for the complete rationale and deliberate limitations.

## Architecture

```text
Browser
  ↓
Blazor Web App / Razor components
  ↓
ASP.NET Core authentication + authorization
  ↓
Application services + explicit business rules
  ↓
ASP.NET Core Identity + Entity Framework Core
  ↓
SQL Server / Azure SQL
```

The application uses static/server-rendered identity forms where cookie-writing flows require normal HTTP responses, and Interactive Server components for stateful workflows such as attendance, filters, access review and the Secure Assistant.

## Technology stack

- C# 14
- .NET 10 / ASP.NET Core
- Blazor Web App / Razor components
- ASP.NET Core Identity
- Entity Framework Core 10
- SQL Server / SQL Server LocalDB
- MSTest
- Playwright
- CSS
- GitHub Actions

The repository targets .NET SDK **10.0.301** and the **10.0.10** ASP.NET Core Identity / Entity Framework Core patch line.

## Testing and CI

Two complementary GitHub Actions workflows protect the project:

- **CI** — warnings-as-errors Release build, exact automated-test inventory, 74/74 tests and EF Core model/migration alignment.
- **Runtime E2E** — clean SQL Server, real application startup, health check, Playwright browser workflows, authorization boundaries, persistence and responsive screenshot verification.

Run the local verification gate with:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\verify-portfolio.ps1
```

See [Testing and Verification](docs/testing.md) for the wider manual security/workflow checklist.

## Local development

### Prerequisites

- .NET SDK 10.0.301 or a compatible .NET 10 SDK
- SQL Server LocalDB on Windows for the included development connection string

### Restore and run

```powershell
dotnet tool restore
dotnet restore
dotnet build
dotnet run
```

Development applies EF Core migrations automatically so the local schema can be reproduced from source.

### Development administrator

Administrator credentials are intentionally not committed. Configure a local administrator with .NET user secrets:

```powershell
dotnet user-secrets set "SeedAdmin:Email" "admin@secureportal.local"
dotnet user-secrets set "SeedAdmin:Password" "YOUR-STRONG-LOCAL-PASSWORD"
```

## Deployment readiness

The project is prepared for an ASP.NET Core host such as Azure App Service plus a SQL Server-compatible hosted database such as Azure SQL. Hosted credentials, SMTP settings and connection strings are external configuration and are never committed to the repository.

See [Deployment Readiness](docs/deployment.md) for the migration strategy, required environment variables and health-check process.

## Documentation

- [Complete Interface Gallery](docs/interface-gallery.md)
- [Runtime Verification Record](docs/runtime-verification.md)
- [Architecture](docs/architecture.md)
- [Security Decisions](docs/security.md)
- [Role Permissions](docs/role-permissions.md)
- [Testing and Verification](docs/testing.md)
- [Deployment Readiness](docs/deployment.md)
- [Portfolio Case Study](docs/portfolio-case-study.md)
- [Recruiter Demo Script](docs/demo-script.md)
- [Portfolio Handoff](docs/portfolio-handoff.md)
- [Release Checklist](docs/release-checklist.md)

## Scope boundary

The project deliberately excludes payroll, tax calculations, recruitment, performance management, full benefits administration, complex shift scheduling, biometric/location tracking and a custom enterprise permission builder.

It also does not claim MFA, external identity federation, penetration-test certification, enterprise compliance certification or a public production deployment.

Those are deliberate boundaries. The implemented scope is designed to demonstrate **identity, authorization, account security, persisted workflows and auditable administration deeply rather than superficially implementing an entire HR suite**.
