# Role and Permission Matrix

| Capability | Employee | Manager | Administrator |
| --- | :---: | :---: | :---: |
| Sign in / sign out | ✓ | ✓ | ✓ |
| View own dashboard | ✓ | ✓ | ✓ |
| View/edit permitted own profile fields | ✓ | ✓ | ✓ |
| Change own password | ✓ | ✓ | ✓ |
| View own security/account activity | ✓ | ✓ | ✓ |
| Clock self in/out | ✓ | ✓ | ✓ |
| View own attendance | ✓ | ✓ | ✓ |
| Submit access request | ✓ | ✓ | ✓ |
| View limited team area | — | ✓ | ✓ |
| View all users | — | — | ✓ |
| Create employee accounts | — | — | ✓ |
| Activate/suspend/disable accounts | — | — | ✓ |
| Assign/change roles | — | — | ✓ |
| Review access requests | — | — | ✓ |
| View system security events | — | — | ✓ |
| View system audit logs | — | — | ✓ |

## Notes

The role model is intentionally fixed and understandable. The project does not build a configurable enterprise permission-builder because that complexity is unnecessary for the portfolio objective.

Account status is evaluated separately from role. A user may hold a role but still be unable to sign in when their account is suspended or disabled.
