using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SecureEmployeePortal.Tests;

/// <summary>
/// Deliberate "break it on purpose" guards. These tests protect security and workflow
/// boundaries that are easy to regress while polishing the portal.
/// </summary>
[TestClass]
public sealed class NegativeRegressionGuardTests
{
    [TestMethod]
    public void ProtectedRoutes_KeepExpectedAuthorizationBoundaries()
    {
        string[] employeePages =
        [
            "AccessRequests.razor",
            "AccountActivity.razor",
            "Attendance.razor",
            "Dashboard.razor",
            "Profile.razor",
            "Security.razor",
            "Support.razor"
        ];

        foreach (var page in employeePages)
        {
            StringAssert.Contains(
                ReadRepoFile("Components", "Pages", page),
                "@attribute [Authorize]",
                $"{page} must remain authenticated-only.");
        }

        var manager = ReadRepoFile("Components", "Pages", "Manager.razor");
        StringAssert.Contains(manager, "@attribute [Authorize(Roles = \"Manager,Administrator\")]", "Manager view must not become employee-accessible.");

        string[] administratorPages =
        [
            "Admin.razor",
            "AdminAccessRequests.razor",
            "AdminAuditLogs.razor",
            "AdminCreateUser.razor",
            "AdminSecurityEvents.razor",
            "AdminUserDetail.razor",
            "AdminUsers.razor",
            "RolesPermissions.razor"
        ];

        foreach (var page in administratorPages)
        {
            StringAssert.Contains(
                ReadRepoFile("Components", "Pages", page),
                "@attribute [Authorize(Roles = \"Administrator\")]",
                $"{page} must remain Administrator-only.");
        }
    }

    [TestMethod]
    public void PublicAuthenticationFlows_KeepNeutralFailureResponses()
    {
        var login = ReadRepoFile("Components", "Pages", "Home.razor");
        const string neutralLoginMessage = "The email address or password is incorrect, or the account is not available.";
        Assert.IsTrue(Regex.Matches(login, Regex.Escape(neutralLoginMessage)).Count >= 2,
            "Unknown users, disabled accounts and bad passwords should retain the same public login message.");
        StringAssert.Contains(login, "lockoutOnFailure: true");

        var forgot = ReadRepoFile("Components", "Pages", "ForgotPassword.razor");
        StringAssert.Contains(forgot, "Submitted = true");
        StringAssert.Contains(forgot, "if (user is null)");
        StringAssert.Contains(forgot, "return;");
        StringAssert.Contains(forgot, "Keep the public response neutral");
    }

    [TestMethod]
    public void AttendanceAndAccessRequests_KeepTransactionalStateGuards()
    {
        var attendance = ReadRepoFile("Services", "AttendanceService.cs");
        Assert.IsTrue(Regex.Matches(attendance, "BeginTransactionAsync\\(IsolationLevel\\.Serializable\\)").Count >= 2,
            "Clock-in and clock-out must keep serializable state transitions.");
        StringAssert.Contains(attendance, "AttendanceRules.CanClockIn(hasOpenRecord)");
        StringAssert.Contains(attendance, "AttendanceRules.CanClockOut(openRecord is not null)");
        StringAssert.Contains(attendance, "authorizationService.IsActiveUserAsync(userId)");

        var requests = ReadRepoFile("Services", "AccessRequestService.cs");
        Assert.IsTrue(Regex.Matches(requests, "BeginTransactionAsync\\(IsolationLevel\\.Serializable\\)").Count >= 2,
            "Request submission and review must keep serializable state transitions.");
        StringAssert.Contains(requests, "request.Status == AccessRequestRules.Pending");
        StringAssert.Contains(requests, "AccessRequestRules.CanReview(request.Status)");
        StringAssert.Contains(requests, "AccessRequestRules.MayReviewerReview(reviewerUserId, request.RequesterUserId)");
    }

