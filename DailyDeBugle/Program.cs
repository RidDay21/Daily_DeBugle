using DailyDeBugle.Components;
using DailyDeBugle.Data;
using DailyDeBugle.Services;
using Microsoft.EntityFrameworkCore;

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

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();