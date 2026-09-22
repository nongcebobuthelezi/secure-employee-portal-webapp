# Runtime Verification Record

## Verified build

The portfolio release was verified on **22 September 2026** using GitHub-hosted Linux runners, .NET 10, an isolated SQL Server 2022 container and Playwright Chromium.

Application/runtime verification commit:

**5bcee9a4dae1738adacfee7bb7a9768de838e969**

Subsequent repository commits publish documentation and the verified screenshot gallery; they do not change the verified application behaviour.

## Automated .NET gate

The CI gate completed successfully with:

- Release build: **passed**
- Compiler/analyzer warnings: **0**
- Build errors: **0**
- Automated tests: **74 passed / 0 failed / 0 skipped**
- Enforced test inventory: **74 / 74**
- EF Core pending model changes: **none**

The workflow is defined in [.github/workflows/ci.yml](../.github/workflows/ci.yml).

## SQL-backed browser gate

The runtime workflow:

1. restored and Release-built the portal;
2. started a clean SQL Server 2022 container;
3. launched the actual ASP.NET Core / Blazor application;
4. waited for the /health endpoint;
5. installed Playwright Chromium;
6. exercised the real UI through registration, authentication, employee workflows and protected administration;
7. captured the final interface gallery;
8. checked every captured route for document-level horizontal overflow.

The successful run produced **26 runtime screenshots** with **0 horizontal-overflow failures**.

The workflow and browser journey are defined in:

- [.github/workflows/runtime-e2e.yml](../.github/workflows/runtime-e2e.yml)
- [scripts/runtime-e2e.py](../scripts/runtime-e2e.py)

## Browser journey exercised

The verified flow covers:

- sign in;
- employee registration;
- forgot-password UI and neutral privacy response;
- employee dashboard;
- Secure Assistant;
- profile;
- account security;
- attendance clock-in and clock-out;
- access-request submission;
- account activity;
- help and support;
- employee denial from an administrator route;
- administrator dashboard;
- user management;
- administrator-created account UI;
- employee account administration;
- roles and permissions;
- access-request approval;
- security events;
- audit logs;
- manager team view;
- mobile sign in;
- mobile employee dashboard;
- mobile employee profile;
- mobile attendance;
- mobile administrator dashboard.

## Runtime defects found and repaired

The browser gate was intentionally allowed to fail while expanding coverage. It exposed issues that static/source checks did not reproduce reliably:

- an Interactive Server timing issue in the access-request automation, resolved by waiting for hydration and asserting live form values before submit;
- shared EF Core context contention during concurrent layout/page initialization, resolved by isolating authenticated shell/dashboard identity reads with IDbContextFactory<ApplicationDbContext>;
- a forgot-password privacy notice whose grid expected an icon that was not present, causing extreme text wrapping; the notice now uses the available width correctly.

The final runtime run passed after those fixes.

## Screenshot evidence

The complete verified gallery is in [Complete Interface Gallery](interface-gallery.md) and the underlying PNG files are stored under:

**docs/images/runtime/**

The committed runtime-e2e-report.json records each captured route and its measured horizontal overflow.
