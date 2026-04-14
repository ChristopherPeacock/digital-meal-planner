using DigitalMealPlanner.Web.Infrastructure.Data.Entities;

namespace DigitalMealPlanner.Web.Modules.Recipes;

public interface IRecipeService
{
    Task<IEnumerable<Recipe>> GetByCookbookAsync(int cookbookId, string userId);
    Task<Recipe?> GetByIdAsync(int id, string userId);
    Task<Recipe> CreateAsync(Recipe recipe, string userId);
    Task UpdateAsync(Recipe recipe, string userId);
    Task DeleteAsync(int id, string userId);
}
