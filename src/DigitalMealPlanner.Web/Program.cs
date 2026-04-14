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
var dbPath = Path.Combine(builder.Environment.ContentRootPath, "App_Data");
Directory.CreateDirectory(dbPath);
builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

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

// --- Anthropic --- reads from CLAUDE_API env var, falls back to appsettings
var anthropicKey = Environment.GetEnvironmentVariable("CLAUDE_API")
    ?? builder.Configuration["Anthropic:ApiKey"]
    ?? string.Empty;
builder.Services.AddSingleton(_ => new AnthropicClient(anthropicKey));

// --- App Services ---
builder.Services.AddScoped<ICookbookService, CookbookService>();
builder.Services.AddScoped<IRecipeService, RecipeService>();
builder.Services.AddScoped<IMealPlanService, MealPlanService>();
builder.Services.AddScoped<ICalorieService, CalorieService>();
builder.Services.AddScoped<IImageStorageService, ImageStorageService>();
builder.Services.AddScoped<IGptVisionService, GptVisionService>();

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
