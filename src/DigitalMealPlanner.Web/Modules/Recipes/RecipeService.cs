using DigitalMealPlanner.Web.Infrastructure.Data;
using DigitalMealPlanner.Web.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace DigitalMealPlanner.Web.Modules.Recipes;

public class RecipeService(AppDbContext db) : IRecipeService
{
    public async Task<IEnumerable<Recipe>> GetByCookbookAsync(int cookbookId, string userId)
    {
        var cookbookBelongsToUser = await db.Cookbooks
            .AnyAsync(c => c.Id == cookbookId && c.UserId == userId);

        if (!cookbookBelongsToUser)
            throw new UnauthorizedAccessException("Cookbook not found or access denied.");

        return await db.Recipes
            .Where(r => r.CookbookId == cookbookId)
            .Include(r => r.Ingredients)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<Recipe?> GetByIdAsync(int id, string userId) =>
        await db.Recipes
            .Where(r => r.Id == id && r.Cookbook.UserId == userId)
            .Include(r => r.Ingredients)
            .Include(r => r.Cookbook)
            .FirstOrDefaultAsync();

    public async Task<Recipe> CreateAsync(Recipe recipe, string userId)
    {
        var cookbookBelongsToUser = await db.Cookbooks
            .AnyAsync(c => c.Id == recipe.CookbookId && c.UserId == userId);

        if (!cookbookBelongsToUser)
            throw new UnauthorizedAccessException("Cookbook not found or access denied.");

        recipe.CreatedAt = DateTime.UtcNow;
        recipe.UpdatedAt = DateTime.UtcNow;
        db.Recipes.Add(recipe);
        await db.SaveChangesAsync();
        return recipe;
    }

    public async Task UpdateAsync(Recipe recipe, string userId)
    {
        var existing = await db.Recipes
            .Include(r => r.Ingredients)
            .FirstOrDefaultAsync(r => r.Id == recipe.Id && r.Cookbook.UserId == userId)
            ?? throw new UnauthorizedAccessException("Recipe not found or access denied.");

        existing.Title = recipe.Title;
        existing.Description = recipe.Description;
        existing.Servings = recipe.Servings;
        existing.PrepMinutes = recipe.PrepMinutes;
        existing.CookMinutes = recipe.CookMinutes;
        existing.CuisineType = recipe.CuisineType;
        existing.CaloriesPerServing = recipe.CaloriesPerServing;
        existing.ProteinG = recipe.ProteinG;
        existing.CarbsG = recipe.CarbsG;
        existing.FatG = recipe.FatG;
        existing.StepsJson = recipe.StepsJson;
        existing.TagsJson = recipe.TagsJson;
        existing.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrEmpty(recipe.ImagePath))
            existing.ImagePath = recipe.ImagePath;

        db.Ingredients.RemoveRange(existing.Ingredients);
        existing.Ingredients = recipe.Ingredients;

        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id, string userId)
    {
        var recipe = await db.Recipes
            .FirstOrDefaultAsync(r => r.Id == id && r.Cookbook.UserId == userId)
            ?? throw new UnauthorizedAccessException("Recipe not found or access denied.");

        var mealPlanEntries = await db.MealPlanEntries
            .Where(e => e.RecipeId == id)
            .ToListAsync();
        db.MealPlanEntries.RemoveRange(mealPlanEntries);

        db.Recipes.Remove(recipe);
        await db.SaveChangesAsync();
    }
}
