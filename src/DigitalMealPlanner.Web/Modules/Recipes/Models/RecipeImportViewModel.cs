using System.ComponentModel.DataAnnotations;

namespace DigitalMealPlanner.Web.Modules.Recipes.Models;

public class RecipeImportViewModel
{
    public int CookbookId { get; set; }
    public string CookbookName { get; set; } = string.Empty;
    public List<IFormFile>? Images { get; set; }
}

public class RecipeReviewViewModel
{
    public int CookbookId { get; set; }
    public string CookbookName { get; set; } = string.Empty;
    public List<string> ImagePaths { get; set; } = [];
    public int SelectedImageIndex { get; set; }

    [Required] public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Servings { get; set; } = 1;
    public int PrepMinutes { get; set; }
    public int CookMinutes { get; set; }
    public string CuisineType { get; set; } = string.Empty;
    public int CaloriesPerServing { get; set; }
    public int ProteinG { get; set; }
    public int CarbsG { get; set; }
    public int FatG { get; set; }
    public string Tags { get; set; } = string.Empty;          // comma-separated
    public string StepsRaw { get; set; } = string.Empty;      // newline-separated
    public List<IngredientReviewRow> Ingredients { get; set; } = [];
}

public class IngredientReviewRow
{
    public string Name { get; set; } = string.Empty;
    public string Quantity { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int CaloriesPerUnit { get; set; }
}
