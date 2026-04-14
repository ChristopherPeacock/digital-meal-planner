namespace DigitalMealPlanner.Web.Infrastructure.Data.Entities;

public class MealTheme
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string ColourHex { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    public ICollection<MealPlanEntry> MealPlanEntries { get; set; } = [];
}
