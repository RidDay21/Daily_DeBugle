using DailyDeBugle.Components;
using DailyDeBugle.Data;
using DailyDeBugle.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// Add services to the container

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// В Program.cs, после builder.Services.AddRazorComponents()
builder.Services.AddServerSideBlazor()
    .AddHubOptions(options =>
    {
        // Увеличиваем таймауты
        options.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
        options.HandshakeTimeout = TimeSpan.FromSeconds(30);
        options.KeepAliveInterval = TimeSpan.FromSeconds(15);
        options.MaximumReceiveMessageSize = 10 * 1024 * 1024; // 10MB максимум
    });

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Authentication & Authorization
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "auth_token";
        options.LoginPath = "/login";
        options.Cookie.MaxAge = TimeSpan.FromDays(7);
    });
builder.Services.AddAuthorization(options =>
{
    // Baseline: everyone except Admin can view the app content.
    options.AddPolicy(DailyDeBugle.Security.Policies.ViewContent, policy =>
        policy.RequireAssertion(context => 
            context.User.Identity?.IsAuthenticated == true && 
            !context.User.IsInRole(DailyDeBugle.Security.Roles.Admin)));

    options.AddPolicy(DailyDeBugle.Security.Policies.WriteArticles, policy =>
        policy.RequireRole(DailyDeBugle.Security.Roles.Author, DailyDeBugle.Security.Roles.EditorInChief));

    options.AddPolicy(DailyDeBugle.Security.Policies.ReviewArticles, policy =>
        policy.RequireRole(DailyDeBugle.Security.Roles.Editor, DailyDeBugle.Security.Roles.EditorInChief));

    options.AddPolicy(DailyDeBugle.Security.Policies.LayoutIssue, policy =>
        policy.RequireRole(DailyDeBugle.Security.Roles.LayoutDesigner, DailyDeBugle.Security.Roles.EditorInChief));

    options.AddPolicy(DailyDeBugle.Security.Policies.ManageIssues, policy =>
        policy.RequireRole(DailyDeBugle.Security.Roles.EditorInChief));

    options.AddPolicy(DailyDeBugle.Security.Policies.ManagePublications, policy =>
        policy.RequireRole(DailyDeBugle.Security.Roles.EditorInChief));

    options.AddPolicy(DailyDeBugle.Security.Policies.AccessAdminPanel, policy =>
        policy.RequireRole(DailyDeBugle.Security.Roles.Admin));
});
builder.Services.AddCascadingAuthenticationState();


// Services
builder.Services.AddScoped<IPublicationService, PublicationService>();
builder.Services.AddScoped<IHeaderFooterService, HeaderFooterService>();
builder.Services.AddScoped<IGlobalTextStyleService, GlobalTextStyleService>();
builder.Services.AddScoped<IIssueService, IssueService>();
builder.Services.AddScoped<ILayoutService, LayoutService>();
builder.Services.AddScoped<IArticleService, ArticleService>();
builder.Services.AddScoped<IUserService, UserService>(); 
builder.Services.AddScoped<ITemplateService, TemplateService>();
builder.Services.AddScoped<IAdvertisementService, AdvertisementService>();

builder.Services.AddLogging();


var app = builder.Build();

// Dev-only: auto-create database/schema if missing.
// The project currently has no EF migrations checked in, so without this the app fails
// when the configured database does not exist yet.
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.EnsureCreatedAsync();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/api/auth/signin", async (string username, string? returnUrl, IUserService userService, HttpContext httpContext) =>
{
    var users = await userService.GetAllAsync();
    var dbUser = users.FirstOrDefault(u => u.Username == username);
    
    if (dbUser == null) return Results.Redirect("/login?error=InvalidUser");

    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, dbUser.Username),
        new Claim(ClaimTypes.Email, dbUser.Email),
        new Claim(ClaimTypes.Role, dbUser.Role.ToString()),
        new Claim("UserId", dbUser.UserId.ToString())
    };

    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var authProperties = new AuthenticationProperties { IsPersistent = true };

    await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

    return Results.Redirect(returnUrl ?? "/");
});

app.MapGet("/api/auth/signout", async (HttpContext httpContext) =>
{
    await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/");
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();