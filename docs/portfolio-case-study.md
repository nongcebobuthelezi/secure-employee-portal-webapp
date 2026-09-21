# Secure Employee Portal — Portfolio Case Study

## Project summary

Secure Employee Portal is a focused internal business application built to demonstrate secure employee identity, account access and auditable administration in a realistic .NET application.

The project intentionally avoids becoming a full HR platform. Its core engineering question is narrower:

> Who is the user, have they proved their identity, and what are they allowed to access or change?

That scope keeps the project centred on authentication, authorization, account state, employee self-service, protected administration and traceable business workflows.

## The problem being solved

Internal employee systems need more than a login screen. A credible access-management application must handle several connected concerns:

- users need database-backed identities rather than mock accounts;
- passwords must be handled by a proven identity system rather than custom cryptography;
- protected pages must enforce authorization on the server, not only hide links in the UI;
- an account may need to be suspended or disabled independently of its role;
- privileged changes should be traceable;
- employees need useful self-service workflows that exercise the same identity and authorization model;
- long-lived sessions must not remain trusted forever after a role or account-state change.

Secure Employee Portal addresses those concerns as one coherent application rather than as isolated demos.

## Technology and architecture

The release candidate uses:

- **C# 14** and **.NET 10**;
- **Blazor Web App / Razor components** for the interface;
- **ASP.NET Core Identity** for users, passwords, lockout, roles and security stamps;
- **Entity Framework Core 10** for persistence and migrations;
- **SQL Server / LocalDB** for the relational data model;
- **MSTest** for automated business-rule, validation and regression tests;
- **GitHub Actions** for the release build, test-inventory and EF model/migration gate.

The main runtime flow is:

```text
Browser
  ↓
Blazor Web App / Razor components
  ↓
ASP.NET Core authentication + authorization
  ↓
Application services and explicit business rules
  ↓
ASP.NET Core Identity + Entity Framework Core
  ↓
SQL Server
```

Server-rendered forms are retained where cookie-writing identity operations require them, while Interactive Server components are used for workflows that benefit from immediate UI updates such as attendance clocking and protected administration.

## Identity and account security

Public registration creates the least-privileged `Employee` role. ASP.NET Core Identity owns password hashing and verification, password requirements, lockout and reset tokens.

The application separates **role** from **account status**. The fixed account states are `Active`, `Suspended` and `Disabled`, so a valid password is not enough to enter the application when the business account itself is blocked.

Long-lived sessions are periodically revalidated against the current user record, account status and security stamp. Sensitive write operations also perform fresh checks before changing protected data, reducing the window in which a stale session could continue acting after suspension or role removal.

## Authorization model

The project uses three fixed application roles:

- `Employee`
- `Manager`
- `Administrator`

Employees receive their own dashboard, profile, security, attendance and access-request workflows. Managers inherit those capabilities and receive a deliberately narrow team view. Administrators receive security-focused user/account management, role assignment, access-request review, security-event inspection and audit-log inspection.

Authorization attributes and server-side checks are the real boundary. Sidebar visibility is treated as a usability concern rather than a security control.

## Employee workflows

The employee experience is intentionally more substantial than a protected landing page.

### Profile and security

Employees can view database-backed identity/profile information, update permitted personal fields, change their password and review their own account activity.

### Attendance

Employees can clock in and clock out against persisted attendance records. Business rules prevent invalid state transitions such as opening a second active attendance session or clocking out without an open session.

### Access requests

Employees can submit narrow requests for access to protected resources. The request creates a reviewable record; it does not automatically change the user's permissions.

### Secure Assistant

The dashboard includes a deterministic C# support assistant. It maps supported questions to bounded portal guidance and real internal routes. It does not call an external AI API and cannot grant permissions or bypass authorization.

## Administrator workflows

Administrators can:

- search and filter users;
- create employee accounts;
- suspend, disable and restore accounts;
- change a user's application role;
- review access requests;
- inspect security events;
- inspect auditable administrator actions.

The portal includes safeguards against self-lockout and self-review. An administrator cannot suspend/disable/re-role their own account through the UI, and an administrator cannot approve or reject an access request submitted by that same administrator account.

Account-status and role changes invalidate the target user's security stamp before the protected change is completed, so existing authenticated sessions are forced to revalidate.

## Auditability and privacy boundaries

Important authentication and security events are recorded separately from administrator audit logs.

Audit entries identify the actor, action, target, time and result without storing passwords, reset tokens or application secrets. The employee Account Activity page is deliberately personal: administrator actions concerning other employees do not leak into another user's personal activity timeline.

Development password-reset messages are written only to an ignored local directory when SMTP is unavailable. Hosted environments require explicit SMTP configuration.

## Testing and release protection

The current source represents **74 expected automated test executions**. The suite covers business rules, input validation, role/account boundaries, malformed return URLs, transactional workflow guards, privacy boundaries and source-level regression protections.

The release gate is designed to fail if the discovered test inventory is anything other than 74, preventing deleted or accidentally disabled tests from making CI appear green with a silently smaller suite.

Additional source-level protections cover areas that are easy to regress in Blazor/Identity applications, including:

- the approved authentication layout;
- static-SSR form wiring for identity operations;
- cookie-writing render-mode boundaries;
- fresh database-backed authorization checks on sensitive writes;
- literal internal-link routing;
- reset-token containment;
- mobile action-link containment;
- Entity Framework workflow/migration coverage.

The release candidate is deliberately described as **source complete / pre-runtime verified** until the exact package is built and the full automated + manual runtime gate is reproduced on that same source.

## UI and responsive design

The interface uses a restrained MyNIF-inspired business visual system: navy navigation, light neutral surfaces, white content cards and turquoise/blue accents.

The approved authentication composition is protected from short-viewport regression. Authenticated screens have also been statically exercised across phone, tablet and desktop viewport widths, including privileged administration pages.

The visual design supports the security story rather than replacing it: the same account identity, role and account-state model drives what the user can actually access.

## Important engineering decisions

### Use ASP.NET Core Identity instead of custom authentication

The project deliberately delegates password hashing, reset tokens, lockout, cookies, roles and security stamps to ASP.NET Core Identity rather than rebuilding security primitives.

### Keep role and account state separate

`Employee`, `Manager` and `Administrator` describe authority. `Active`, `Suspended` and `Disabled` describe whether the business account may currently be used. Treating them separately produces clearer rules and safer administration.

### Use explicit rules for workflow transitions

Attendance and access-request transitions are represented by explicit C# rules rather than being buried only inside UI event handlers. This makes those behaviours easier to test and reason about.

### Keep the product scope narrow

Payroll, recruitment, performance management, benefits, complex scheduling and biometric/location tracking are intentionally excluded. The project is strongest when it demonstrates identity, authorization, account security and auditable protected workflows deeply rather than pretending to be an entire HR suite.

## What this project demonstrates to a reviewer

A short review of the project can demonstrate:

1. database-backed employee registration and authentication;
2. protected routes and real authenticated identity;
3. employee profile/security workflows;
4. persisted attendance with state rules;
5. an employee access request;
6. Administrator account and role controls;
7. Administrator request review;
8. security events and audit evidence;
9. automated and manual release gates around the same workflows.

## Deliberate limitations

The project does not claim MFA, external identity federation, penetration-test certification, enterprise compliance certification, production SMTP infrastructure or a currently verified public production deployment.

Those are not hidden gaps. They are explicit scope boundaries. The release should only make stronger runtime/deployment claims after the exact release-candidate package passes the documented build, test, EF and browser workflow gate.
