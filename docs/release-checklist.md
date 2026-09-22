# Verified Release Checklist

This record captures the release gates completed for the Secure Employee Portal portfolio build on **22 September 2026**.

## 1. Source integrity

- [x] No source-controlled administrator, SMTP, database or API secrets.
- [x] Approved authentication layout regression guards remain intact.
- [x] Literal internal navigation resolves to implemented routes.
- [x] Employee / Manager / Administrator route boundaries match the documented role model.
- [x] One-time gallery-publishing automation was removed after the verified screenshots were committed.

## 2. Automated .NET gate

The GitHub CI workflow completed successfully on the verified application source:

- [x] .NET SDK/tool restore succeeded.
- [x] Dependency restore succeeded.
- [x] Release build succeeded with warnings treated as errors.
- [x] **74 / 74 automated test executions passed.**
- [x] Test inventory enforcement confirmed exactly 74 discovered executions.
- [x] EF Core reported **no pending model changes**.
- [x] Build completed with **0 warnings and 0 errors**.

Local equivalent:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\verify-portfolio.ps1
```

## 3. SQL-backed browser gate

The runtime workflow started an isolated SQL Server 2022 instance and exercised the actual .NET application with Playwright Chromium.

Verified flows include:

- [x] Registration → login → protected employee dashboard.
- [x] Secure Assistant.
- [x] Profile and security pages.
- [x] Attendance clock-in / clock-out.
- [x] Access-request submission.
- [x] Personal account activity.
- [x] Employee denial from an administrator route.
- [x] Administrator dashboard and user management.
- [x] Create-employee and employee-account administration screens.
- [x] Roles and permissions.
- [x] Access-request approval.
- [x] Security events and audit logs.
- [x] Manager team boundary.
- [x] Mobile employee and administrator views.

## 4. Visual gate

- [x] Desktop authentication retains the approved frozen composition.
- [x] Forgot-password privacy wording renders normally.
- [x] Authenticated desktop pages have no clipping/overlap issues in the verified captures.
- [x] Phone captures render without document-level horizontal overflow.
- [x] **26 runtime screenshots** were captured from the verified SQL-backed application.
- [x] Every captured route reported **0 px document-level horizontal overflow**.

See [Complete Interface Gallery](interface-gallery.md).

## 5. Documentation gate

- [x] README reflects implemented and runtime-verified behaviour.
- [x] Architecture, security, roles, testing and deployment docs are present.
- [x] Runtime verification evidence is documented.
- [x] Recruiter demo path matches the implemented application.
- [x] Portfolio case study matches the verified release.
- [x] Full screenshot gallery is committed under docs/images/runtime.

## 6. Release record

Verified runtime source commit:

**5bcee9a4dae1738adacfee7bb7a9768de838e969**

The repository commits after that point publish the verified gallery and documentation; they do not change the verified application behaviour.

This project is **runtime verified as a portfolio build**. It is not presented as a certified production security/HR platform or as a publicly deployed service.
