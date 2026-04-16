using DigitalMealPlanner.Web.Infrastructure.Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace DigitalMealPlanner.Web.Infrastructure.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(
        AppDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        await context.Database.EnsureCreatedAsync();

        if (userManager.Users.Any()) return;

        var robyn = new ApplicationUser
        {
            UserName = "robyn",
            Email = "robyn@mealplanner.local",
            DisplayName = "Robyn",
            DailyCalorieTarget = 2000,
            EmailConfirmed = true
        };

        await userManager.CreateAsync(robyn, "C00kb00k$Pl@nn3r!Mx9#");

        var defaultThemes = new[]
        {
            new MealTheme { Name = "Breakfast", Icon = "🌅", ColourHex = "#f4a261", IsDefault = true, UserId = robyn.Id },
            new MealTheme { Name = "Lunch",     Icon = "🥗", ColourHex = "#2a9d8f", IsDefault = true, UserId = robyn.Id },
            new MealTheme { Name = "Dinner",    Icon = "🍽️", ColourHex = "#e63946", IsDefault = true, UserId = robyn.Id },
        };

        context.MealThemes.AddRange(defaultThemes);
        await context.SaveChangesAsync();
    }
}
