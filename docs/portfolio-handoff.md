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

The current release-candidate source represents **74 expected automated test executions** plus the manual security/workflow checklist in `docs/testing.md`. Earlier Windows checkpoints established that the project can build, migrate and run on the intended stack, but the newest candidate must still pass the exact-package Release build, automated tests, EF model/migration check and manual browser workflow before it is described as runtime-verified.

The final portfolio/GitHub release should be cut only from the exact source package that passes that gate.
