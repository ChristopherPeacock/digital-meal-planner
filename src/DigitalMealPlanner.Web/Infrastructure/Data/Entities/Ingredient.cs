namespace DigitalMealPlanner.Web.Infrastructure.Data.Entities;

public class Ingredient
{
    public int Id { get; set; }
    public int RecipeId { get; set; }
    public Recipe Recipe { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string Quantity { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int CaloriesPerUnit { get; set; }
}
