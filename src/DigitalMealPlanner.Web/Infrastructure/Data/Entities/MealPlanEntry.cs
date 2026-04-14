namespace DigitalMealPlanner.Web.Infrastructure.Data.Entities;

public class MealPlanEntry
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    public DateOnly Date { get; set; }
    public int MealThemeId { get; set; }
    public MealTheme MealTheme { get; set; } = null!;
    public int RecipeId { get; set; }
    public Recipe Recipe { get; set; } = null!;
    public int ServingsPlanned { get; set; } = 1;
}
