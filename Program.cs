// Configures the Secure Employee Portal application,
// database connection, Identity services, and request pipeline.
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SecureEmployeePortal.Components;
using SecureEmployeePortal.Data;

var builder = WebApplication.CreateBuilder(args);

// Read the SQL Server LocalDB address from appsettings.json.
// Stop with a clear message if the connection string is missing.
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' was not found.");

// Register ApplicationDbContext as the gateway between
// Entity Framework Core and SQL Server LocalDB.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Register ASP.NET Core Identity for employee accounts and roles.
// Identity stores its users, roles, tokens, and security information
// through the ApplicationDbContext configured above.
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        // Prevent two employee accounts from sharing one email address.
        options.User.RequireUniqueEmail = true;

        // Email confirmation remains disabled until the portal has
        // a genuine email-delivery service.
        options.SignIn.RequireConfirmedAccount = false;

        // Keep the backend password policy aligned with the
        // requirements displayed on the Registration page.
        options.Password.RequiredLength = 8;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = false;
        options.Password.RequireDigit = true;
        options.Password.RequireNonAlphanumeric = true;

        // Temporarily lock an account after repeated failed attempts.
        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan =
            TimeSpan.FromMinutes(15);
    })
    // Store Identity records through Entity Framework Core.
    .AddEntityFrameworkStores<ApplicationDbContext>()

    // Provide secure tokens for later password-reset and
    // email-confirmation functionality.
    .AddDefaultTokenProviders();

// Register permission checking for protected pages and actions.
builder.Services.AddAuthorization();

// Make the signed-in employee's authentication state available
// throughout the Blazor component hierarchy.
builder.Services.AddCascadingAuthenticationState();

// Preserve the existing Razor-components and
// Interactive Server configuration.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Preserve the existing production error handling.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

// Preserve the existing not-found-page behaviour.
app.UseStatusCodePagesWithReExecute(
    "/not-found",
    createScopeForStatusCodePages: true);

// Preserve the existing HTTPS redirection.
app.UseHttpsRedirection();

// Read the signed-in employee's authentication cookie
// before checking what that employee may access.
app.UseAuthentication();
app.UseAuthorization();

// Preserve the existing antiforgery protection.
app.UseAntiforgery();

// Preserve the existing static assets and Blazor application mapping.
app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();