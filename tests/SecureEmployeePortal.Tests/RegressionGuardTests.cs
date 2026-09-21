using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SecureEmployeePortal.Tests;

[TestClass]
public class RegressionGuardTests
{
    [TestMethod]
    public void ShortDesktopHeightRules_DoNotMutateFrozenBrandPanel()
    {
        var css = ReadRepoFile("wwwroot", "portal-polish.css");
        var shortHeightBlocks = ExtractMediaBlocks(css)
            .Where(block => block.Header.Contains("min-width: 951px", StringComparison.OrdinalIgnoreCase)
                         && block.Header.Contains("max-height", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        Assert.IsTrue(shortHeightBlocks.Length > 0, "Expected at least one desktop short-height media query.");

        string[] frozenSelectors =
        [
            ".login-visual-panel",
            ".brand-row",
            ".brand-icon",
            ".brand-name",
            ".brand-tagline",
            ".visual-content",
            ".visual-intro-row",
            ".dot-grid",
            ".illustration-wrapper",
            ".feature-row",
            ".feature-item",
            ".feature-icon",
            ".feature-copy"
        ];

        foreach (var block in shortHeightBlocks)
        {
            foreach (var selector in frozenSelectors)
            {
                Assert.IsFalse(
                    block.Body.Contains(selector, StringComparison.Ordinal),
                    $"Short-height desktop CSS must not mutate frozen authentication-panel selector '{selector}'.");
            }
        }
    }

    [TestMethod]
    public void AuthPageScopedCss_PreservesDesktopMinimumHeightWithoutCompression()
    {
        // These are CSS-isolated page styles, so the desktop-height guard must live in
        // the scoped files themselves rather than relying only on a later global rule.
        string[] cssFiles =
        [
            Path.Combine("Components", "Pages", "Home.razor.css"),
            Path.Combine("Components", "Pages", "ForgotPassword.razor.css"),
            Path.Combine("Components", "Pages", "ResetPassword.razor.css")
        ];

        foreach (var relativePath in cssFiles)
        {
            var css = ReadRepoFile(relativePath.Split(Path.DirectorySeparatorChar));
            AssertRuleContains(
                css,
                ".login-shell",
                "height: clamp(760px, calc(100vh - 72px), 880px)",
                "min-height: 760px");
        }

        // Registration is intentionally allowed to grow vertically because its form is
        // longer, while retaining the same 760-880px protected desktop minimum.
        var registerCss = ReadRepoFile("Components", "Pages", "Register.razor.css");
        AssertRuleContains(
            registerCss,
            ".login-shell",
            "height: auto",
            "min-height: clamp(760px, calc(100vh - 72px), 880px)");
    }

    [TestMethod]
    public void AuthPageScopedCss_ReleasesFixedDesktopHeightOnTabletAndPhone()
    {
        // CSS isolation gives these rules more specificity than the global polish file.
        // Keep the responsive reset in every auth page so mobile layouts cannot inherit
        // the desktop 760px shell accidentally.
        string[] cssFiles =
        [
            Path.Combine("Components", "Pages", "Home.razor.css"),
            Path.Combine("Components", "Pages", "Register.razor.css"),
            Path.Combine("Components", "Pages", "ForgotPassword.razor.css"),
            Path.Combine("Components", "Pages", "ResetPassword.razor.css")
        ];

        foreach (var relativePath in cssFiles)
        {
            var css = ReadRepoFile(relativePath.Split(Path.DirectorySeparatorChar));
            var tabletBlocks = ExtractMediaBlocks(css)
                .Where(block => block.Header.Contains("max-width: 950px", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            Assert.AreEqual(1, tabletBlocks.Length, $"Expected one tablet auth media block in {relativePath}.");
            StringAssert.Contains(tabletBlocks[0].Body, "height: auto");
            StringAssert.Contains(tabletBlocks[0].Body, "min-height: 100vh");
            StringAssert.Contains(tabletBlocks[0].Body, "overflow: visible");
        }
    }

    [TestMethod]
    public void FeatureRow_IsNeverAbsolutelyPositioned()
    {
        string[] cssFiles =
        [
            Path.Combine("Components", "Pages", "Home.razor.css"),
            Path.Combine("Components", "Pages", "Register.razor.css"),
            Path.Combine("Components", "Pages", "ForgotPassword.razor.css"),
            Path.Combine("Components", "Pages", "ResetPassword.razor.css"),
            Path.Combine("wwwroot", "portal-polish.css")
        ];

        foreach (var relativePath in cssFiles)
        {
            var css = ReadRepoFile(relativePath.Split(Path.DirectorySeparatorChar));
            var featureRules = Regex.Matches(css, @"\.feature-row\s*\{(?<body>[^}]*)\}", RegexOptions.Singleline);

            foreach (Match rule in featureRules)
            {
                Assert.IsFalse(
                    Regex.IsMatch(rule.Groups["body"].Value, @"position\s*:\s*absolute", RegexOptions.IgnoreCase),
                    $"{relativePath} must not absolutely position .feature-row; that previously broke the approved auth layout.");
            }
        }
    }


    [TestMethod]
    public void FrozenLoginVisualGeometry_RemainsApproved()
    {
        var css = ReadRepoFile("Components", "Pages", "Home.razor.css");

        AssertRuleContains(css, ".login-visual-panel", "padding: 38px 48px", "justify-content: flex-start");
        AssertRuleContains(css, ".visual-content", "margin: 54px auto 0");
        AssertRuleContains(css, ".visual-intro-row", "grid-template-columns: 118px 1fr", "transform: translateY(-22px)", "margin-bottom: 18px");
        AssertRuleContains(css, ".illustration-wrapper", "height: 245px", "margin-top: 24px");
        AssertRuleContains(css, ".illustration-wrapper img", "width: 138%", "top: 148px", "left: 50%");
        AssertRuleContains(css, ".feature-row", "position: relative", "margin-top: auto", "grid-template-columns: repeat(3, minmax(0, 1fr))");
    }

    [TestMethod]
    public void StaticSsrLoginAndRegistration_KeepNamedFormBinding()
    {
        var login = ReadRepoFile("Components", "Pages", "Home.razor");
        StringAssert.Contains(login, "FormName=\"login\"");
        StringAssert.Contains(login, "SupplyParameterFromForm(FormName = \"login\")");

        var register = ReadRepoFile("Components", "Pages", "Register.razor");
        StringAssert.Contains(register, "FormName=\"register\"");
        StringAssert.Contains(register, "SupplyParameterFromForm(FormName = \"register\")");
    }


    [TestMethod]
    public void CoreAuthenticationPipelineGuards_RemainInPlace()
    {
        var login = ReadRepoFile("Components", "Pages", "Home.razor");
        StringAssert.Contains(login, "AccountAccessRules.MaySignIn");
        StringAssert.Contains(login, "PasswordSignInAsync");
        StringAssert.Contains(login, "lockoutOnFailure: true");
        StringAssert.Contains(login, "PortalSecurityRules.IsSafeLocalReturnUrl");

        var register = ReadRepoFile("Components", "Pages", "Register.razor");
        StringAssert.Contains(register, "UserManager.CreateAsync");
        StringAssert.Contains(register, "UserManager.AddToRoleAsync(user, \"Employee\")");

        var program = ReadRepoFile("Program.cs");
        StringAssert.Contains(program, "app.MapPost(\"/account/logout\"");
        StringAssert.Contains(program, "antiforgery.ValidateRequestAsync(context)");
        StringAssert.Contains(program, ".RequireAuthorization()");

        var layout = ReadRepoFile("Components", "Layout", "EmployeeLayout.razor");
        StringAssert.Contains(layout, "<form action=\"/account/logout\" method=\"post\"");
        StringAssert.Contains(layout, "<AntiforgeryToken />");

        var provider = ReadRepoFile("Services", "PortalRevalidatingAuthenticationStateProvider.cs");
        StringAssert.Contains(provider, "AccountAccessRules.MaySignIn(user.AccountStatus)");
        StringAssert.Contains(provider, "GetSecurityStampAsync");
    }

    [TestMethod]
    public void CookieWritingIdentityFlows_RemainStaticSsr()
    {
        // Sign-in and RefreshSignInAsync issue authentication cookies. Keeping these
        // pages in static SSR ensures the HTTP response can still set cookie headers.
        var login = ReadRepoFile("Components", "Pages", "Home.razor");
        StringAssert.Contains(login, "SignInManager.PasswordSignInAsync");
        Assert.IsFalse(
            login.Contains("@rendermode InteractiveServer", StringComparison.Ordinal),
            "The login page must not move cookie-writing sign-in into an interactive server circuit.");

        var security = ReadRepoFile("Components", "Pages", "Security.razor");
        StringAssert.Contains(security, "SignInManager.RefreshSignInAsync");
        Assert.IsFalse(
            security.Contains("@rendermode InteractiveServer", StringComparison.Ordinal),
            "The password-change page must stay static SSR so RefreshSignInAsync can refresh the authentication cookie.");
    }

    [TestMethod]
    public void ProtectedDashboardRouting_GuardsRemainInPlace()
    {
        var dashboard = ReadRepoFile("Components", "Pages", "Dashboard.razor");
        StringAssert.Contains(dashboard, "@attribute [Authorize]");

        var routes = ReadRepoFile("Components", "Routes.razor");
        StringAssert.Contains(routes, "<AuthorizeRouteView");
        StringAssert.Contains(routes, "<RedirectToLogin />");
    }

    [TestMethod]
    public void SensitiveWrites_KeepFreshDatabaseAuthorizationChecks()
    {
        var authorization = ReadRepoFile("Services", "PortalAuthorizationService.cs");
        StringAssert.Contains(authorization, "user.AccountStatus == AccountStatus.Active");
        StringAssert.Contains(authorization, "role.NormalizedName == \"ADMINISTRATOR\"");

        var attendance = ReadRepoFile("Services", "AttendanceService.cs");
        Assert.IsTrue(
            Regex.Matches(attendance, "authorizationService\\.IsActiveUserAsync\\(userId\\)").Count >= 2,
            "Clock-in and clock-out must each re-check that the account is still active before writing attendance data.");

        var requests = ReadRepoFile("Services", "AccessRequestService.cs");
        StringAssert.Contains(requests, "authorizationService.IsActiveUserAsync(userId)");
        StringAssert.Contains(requests, "authorizationService.IsActiveAdministratorAsync(reviewerUserId)");

        var adminDetail = ReadRepoFile("Components", "Pages", "AdminUserDetail.razor");
        StringAssert.Contains(adminDetail, "AuthorizationService.IsActiveAdministratorAsync(ActorUser.Id)");
        StringAssert.Contains(adminDetail, "UpdateSecurityStampAsync(TargetUser)");
    }

    [TestMethod]
    public void ClockOut_RechecksActiveAccountBeforeAttendanceMutation()
    {
        var attendance = ReadRepoFile("Services", "AttendanceService.cs");
        var methodStart = attendance.IndexOf("public async Task<AttendanceOperationResult> ClockOutAsync", StringComparison.Ordinal);
        var transactionStart = attendance.IndexOf("BeginTransactionAsync(IsolationLevel.Serializable)", methodStart, StringComparison.Ordinal);
        var activeCheck = attendance.IndexOf("authorizationService.IsActiveUserAsync(userId)", methodStart, StringComparison.Ordinal);

        Assert.IsTrue(methodStart >= 0, "ClockOutAsync must remain implemented.");
        Assert.IsTrue(activeCheck > methodStart, "ClockOutAsync must re-check the current account state.");
        Assert.IsTrue(transactionStart > activeCheck, "The active-account check must happen before clock-out opens its write transaction.");
        StringAssert.Contains(
            attendance[methodStart..transactionStart],
            "Your account must be active before you can clock out.");
    }

    [TestMethod]
    public void PasswordResetSuccess_ClearsSensitiveInputAndReplacesTokenUrl()
    {
        var reset = ReadRepoFile("Components", "Pages", "ResetPassword.razor");
        StringAssert.Contains(reset, "Input = new ResetPasswordInputModel()");
        StringAssert.Contains(reset, "NavigateTo(\"/reset-password?completed=true\", replace: true)");
    }


    [TestMethod]
    public void MobileHeaderLinks_UseBorderBoxSizingToPreventViewportOverflow()
    {
        var css = ReadRepoFile("wwwroot", "portal-polish.css");

        StringAssert.Contains(css, ".portal-page-header > a");
        StringAssert.Contains(css, "width: 100%");

        var controlRule = Regex.Match(
            css,
            @"\.portal-primary-button,\s*\.portal-secondary-button,\s*\.portal-danger-button,\s*\.portal-primary-link,\s*\.portal-secondary-link\s*\{(?<body>[^}]*)\}",
            RegexOptions.Singleline);

        Assert.IsTrue(controlRule.Success, "Expected the shared portal action-control CSS rule.");
        StringAssert.Contains(controlRule.Groups["body"].Value, "box-sizing: border-box");
    }


    private static void AssertRuleContains(string css, string selector, params string[] expectedDeclarations)
    {
        var match = Regex.Match(
            css,
            $@"{Regex.Escape(selector)}\s*\{{(?<body>[^}}]*)\}}",
            RegexOptions.Singleline);

        Assert.IsTrue(match.Success, $"Expected CSS rule '{selector}' was not found.");
        var body = Regex.Replace(match.Groups["body"].Value, @"\s+", " ");

        foreach (var declaration in expectedDeclarations)
        {
            Assert.IsTrue(
                body.Contains(declaration, StringComparison.Ordinal),
                $"Frozen CSS rule '{selector}' no longer contains approved declaration '{declaration}'.");
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

    private static IEnumerable<(string Header, string Body)> ExtractMediaBlocks(string css)
    {
        var searchIndex = 0;

        while (true)
        {
            var mediaIndex = css.IndexOf("@media", searchIndex, StringComparison.OrdinalIgnoreCase);
            if (mediaIndex < 0)
            {
                yield break;
            }

            var openBrace = css.IndexOf('{', mediaIndex);
            if (openBrace < 0)
            {
                yield break;
            }

            var depth = 1;
            var index = openBrace + 1;
            while (index < css.Length && depth > 0)
            {
                if (css[index] == '{') depth++;
                else if (css[index] == '}') depth--;
                index++;
            }

            if (depth != 0)
            {
                yield break;
            }

            var header = css[mediaIndex..openBrace];
            var body = css[(openBrace + 1)..(index - 1)];
            yield return (header, body);
            searchIndex = index;
        }
    }
}
