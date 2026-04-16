using DigitalMealPlanner.Web.Infrastructure.Data;
using DigitalMealPlanner.Web.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace DigitalMealPlanner.Web.Modules.MealPlan;

public class MealPlanService(AppDbContext db) : IMealPlanService
{
    public async Task<IEnumerable<MealPlanEntry>> GetWeekAsync(string userId, DateOnly weekStart)
    {
        var weekEnd = weekStart.AddDays(6);
        return await db.MealPlanEntries
            .Where(e => e.UserId == userId && e.Date >= weekStart && e.Date <= weekEnd)
            .Include(e => e.Recipe)
            .Include(e => e.MealTheme)
            .OrderBy(e => e.Date)
            .ThenBy(e => e.MealTheme.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<MealTheme>> GetThemesAsync(string userId) =>
        await db.MealThemes
            .Where(t => t.UserId == userId)
            .OrderBy(t => t.IsDefault ? 0 : 1)
            .ThenBy(t => t.Name)
            .ToListAsync();

    public async Task<MealPlanEntry> AssignAsync(string userId, DateOnly date, int themeId, int recipeId, int servings)
    {
        var recipeExists = await db.Recipes
            .AnyAsync(r => r.Id == recipeId && r.Cookbook.UserId == userId);
        if (!recipeExists)
            throw new UnauthorizedAccessException("Recipe not found or access denied.");

        var themeExists = await db.MealThemes
            .AnyAsync(t => t.Id == themeId && t.UserId == userId);
        if (!themeExists)
            throw new UnauthorizedAccessException("Meal theme not found or access denied.");

        var entry = new MealPlanEntry
        {
            UserId = userId,
            Date = date,
            MealThemeId = themeId,
            RecipeId = recipeId,
            ServingsPlanned = servings < 1 ? 1 : servings
        };

        db.MealPlanEntries.Add(entry);
        await db.SaveChangesAsync();

        return await db.MealPlanEntries
            .Include(e => e.Recipe)
            .Include(e => e.MealTheme)
            .FirstAsync(e => e.Id == entry.Id);
    }

    public async Task RemoveAsync(int entryId, string userId)
    {
        var entry = await db.MealPlanEntries
            .FirstOrDefaultAsync(e => e.Id == entryId && e.UserId == userId)
            ?? throw new UnauthorizedAccessException("Entry not found or access denied.");

        db.MealPlanEntries.Remove(entry);
        await db.SaveChangesAsync();
    }
}
