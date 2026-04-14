using System.Security.Claims;
using System.Text.Json;
using DigitalMealPlanner.Web.Infrastructure.Data.Entities;
using DigitalMealPlanner.Web.Infrastructure.Storage;
using DigitalMealPlanner.Web.Modules.Cookbooks;
using DigitalMealPlanner.Web.Modules.Recipes.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalMealPlanner.Web.Modules.Recipes;

[Authorize]
public class RecipesController(
    IRecipeService recipeService,
    ICookbookService cookbookService,
    IImageStorageService imageStorage) : Controller
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet("/cookbooks/{cookbookId}/recipes/{recipeId}")]
    public async Task<IActionResult> Detail(int cookbookId, int recipeId)
    {
        var recipe = await recipeService.GetByIdAsync(recipeId, UserId);
        if (recipe is null || recipe.CookbookId != cookbookId) return NotFound();
        return View(recipe);
    }

    [HttpGet("/cookbooks/{cookbookId}/recipes/{recipeId}/edit")]
    public async Task<IActionResult> Edit(int cookbookId, int recipeId)
    {
        var recipe = await recipeService.GetByIdAsync(recipeId, UserId);
        if (recipe is null || recipe.CookbookId != cookbookId) return NotFound();

        var steps = JsonSerializer.Deserialize<List<string>>(recipe.StepsJson) ?? [];
        var tags  = JsonSerializer.Deserialize<List<string>>(recipe.TagsJson)  ?? [];

        var vm = new RecipeReviewViewModel
        {
            CookbookId = cookbookId,
            CookbookName = recipe.Cookbook.Name,
            ImagePath = recipe.ImagePath,
            Title = recipe.Title,
            Description = recipe.Description,
            Servings = recipe.Servings,
            PrepMinutes = recipe.PrepMinutes,
            CookMinutes = recipe.CookMinutes,
            CuisineType = recipe.CuisineType,
            CaloriesPerServing = recipe.CaloriesPerServing,
            ProteinG = recipe.ProteinG,
            CarbsG = recipe.CarbsG,
            FatG = recipe.FatG,
            Tags = string.Join(", ", tags),
            StepsRaw = string.Join("\n", steps),
            Ingredients = recipe.Ingredients.Select(i => new IngredientReviewRow
            {
                Name = i.Name,
                Quantity = i.Quantity,
                Unit = i.Unit,
                Category = i.Category,
                CaloriesPerUnit = i.CaloriesPerUnit
            }).ToList()
        };
        return View(vm);
    }

    [HttpPost("/cookbooks/{cookbookId}/recipes/{recipeId}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int cookbookId, int recipeId, RecipeReviewViewModel model, IFormFile? newImage)
    {
        if (!ModelState.IsValid) return View(model);

        var steps = model.StepsRaw
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim()).Where(s => s.Length > 0).ToList();

        var tags = model.Tags
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(t => t.Trim()).Where(t => t.Length > 0).ToList();

        var imagePath = model.ImagePath;
        if (newImage is not null && newImage.Length > 0)
            imagePath = await imageStorage.SaveAsync(newImage);

        var recipe = new Recipe
        {
            Id = recipeId,
            CookbookId = cookbookId,
            Title = model.Title,
            Description = model.Description,
            Servings = model.Servings,
            PrepMinutes = model.PrepMinutes,
            CookMinutes = model.CookMinutes,
            CuisineType = model.CuisineType,
            CaloriesPerServing = model.CaloriesPerServing,
            ProteinG = model.ProteinG,
            CarbsG = model.CarbsG,
            FatG = model.FatG,
            ImagePath = imagePath,
            StepsJson = JsonSerializer.Serialize(steps),
            TagsJson = JsonSerializer.Serialize(tags),
            Ingredients = model.Ingredients.Select(i => new Ingredient
            {
                Name = i.Name, Quantity = i.Quantity,
                Unit = i.Unit, Category = i.Category,
                CaloriesPerUnit = i.CaloriesPerUnit
            }).ToList()
        };

        await recipeService.UpdateAsync(recipe, UserId);
        return RedirectToAction(nameof(Detail), new { cookbookId, recipeId });
    }

    [HttpPost("/cookbooks/{cookbookId}/recipes/{recipeId}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int cookbookId, int recipeId)
    {
        var recipe = await recipeService.GetByIdAsync(recipeId, UserId);
        if (recipe is not null && !string.IsNullOrEmpty(recipe.ImagePath))
            imageStorage.Delete(recipe.ImagePath);

        await recipeService.DeleteAsync(recipeId, UserId);
        return RedirectToAction("Detail", "Cookbooks", new { id = cookbookId });
    }
}