    [TestMethod]
    public void AdminMutations_KeepSelfProtectionAndFreshAuthorization()
    {
        var detail = ReadRepoFile("Components", "Pages", "AdminUserDetail.razor");
        StringAssert.Contains(detail, "AccountAccessRules.MayAdministratorChangeTarget(ActorUser?.Id, TargetUser.Id)");
        Assert.IsTrue(Regex.Matches(detail, "AuthorizationService\\.IsActiveAdministratorAsync\\(ActorUser\\.Id\\)").Count >= 2,
            "Both status and role writes must re-check current Administrator access.");
        Assert.IsTrue(Regex.Matches(detail, "UpdateSecurityStampAsync\\(TargetUser\\)").Count >= 2,
            "Status and role changes must invalidate the target account's existing authentication state.");
        StringAssert.Contains(detail, "AccountAccessRules.IsKnownRole(requestedRole)");

        var create = ReadRepoFile("Components", "Pages", "AdminCreateUser.razor");
        StringAssert.Contains(create, "AuthorizationService.IsActiveAdministratorAsync");
        StringAssert.Contains(create, "AccountAccessRules.IsKnownRole");
    }

    [TestMethod]
    public void SourceControlledConfiguration_KeepsCredentialPlaceholdersEmpty()
    {
        using var document = JsonDocument.Parse(ReadRepoFile("appsettings.json"));
        var root = document.RootElement;

        Assert.AreEqual(string.Empty, root.GetProperty("SeedAdmin").GetProperty("Email").GetString());
        Assert.AreEqual(string.Empty, root.GetProperty("SeedAdmin").GetProperty("Password").GetString());
        Assert.AreEqual(string.Empty, root.GetProperty("Smtp").GetProperty("Host").GetString());
        Assert.AreEqual(string.Empty, root.GetProperty("Smtp").GetProperty("Username").GetString());
        Assert.AreEqual(string.Empty, root.GetProperty("Smtp").GetProperty("Password").GetString());
    }

    [TestMethod]
    public void ActivityLoggingCalls_DoNotPassPasswordOrResetTokenValues()
    {
        var root = FindRepositoryRoot();
        var sourceFiles = Directory
            .EnumerateFiles(Path.Combine(root, "Components"), "*.razor", SearchOption.AllDirectories)
            .Concat(Directory.EnumerateFiles(Path.Combine(root, "Services"), "*.cs", SearchOption.AllDirectories))
            .Where(path => !path.EndsWith("PortalActivityService.cs", StringComparison.OrdinalIgnoreCase));

        string[] forbiddenArgumentFragments =
        [
            "Input.Password",
            "Input.Code",
            "CurrentPassword",
            "NewPassword",
            "TemporaryPassword",
            "ConfirmTemporaryPassword"
        ];

        foreach (var path in sourceFiles)
        {
            var source = File.ReadAllText(path);
            var calls = Regex.Matches(
                source,
                @"Record(?:SecurityEvent|Audit)Async\s*\((?<args>.*?)\)\s*;",
                RegexOptions.Singleline);

            foreach (Match call in calls)
            {
                var arguments = call.Groups["args"].Value;
                foreach (var forbidden in forbiddenArgumentFragments)
                {
                    Assert.IsFalse(
                        arguments.Contains(forbidden, StringComparison.Ordinal),
                        $"{Path.GetRelativePath(root, path)} passes sensitive value '{forbidden}' into activity logging.");
                }
            }
        }
    }

    [TestMethod]
    public void AdministratorRoleViews_DoNotTreatUnknownIdentityRolesAsPortalRoles()
    {
        var detail = ReadRepoFile("Components", "Pages", "AdminUserDetail.razor");
        StringAssert.Contains(detail, "assignedRoles.Where(AccountAccessRules.IsKnownRole)");
        StringAssert.Contains(detail, "No portal role");
        StringAssert.Contains(detail, "SelectedRole = portalRoles.Length == 1 ? portalRoles[0] : \"Employee\"");

        var users = ReadRepoFile("Components", "Pages", "AdminUsers.razor");
        StringAssert.Contains(users, "roles.Where(AccountAccessRules.IsKnownRole)");
        StringAssert.Contains(users, "No portal role");
    }

    [TestMethod]
    public void ProfileUpdateFailure_RestoresRenderedIdentityState()
    {
        var profile = ReadRepoFile("Components", "Pages", "Profile.razor");

        string[] preservedValues =
        [
            "previousFirstName",
            "previousLastName",
            "previousPhoneNumber",
            "previousPhoneNumberConfirmed"
        ];

        foreach (var value in preservedValues)
        {
            StringAssert.Contains(profile, value, $"Profile save should preserve {value} before the Identity update.");
        }

        StringAssert.Contains(profile, "CurrentUser.FirstName = previousFirstName");
        StringAssert.Contains(profile, "CurrentUser.LastName = previousLastName");
        StringAssert.Contains(profile, "CurrentUser.PhoneNumber = previousPhoneNumber");
        StringAssert.Contains(profile, "CurrentUser.PhoneNumberConfirmed = previousPhoneNumberConfirmed");
    }

