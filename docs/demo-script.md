# Recruiter Demo Script

This is the concise demonstration path for the **runtime-verified** Secure Employee Portal portfolio build. It is designed for a portfolio review or interview and should take roughly five to eight minutes.

## 1. Start with the engineering problem

Explain the project in one sentence:

> Secure Employee Portal is a Blazor and ASP.NET Core identity and access-management application that combines employee self-service with protected security administration.

Then establish the scope boundary: it is intentionally not a complete HR product. The focus is authentication, authorization, account state and auditable protected workflows.

## 2. Register and sign in as an Employee

Show a normal employee registration and sign-in flow.

Key points to explain:

- users are persisted with ASP.NET Core Identity and Entity Framework Core;
- passwords are hashed and verified by Identity, not custom application code;
- public registration receives the least-privileged `Employee` role;
- suspended and disabled accounts are rejected even when the password is valid.

## 3. Show the protected employee workspace

Open the dashboard and point out that the page is using the real authenticated user rather than mock identity text.

Show:

- profile/security navigation;
- attendance state;
- access-request entry point;
- the Secure Assistant.

For the Secure Assistant, demonstrate one supported question and its deep link to a real workflow. Explain that it is deterministic C# guidance with no external AI API and no permission-changing authority.

## 4. Exercise one persisted employee workflow

Clock in, then show the attendance page/history. If appropriate for the demo environment, clock out later in the sequence.

Explain the state rules:

- one open attendance session at a time;
- no clock-out without an open session;
- account state is rechecked before protected attendance writes.

## 5. Submit an access request

Create a narrow employee request for protected access.

Explain that the request itself does not grant permissions. It becomes a pending record that requires an Administrator decision.

## 6. Switch to Administrator

Open the security-administration area.

Show:

- user search/filtering;
- account status controls;
- role management;
- access-request review;
- security events and audit logs.

Key security points:

- only the fixed Employee / Manager / Administrator roles are assignable;
- an administrator cannot suspend, disable or re-role their own account through the UI;
- protected role/status changes invalidate the target user's security stamp;
- administrators cannot review their own access requests.

## 7. Review the employee request

Approve or reject the request and show the recorded outcome.

Then return to the employee view and show that the decision is visible there.

Explain that the workflow is stateful and a completed request cannot simply be reviewed a second time.

## 8. Show the audit evidence

Open Security Events and Audit Logs.

Explain the distinction:

- security events record authentication/account-security activity;
- audit logs record important protected administrator actions.

Point out that passwords, reset tokens and application secrets are deliberately excluded from those records.

## 9. End on engineering quality

Close with the release/testing story:

- Release build treats compiler/analyzer warnings as errors;
- the verified Release build completed with 0 warnings and 0 errors;
- **74 / 74 automated tests passed**;
- CI and the local verification script fail if the discovered test inventory is not exactly 74;
- Entity Framework reported no pending model changes;
- the SQL-backed Playwright journey passed across employee and administrator workflows;
- 26 runtime screenshots were captured with zero document-level horizontal-overflow failures;
- browser testing complements automated tests because it exposed interaction/layout issues that source checks alone did not reproduce.

The full evidence record is in `docs/runtime-verification.md`, with the interface gallery in `docs/interface-gallery.md`.
