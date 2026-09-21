# Testing and Verification

## Automated test suite

The test project is located at:

`tests/SecureEmployeePortal.Tests/`

It covers representative business rules and validation, including:

- active versus suspended/disabled account sign-in eligibility;
- administrator self-protection and fixed-role allow-list rules;
- attendance clock-in/clock-out state rules;
- attendance duration calculation and invalid time ordering;
- access-request review state transitions and administrator self-review prevention;
- access-request form validation;
- registration, profile, password-change/reset and administrator-create-user validation;
- safe local return-URL rules used after sign-in;
- Secure Assistant topic routing and role/account-status-aware guidance;
- source-level regression guards for the frozen authentication layout, static-SSR form wiring, protected dashboard routing and fresh database-backed authorization checks on sensitive writes;
- negative-regression guards for employee/manager/admin route boundaries, neutral authentication responses, transactional attendance/access-request state changes, admin self-protection, unknown/multiple-role recovery, blank source-controlled credentials, secret-free activity logging, personal account-activity isolation, profile-save rollback consistency, development reset-token containment, literal internal-link routing, mobile full-width action-link containment, and required EF workflow entities.

Expected automated test executions in the current release candidate: **74**. The verification gate also treats compiler/analyzer warnings as errors.

Run the complete automated verification gate from PowerShell:

```powershell
# Restore, build, test and check EF Core migrations without changing
# the machine-wide PowerShell execution policy.
powershell -ExecutionPolicy Bypass -File .\scripts\verify-portfolio.ps1
```

## Manual security/workflow checklist

After the script passes, run the app with `dotnet run` and verify these end-to-end cases.

### Registration and login

- [ ] Register a new employee with valid details.
- [ ] Confirm duplicate email registration is rejected.
- [ ] Confirm invalid/mismatched password input is rejected.
- [ ] Sign in with the newly registered account.
- [ ] Confirm a wrong password receives a neutral error.
- [ ] Confirm an unknown email receives a neutral error.
- [ ] Confirm `/dashboard` cannot be used anonymously.
- [ ] Sign out and confirm protected pages require authentication again.


### Dashboard and Secure Assistant

- [ ] Confirm the dashboard greeting, attendance cards, My Tasks, Quick Links, Latest Announcements and Account Timeline render without layout breakage.
- [ ] Open Secure Assistant from the dashboard help card.
- [ ] Open Secure Assistant from the top-bar Need help? control and confirm the dashboard loads with the assistant open.
- [ ] Confirm the assistant displays a welcome message and quick-question chips.
- [ ] Ask a custom password/security question and confirm the response links to My Security.
- [ ] Ask an attendance question and confirm the response links to My Attendance.
- [ ] Ask an access-request question and confirm the response links to Access Requests.
- [ ] Confirm an unsupported question produces a bounded help response rather than inventing unrelated functionality.
- [ ] Close and reopen the assistant and confirm the current dashboard conversation remains available for the session.

### Profile and account security

- [ ] Confirm the signed-in employee sees their own identity.
- [ ] Update a permitted profile field and refresh the page.
- [ ] Confirm role/account-status fields are not employee-editable.
- [ ] Change the password and sign in with the new password.
- [ ] Request a password reset and complete the local development reset flow.
- [ ] Confirm a successful reset replaces the token-bearing URL with `/reset-password?completed=true`.

### Roles and administration

- [ ] Configure a development administrator with user secrets.
- [ ] Confirm an Employee cannot open administrator routes.
- [ ] Confirm a Manager can open the limited team view but not administrator routes.
- [ ] Confirm an Administrator can open user management.
- [ ] Create a test employee from the admin console.
- [ ] Suspend the test employee and confirm sign-in is blocked.
- [ ] Restore the test employee and confirm sign-in works again.
- [ ] Change the test employee role and confirm authorization changes.
- [ ] Confirm the current administrator cannot suspend/disable/re-role their own account.
- [ ] Demote or suspend an administrator from a separate administrator session and confirm stale-session write attempts are rejected before the periodic sign-out/revalidation completes.

### Attendance

- [ ] Clock in as an employee.
- [ ] Refresh and confirm the open attendance state persists.
- [ ] Confirm a second clock-in is rejected by the workflow.
- [ ] Clock out and confirm duration/history are stored.
- [ ] Confirm the employee sees only their own attendance page.

### Access requests and auditability

- [ ] Submit an employee access request.
- [ ] Confirm it appears as Pending for that employee.
- [ ] Review it from the Administrator console.
- [ ] Confirm a second review is rejected.
- [ ] Confirm an administrator cannot approve or reject a request submitted by that same administrator account.
- [ ] Confirm the employee sees the recorded decision.
- [ ] Confirm security events show authentication/workflow events.
- [ ] Confirm audit logs identify actor, action, target, time and result.
- [ ] Confirm no passwords/reset tokens appear in event or audit values.

## Verification status

A Windows development verification of the first polished package completed successfully: the Release build passed, 31/31 automated tests passed, the EF Core model matched the latest migration, and the application started successfully. Manual sign-in testing then exposed a static-SSR form-binding defect that automated tests did not catch; that defect was repaired before this package.

The current release-candidate source additionally protects the approved short-viewport authentication composition, static-SSR login/registration form wiring, fresh authorization checks on sensitive writes, personal account-activity isolation, failed-profile-update rollback, known-role recovery, reset-token containment, internal route/link integrity and mobile action-link containment. The current source represents **74 expected automated test executions**. The verification script and CI both fail if the discovered execution count is anything other than 74, preventing a silently reduced test suite from passing the release gate. Run the verification script above and complete the manual workflow checklist once more on this exact package before deployment. The project should only be labelled runtime-verified after that final package gate passes.