    [TestMethod]
    public void AccountActivity_DoesNotExposeAdministratorActionsOnOtherEmployees()
    {
        var activity = ReadRepoFile("Components", "Pages", "AccountActivity.razor");
        StringAssert.Contains(activity, ".Where(item => item.TargetUserId == user.Id)");
        Assert.IsFalse(
            activity.Contains("item.ActorUserId == user.Id || item.TargetUserId == user.Id", StringComparison.Ordinal),
            "Personal account activity must not turn into an administrator work-history feed.");
    }

    [TestMethod]
    public void DevelopmentPasswordResetTokens_StayOutOfSourceAndApplicationLogs()
    {
        var delivery = ReadRepoFile("Services", "PasswordResetDeliveryService.cs");
        var gitIgnore = ReadRepoFile(".gitignore");

        StringAssert.Contains(delivery, "Path.Combine(environment.ContentRootPath, \"App_Data\", \"development-mail\")");
        StringAssert.Contains(gitIgnore, "App_Data/");

        var logCalls = Regex.Matches(
            delivery,
            @"Log(?:Information|Warning|Error|Critical|Debug|Trace)\s*\((?<args>.*?)\)\s*;",
            RegexOptions.Singleline);

        foreach (Match call in logCalls)
        {
            Assert.IsFalse(
                call.Groups["args"].Value.Contains("resetLink", StringComparison.Ordinal),
                "Password-reset links must never be written to application logs.");
        }
    }

    [TestMethod]
    public void LiteralInternalLinks_ResolveToImplementedPortalRoutes()
    {
        var root = FindRepositoryRoot();
        var componentFiles = Directory.EnumerateFiles(
            Path.Combine(root, "Components"),
            "*.razor",
            SearchOption.AllDirectories).ToArray();

        var routeTemplates = componentFiles
            .SelectMany(path => Regex.Matches(File.ReadAllText(path), @"@page\s+""(?<route>[^""]+)""")
                .Select(match => match.Groups["route"].Value))
            .ToArray();

        static bool MatchesRoute(string route, string template)
        {
            var routeSegments = route.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
            var templateSegments = template.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (routeSegments.Length != templateSegments.Length) return false;

            for (var index = 0; index < routeSegments.Length; index++)
            {
                var templateSegment = templateSegments[index];
                if (templateSegment.StartsWith('{') && templateSegment.EndsWith('}')) continue;
                if (!string.Equals(routeSegments[index], templateSegment, StringComparison.OrdinalIgnoreCase)) return false;
            }

            return true;
        }

        foreach (var path in componentFiles)
        {
            var source = File.ReadAllText(path);
            var hrefs = Regex.Matches(source, @"href=""(?<href>/[^""]*)""");

            foreach (Match href in hrefs)
            {
                var value = href.Groups["href"].Value;
                var route = value.Split('?', '#')[0];
                Assert.IsTrue(
                    routeTemplates.Any(template => MatchesRoute(route, template)),
                    $"Broken internal link '{value}' in {Path.GetRelativePath(root, path)}.");
            }
        }
    }

    [TestMethod]
    public void EfSnapshot_KeepsPortalWorkflowEntities()
    {
        var snapshot = ReadRepoFile("Migrations", "ApplicationDbContextModelSnapshot.cs");
        string[] requiredEntities =
        [
            "AttendanceRecord",
            "AccessRequest",
            "SecurityEvent",
            "AuditLog"
        ];

        foreach (var entity in requiredEntities)
        {
            StringAssert.Contains(snapshot, entity, $"EF model snapshot no longer contains {entity}.");
        }
    }

    private static string ReadRepoFile(params string[] pathParts)
    {
        var root = FindRepositoryRoot();
        var path = pathParts.Aggregate(root, (current, part) => Path.Combine(current, part));
        return File.ReadAllText(path);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "SecureEmployeePortal.csproj")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate SecureEmployeePortal.csproj from the test output directory.");
    }
}
