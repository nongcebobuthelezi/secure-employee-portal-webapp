# Architecture

## Purpose

Secure Employee Portal is a Blazor-based internal business application focused on employee identity, account access and permissions. It deliberately stops short of becoming a complete HR platform.

## Runtime shape

```text
Browser
  ↓
Blazor Web App / Razor components
  ↓
ASP.NET Core authentication + authorization
  ↓
Application services and business rules
  ↓
ASP.NET Core Identity + Entity Framework Core
  ↓
SQL Server / Azure SQL
```

The application uses server-rendered Razor components for normal account forms and Interactive Server components where immediate UI interaction is useful, such as attendance clocking, filters and protected administrator actions.

## Main areas

### Authentication

Registration, login, password reset and logout use ASP.NET Core Identity. Password handling is delegated to Identity rather than custom cryptography.

### Employee self-service

Authenticated employees can view their own dashboard and profile, update permitted personal fields, change their password, review account activity, clock in/out and submit access requests. The dashboard also contains a functioning Secure Assistant. Its responses are produced by a deterministic C# service that maps supported employee questions to portal-owned guidance and real routes; it does not depend on an external AI API.

### Secure Assistant

`SecureAssistantService` handles bounded support topics such as account security, attendance, profile changes, access requests, account activity and administrator access. The interactive dashboard component keeps the current conversation in component state, exposes quick questions, supports typed questions and renders deep links to the relevant real workflow. Unsupported questions receive a bounded capability response instead of fabricated functionality.

### Manager access

Managers inherit the employee experience and receive a deliberately narrow team view. Manager access does not grant security-administration capabilities.

### Security administration

Administrator-only pages provide user search, account-state controls, role assignment, access-request review, security-event inspection and audit-log inspection.

## Data model

Identity owns the core user and role tables. The application adds:

- `AttendanceRecord` — one employee work session with clock-in/out times.
- `AccessRequest` — a narrow request for access to a protected resource.
- `SecurityEvent` — authentication/account-security activity.
- `AuditLog` — traceable administrator and protected account changes.

All event timestamps are stored as UTC `DateTimeOffset` values and converted to local time for display.

## Authorization model

The fixed application roles are:

- `Employee`
- `Manager`
- `Administrator`

Page authorization is enforced with ASP.NET Core authorization attributes. UI visibility improves usability, but hidden links/buttons are not treated as the security boundary.

## Scope boundary

The project does not implement payroll, tax, recruitment, performance management, benefits administration, complex shift scheduling, biometric tracking or a custom enterprise permission engine. Those features would dilute the security and access-control story the project is intended to demonstrate.
