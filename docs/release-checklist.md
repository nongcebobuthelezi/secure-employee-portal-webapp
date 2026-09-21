# Release Candidate Checklist

Use this checklist for the final Secure Employee Portal release. It deliberately separates **source complete** from **runtime verified** so the portfolio never claims evidence that has not been reproduced on the exact release package.

## 1. Source integrity

- [ ] Working tree is clean.
- [ ] No source-controlled administrator, SMTP, database or API secrets.
- [ ] `git diff --check` passes.
- [ ] Approved authentication layout regression guards remain intact.
- [ ] Literal internal navigation resolves to implemented routes.
- [ ] Employee / Manager / Administrator route boundaries match `docs/role-permissions.md`.

## 2. Automated .NET gate

Run:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\verify-portfolio.ps1
```

Required result on the exact release candidate:

- [ ] .NET SDK/tool restore succeeds.
- [ ] Dependency restore succeeds.
- [ ] Release build succeeds with warnings treated as errors.
- [ ] All **74 expected automated test executions** pass, and the verification gate confirms exactly 74 executions were discovered.
- [ ] `dotnet ef migrations has-pending-model-changes` reports no pending model changes.

## 3. Manual browser gate

Start the exact package and complete every item in `docs/testing.md`, including:

- [ ] Registration → login → protected dashboard → logout.
- [ ] Wrong/unknown login neutrality and lockout behaviour.
- [ ] Profile persistence and password change/reset.
- [ ] Employee, Manager and Administrator authorization boundaries.
- [ ] Suspend/restore and role-change session invalidation.
- [ ] Attendance duplicate/open-session guards.
- [ ] Access-request submit/review/double-review/self-review guards.
- [ ] Security-event and audit-log evidence with no password/reset-token leakage.

## 4. Visual gate

- [ ] Desktop login matches the approved frozen composition.
- [ ] Short desktop scrolls instead of compressing/repositioning the branded panel.
- [ ] Tablet/mobile layouts stack cleanly and remain usable.
- [ ] Authenticated pages do not introduce document-level horizontal overflow at phone/tablet widths.
- [ ] Dashboard, Profile, Security, Attendance, Access Requests and Administrator screens have no clipping, overlapping controls or broken icons.
- [ ] Capture final screenshots only from the runtime-verified candidate.

## 5. Documentation gate

- [ ] README features match implemented behaviour.
- [ ] Architecture, security, roles, testing and deployment docs are current.
- [ ] Known limitations are stated without underselling implemented functionality.
- [ ] Recruiter demo path in `docs/portfolio-handoff.md` is reproducible.
- [ ] Portfolio case study and recruiter demo script match the final implemented behaviour.

## 6. Release

Only after the gates above pass:

- [ ] Tag/copy the exact verified source as the final release candidate.
- [ ] Update final screenshots.
- [ ] Update canonical GitHub.
- [ ] Add the project to the portfolio with truthful verification/deployment wording.

Do not use an older build/test result to label a newer source package as verified.
