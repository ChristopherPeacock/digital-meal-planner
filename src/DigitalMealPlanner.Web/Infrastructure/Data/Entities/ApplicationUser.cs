using Microsoft.AspNetCore.Identity;

namespace DigitalMealPlanner.Web.Infrastructure.Data.Entities;

public class ApplicationUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;
    public int DailyCalorieTarget { get; set; } = 2000;
    public ICollection<Cookbook> Cookbooks { get; set; } = [];
    public ICollection<MealTheme> MealThemes { get; set; } = [];
    public ICollection<MealPlanEntry> MealPlanEntries { get; set; } = [];
}
