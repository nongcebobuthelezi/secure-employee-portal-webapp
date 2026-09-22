# Portfolio Handoff

## What this project demonstrates

- C# and .NET 10
- Blazor Web App / Razor components
- ASP.NET Core Identity
- Entity Framework Core
- SQL Server data persistence
- authentication and password security
- role-based authorization
- protected routes and account status
- employee self-service workflows
- functioning in-dashboard Secure Assistant
- administrator user management
- clock-in/clock-out business rules
- access-request review
- security events and audit logs
- automated business-rule tests
- CI configuration
- deployment-aware configuration and documentation

## Suggested portfolio description

**Secure Employee Portal** is a Blazor and ASP.NET Core employee identity and access-management application. It uses ASP.NET Core Identity and Entity Framework Core to provide database-backed registration, authentication, role-based authorization, account administration, employee attendance, access-request review, a built-in support assistant, security events and auditable administrator actions.

## Recruiter demo path

A concise demonstration should show:

1. Employee registration/sign-in.
2. Protected MyNIF-inspired dashboard and real authenticated identity.
3. Functional Secure Assistant answering a portal question and deep-linking to the correct workflow.
4. Employee clock-in/out and persisted attendance.
5. Employee access request.
6. Administrator user management and role/account controls.
7. Administrator approval/rejection of the access request.
8. Security-event and audit-log evidence of what happened.

This path demonstrates the project's engineering story without clicking through every screen.

## Honest limitations

- The application is a portfolio project, not a certified production HR/security platform.
- A hosted SQL connection and SMTP provider must be supplied for production deployment.
- MFA and external identity providers are outside the current scope.

## Verification truth

The portfolio build completed the exact release gate on **22 September 2026**:

- warnings-as-errors Release build passed with 0 warnings and 0 errors;
- **74 / 74 automated tests passed** with exact inventory enforcement;
- EF Core reported no pending model changes;
- the app started against an isolated SQL Server 2022 instance;
- the Playwright browser journey passed across employee, authorization, administrator, manager and responsive flows;
- **26 verified runtime screenshots** were captured with 0 document-level horizontal-overflow failures.

See `docs/runtime-verification.md` and `docs/interface-gallery.md` for the evidence record.
