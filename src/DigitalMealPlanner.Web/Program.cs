using Anthropic.SDK;
using DigitalMealPlanner.Web.Infrastructure.AI;
using DigitalMealPlanner.Web.Infrastructure.Data;
using DigitalMealPlanner.Web.Infrastructure.Data.Entities;
using DigitalMealPlanner.Web.Infrastructure.Storage;
using DigitalMealPlanner.Web.Modules.Calories;
using DigitalMealPlanner.Web.Modules.Cookbooks;
using DigitalMealPlanner.Web.Modules.MealPlan;
using DigitalMealPlanner.Web.Modules.Recipes;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;

QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

// --- Database ---
// On Azure App Service D:\home persists across deployments; D:\home\site\wwwroot does not.
// Use WEBSITE_CONTENTSHARE env var presence to detect Azure, then store db outside wwwroot.
var isAzure = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME"));
var dbDir = isAzure
    ? @"D:\home\data\App_Data"
    : Path.Combine(builder.Environment.ContentRootPath, "App_Data");
Directory.CreateDirectory(dbDir);
var dbConnectionString = $"Data Source={Path.Combine(dbDir, "mealplanner.db")}";
builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlite(dbConnectionString));

// --- Identity ---
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/login";
    options.LogoutPath = "/logout";
    options.AccessDeniedPath = "/login";
    options.ExpireTimeSpan = TimeSpan.FromDays(14);
    options.SlidingExpiration = true;
});

// --- Anthropic --- tries process → user → machine env var, then appsettings
var anthropicKey =
    Environment.GetEnvironmentVariable("CLAUDE_API", EnvironmentVariableTarget.Process)
    ?? Environment.GetEnvironmentVariable("CLAUDE_API", EnvironmentVariableTarget.User)
    ?? Environment.GetEnvironmentVariable("CLAUDE_API", EnvironmentVariableTarget.Machine)
    ?? builder.Configuration["Anthropic:ApiKey"]
    ?? string.Empty;
builder.Services.AddSingleton(_ => new AnthropicClient(anthropicKey));

// --- OpenAI key --- tries standard env var, then appsettings
var openAiKey =
    Environment.GetEnvironmentVariable("OPENAI_API", EnvironmentVariableTarget.Process)
    ?? Environment.GetEnvironmentVariable("OPENAI_API", EnvironmentVariableTarget.User)
    ?? Environment.GetEnvironmentVariable("OPENAI_API", EnvironmentVariableTarget.Machine)
    ?? builder.Configuration["OpenAI:ApiKey"]
    ?? string.Empty;

// --- App Services ---
builder.Services.AddScoped<ICookbookService, CookbookService>();
builder.Services.AddScoped<IRecipeService, RecipeService>();
builder.Services.AddScoped<IMealPlanService, MealPlanService>();
builder.Services.AddScoped<ICalorieService, CalorieService>();
builder.Services.AddScoped<IImageStorageService, ImageStorageService>();

// Vision: always register both; FallbackVisionService tries Claude first,
// then automatically retries with OpenAI if Claude throws.
builder.Services.AddScoped<GptVisionService>();
builder.Services.AddScoped<OpenAiVisionService>();
builder.Services.AddScoped<IGptVisionService, FallbackVisionService>();

// --- MVC with feature folder view locations ---
builder.Services.AddControllersWithViews()
    .AddRazorOptions(options =>
    {
        options.ViewLocationFormats.Add("/Modules/{1}/Views/{0}.cshtml");
        options.ViewLocationFormats.Add("/Modules/{1}/Views/Partials/{0}.cshtml");
    });

var app = builder.Build();

// --- Seed ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    await DatabaseSeeder.SeedAsync(db, userManager);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();

public partial class Program { }
