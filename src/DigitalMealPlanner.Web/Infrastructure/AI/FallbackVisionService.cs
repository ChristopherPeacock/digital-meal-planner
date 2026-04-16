using DigitalMealPlanner.Web.Modules.Recipes.Models;

namespace DigitalMealPlanner.Web.Infrastructure.AI;

/// <summary>
/// Tries Claude first; if it throws for any reason, falls back to OpenAI.
/// </summary>
public class FallbackVisionService(
    GptVisionService claude,
    OpenAiVisionService openAi,
    ILogger<FallbackVisionService> logger) : IGptVisionService
{
    public async Task<RecipeDraft> ParseRecipeFromImageAsync(string imagePath)
    {
        try
        {
            return await claude.ParseRecipeFromImageAsync(imagePath);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Claude vision failed, falling back to OpenAI.");
            return await openAi.ParseRecipeFromImageAsync(imagePath);
        }
    }
}
