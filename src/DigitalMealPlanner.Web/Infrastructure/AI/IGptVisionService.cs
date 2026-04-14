using DigitalMealPlanner.Web.Modules.Recipes.Models;

namespace DigitalMealPlanner.Web.Infrastructure.AI;

public interface IGptVisionService
{
    Task<RecipeDraft> ParseRecipeFromImageAsync(string imagePath);
}
