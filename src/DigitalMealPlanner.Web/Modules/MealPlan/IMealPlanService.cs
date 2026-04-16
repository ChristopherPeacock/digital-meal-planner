using DigitalMealPlanner.Web.Infrastructure.Data.Entities;

namespace DigitalMealPlanner.Web.Modules.MealPlan;

public interface IMealPlanService
{
    Task<IEnumerable<MealPlanEntry>> GetWeekAsync(string userId, DateOnly weekStart);
    Task<IEnumerable<MealTheme>> GetThemesAsync(string userId);
    Task<MealPlanEntry> AssignAsync(string userId, DateOnly date, int themeId, int recipeId, int servings);
    Task RemoveAsync(int entryId, string userId);
}
