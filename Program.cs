// Configures the Secure Employee Portal application, security, data, and request pipeline.
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SecureEmployeePortal.Components;
using SecureEmployeePortal.Data;
using SecureEmployeePortal.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

// LocalDB is intentionally a Windows-development dependency. A hosted build must
// receive a real SQL Server/Azure SQL connection string through secure configuration.
if (!builder.Environment.IsDevelopment()
    && connectionString.Contains("(localdb)", StringComparison.OrdinalIgnoreCase))
{
    throw new InvalidOperationException(
        "A hosted environment must supply ConnectionStrings__DefaultConnection; LocalDB is development-only.");
}

builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedAccount = false;

        options.Password.RequiredLength = 8;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireDigit = true;
        options.Password.RequireNonAlphanumeric = true;

        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/";
    options.AccessDeniedPath = "/access-denied";
    options.Cookie.Name = "SecureEmployeePortal.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
});

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider, PortalRevalidatingAuthenticationStateProvider>();
builder.Services.AddScoped<PortalActivityService>();
builder.Services.AddScoped<PortalAuthorizationService>();
builder.Services.AddScoped<PasswordResetDeliveryService>();
builder.Services.AddScoped<AttendanceService>();
builder.Services.AddScoped<AccessRequestService>();
builder.Services.AddSingleton<SecureAssistantService>();
builder.Services.AddHealthChecks();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

// Add a small set of defensive response headers that do not interfere with Blazor.
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "no-referrer";
    await next();
});

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapHealthChecks("/health").AllowAnonymous();

// Logout is deliberately a POST action and validates the antiforgery token.
app.MapPost("/account/logout", async (
        HttpContext context,
        IAntiforgery antiforgery,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        PortalActivityService activityService,
        ILoggerFactory loggerFactory) =>
    {
        await antiforgery.ValidateRequestAsync(context);
        var user = await userManager.GetUserAsync(context.User);

        if (user is not null)
        {
            try
            {
                await activityService.RecordSecurityEventAsync(
                    user.Id,
                    "Logout",
                    true,
                    "Employee signed out.");
            }
            catch (Exception exception)
            {
                // Logging must never prevent an authenticated user from ending their session.
                loggerFactory.CreateLogger("Logout").LogWarning(exception, "Logout security-event recording failed.");
            }
        }

        await signInManager.SignOutAsync();
        return Results.LocalRedirect("/");
    })
    .RequireAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Development applies migrations automatically for a low-friction local demo.
// Hosted environments may opt in with Database__ApplyMigrationsOnStartup=true;
// otherwise migrations should be applied explicitly during deployment.
var applyMigrationsOnStartup = app.Environment.IsDevelopment()
    || app.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup");

if (applyMigrationsOnStartup)
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();
}

await PortalSeedService.SeedAsync(app.Services, app.Configuration);

app.Run();

// Exposes Program to the integration-test host without changing runtime behaviour.
public partial class Program { }
