using DigitalMealPlanner.Tests.Helpers;
using DigitalMealPlanner.Web.Infrastructure.Data.Entities;
using DigitalMealPlanner.Web.Modules.Calories;
using Xunit;

namespace DigitalMealPlanner.Tests.Calories;

public class CalorieServiceTests
{
    private const string UserId = "user-123";
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.Today);

    private static (CalorieService svc, Web.Infrastructure.Data.AppDbContext db) CreateService()
    {
        var db = TestDbContextFactory.Create();
        return (new CalorieService(db), db);
    }

    [Fact]
    public async Task GetDaySummary_NoMeals_ReturnsZeroCalories()
    {
        var (svc, _) = CreateService();
        var result = await svc.GetDaySummaryAsync(UserId, Today, 2000);
        Assert.Equal(0, result.TotalCalories);
        Assert.Equal(2000, result.TargetCalories);
        Assert.Equal(2000, result.Deficit);
    }

    [Fact]
    public async Task GetDaySummary_TwoMeals_SumsCaloriesCorrectly()
    {
        var (svc, db) = CreateService();
        var (cookbook, recipe1, recipe2, theme) = await SeedData(db, 500, 300);

        db.MealPlanEntries.AddRange(
            new MealPlanEntry { UserId = UserId, Date = Today, RecipeId = recipe1.Id, MealThemeId = theme.Id, ServingsPlanned = 1 },
            new MealPlanEntry { UserId = UserId, Date = Today, RecipeId = recipe2.Id, MealThemeId = theme.Id, ServingsPlanned = 1 }
        );
        await db.SaveChangesAsync();

        var result = await svc.GetDaySummaryAsync(UserId, Today, 2000);
        Assert.Equal(800, result.TotalCalories);
        Assert.Equal(1200, result.Deficit);
    }

    [Fact]
    public async Task GetDaySummary_ServingsMultiplier_AppliedCorrectly()
    {
        var (svc, db) = CreateService();
        var (_, recipe1, _, theme) = await SeedData(db, 400, 0);

        db.MealPlanEntries.Add(
            new MealPlanEntry { UserId = UserId, Date = Today, RecipeId = recipe1.Id, MealThemeId = theme.Id, ServingsPlanned = 3 }
        );
        await db.SaveChangesAsync();

        var result = await svc.GetDaySummaryAsync(UserId, Today, 2000);
        Assert.Equal(1200, result.TotalCalories);
    }

    [Fact]
    public async Task GetWeekSummary_PartialWeek_AveragesCorrectly()
    {
        var (svc, db) = CreateService();
        var monday = Today.AddDays(-(int)Today.DayOfWeek + 1);
        var (_, recipe1, _, theme) = await SeedData(db, 700, 0);

        // Only assign Monday and Wednesday
        db.MealPlanEntries.AddRange(
            new MealPlanEntry { UserId = UserId, Date = monday, RecipeId = recipe1.Id, MealThemeId = theme.Id, ServingsPlanned = 1 },
            new MealPlanEntry { UserId = UserId, Date = monday.AddDays(2), RecipeId = recipe1.Id, MealThemeId = theme.Id, ServingsPlanned = 1 }
        );
        await db.SaveChangesAsync();

        var summary = await svc.GetWeekSummaryAsync(UserId, monday, 2000);
        Assert.Equal(1400, summary.TotalWeekCalories);
        Assert.Equal(200, summary.AverageDailyCalories); // 1400 / 7
    }

    [Fact]
    public async Task GetDaySummary_PercentageConsumed_CalculatedCorrectly()
    {
        var (svc, db) = CreateService();
        var (_, recipe1, _, theme) = await SeedData(db, 1000, 0);

        db.MealPlanEntries.Add(
            new MealPlanEntry { UserId = UserId, Date = Today, RecipeId = recipe1.Id, MealThemeId = theme.Id, ServingsPlanned = 1 }
        );
        await db.SaveChangesAsync();

        var result = await svc.GetDaySummaryAsync(UserId, Today, 2000);
        Assert.Equal(50, result.PercentageConsumed);
    }

    private static async Task<(Cookbook, Recipe, Recipe, MealTheme)> SeedData(
        Web.Infrastructure.Data.AppDbContext db, int cal1, int cal2)
    {
        var user = new ApplicationUser { Id = UserId, UserName = "test", Email = "t@t.com" };
        var cookbook = new Cookbook { UserId = UserId, Name = "Test", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        db.Users.Add(user);
        db.Cookbooks.Add(cookbook);
        await db.SaveChangesAsync();

        var recipe1 = new Recipe { CookbookId = cookbook.Id, Title = "R1", CaloriesPerServing = cal1, Servings = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var recipe2 = new Recipe { CookbookId = cookbook.Id, Title = "R2", CaloriesPerServing = cal2, Servings = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var theme   = new MealTheme { UserId = UserId, Name = "Dinner", Icon = "🍽️", ColourHex = "#e63946" };

        db.Recipes.AddRange(recipe1, recipe2);
        db.MealThemes.Add(theme);
        await db.SaveChangesAsync();

        return (cookbook, recipe1, recipe2, theme);
    }
}
