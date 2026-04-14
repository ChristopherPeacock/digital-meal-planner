namespace DigitalMealPlanner.Web.Infrastructure.Data.Entities;

public class Recipe
{
    public int Id { get; set; }
    public int CookbookId { get; set; }
    public Cookbook Cookbook { get; set; } = null!;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Servings { get; set; } = 1;
    public int PrepMinutes { get; set; }
    public int CookMinutes { get; set; }
    public string CuisineType { get; set; } = string.Empty;
    public int CaloriesPerServing { get; set; }
    public int ProteinG { get; set; }
    public int CarbsG { get; set; }
    public int FatG { get; set; }
    public string ImagePath { get; set; } = string.Empty;
    public string StepsJson { get; set; } = "[]";
    public string TagsJson { get; set; } = "[]";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ICollection<Ingredient> Ingredients { get; set; } = [];
    public ICollection<MealPlanEntry> MealPlanEntries { get; set; } = [];
}
