using System.Security.Claims;
using System.Text.Json;
using DigitalMealPlanner.Web.Infrastructure.AI;
using DigitalMealPlanner.Web.Infrastructure.Data.Entities;
using DigitalMealPlanner.Web.Infrastructure.Storage;
using DigitalMealPlanner.Web.Modules.Cookbooks;
using DigitalMealPlanner.Web.Modules.Recipes.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalMealPlanner.Web.Modules.Recipes;

[Authorize]
public class RecipeImportController(
    ICookbookService cookbookService,
    IRecipeService recipeService,
    IImageStorageService imageStorage,
    IGptVisionService gptVision) : Controller
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet("/cookbooks/{cookbookId}/recipes/import")]
    public async Task<IActionResult> Import(int cookbookId)
    {
        var cookbook = await cookbookService.GetByIdAsync(cookbookId, UserId);
        if (cookbook is null) return NotFound();

        return View("~/Modules/Recipes/Views/Import.cshtml", new RecipeImportViewModel
        {
            CookbookId = cookbookId,
            CookbookName = cookbook.Name
        });
    }

    [HttpPost("/cookbooks/{cookbookId}/recipes/import")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Import(int cookbookId, RecipeImportViewModel model)
    {
        var cookbook = await cookbookService.GetByIdAsync(cookbookId, UserId);
        if (cookbook is null) return NotFound();

        if (model.Images is null || model.Images.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "Please select at least one image.");
            model.CookbookName = cookbook.Name;
            return View("~/Modules/Recipes/Views/Import.cshtml", model);
        }

        // Save all images
        var imagePaths = new List<string>();
        foreach (var img in model.Images)
        {
            if (img.Length > 0)
                imagePaths.Add(await imageStorage.SaveAsync(img));
        }

        // Call Claude Vision with all images
        RecipeDraft draft;
        try
        {
            draft = await gptVision.ParseRecipeFromImageAsync(imagePaths);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"AI extraction failed: {ex.Message}. You can enter the recipe manually.");
            model.CookbookName = cookbook.Name;
            return View("~/Modules/Recipes/Views/Import.cshtml", model);
        }

        // Map draft -> review view model
        var review = new RecipeReviewViewModel
        {
            CookbookId = cookbookId,
            CookbookName = cookbook.Name,
            ImagePaths = imagePaths,
            SelectedImageIndex = 0,
            Title = draft.Title,
            Description = draft.Description,
            Servings = draft.Servings > 0 ? draft.Servings : 1,
            PrepMinutes = draft.PrepMinutes,
            CookMinutes = draft.CookMinutes,
            CuisineType = draft.CuisineType,
            CaloriesPerServing = (int)Math.Round(draft.CaloriesPerServing),
            ProteinG = (int)Math.Round(draft.ProteinG),
            CarbsG = (int)Math.Round(draft.CarbsG),
            FatG = (int)Math.Round(draft.FatG),
            Tags = string.Join(", ", draft.Tags),
            StepsRaw = string.Join("\n", draft.Steps),
            Ingredients = draft.Ingredients.Select(i => new IngredientReviewRow
            {
                Name = i.Name,
                Quantity = i.Quantity,
                Unit = i.Unit,
                Category = i.Category,
                CaloriesPerUnit = (int)Math.Round(i.CaloriesPerUnit)
            }).ToList()
        };

        return View("~/Modules/Recipes/Views/Review.cshtml", review);
    }

    [HttpPost("/cookbooks/{cookbookId}/recipes/import/save")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(int cookbookId, RecipeReviewViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var cookbook = await cookbookService.GetByIdAsync(cookbookId, UserId);
            model.CookbookName = cookbook?.Name ?? string.Empty;
            return View("~/Modules/Recipes/Views/Review.cshtml", model);
        }

        var steps = model.StepsRaw
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => s.Length > 0)
            .ToList();

        var tags = model.Tags
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(t => t.Trim())
            .Where(t => t.Length > 0)
            .ToList();

        var recipe = new Recipe
        {
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
            ImagePath = model.ImagePaths.ElementAtOrDefault(model.SelectedImageIndex) ?? model.ImagePaths.FirstOrDefault() ?? string.Empty,
            StepsJson = JsonSerializer.Serialize(steps),
            TagsJson = JsonSerializer.Serialize(tags),
            Ingredients = model.Ingredients.Select(i => new Ingredient
            {
                Name = i.Name,
                Quantity = i.Quantity,
                Unit = i.Unit,
                Category = i.Category,
                CaloriesPerUnit = i.CaloriesPerUnit
            }).ToList()
        };

        var saved = await recipeService.CreateAsync(recipe, UserId);
        return RedirectToAction("Detail", "Recipes", new { cookbookId, recipeId = saved.Id });
    }
}
