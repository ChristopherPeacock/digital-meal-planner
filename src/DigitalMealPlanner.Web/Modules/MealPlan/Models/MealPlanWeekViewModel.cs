using DigitalMealPlanner.Web.Infrastructure.Data.Entities;

namespace DigitalMealPlanner.Web.Modules.MealPlan.Models;

public class MealPlanWeekViewModel
{
    public DateOnly WeekStart { get; set; }
    public List<DateOnly> Days { get; set; } = [];
    public List<MealTheme> Themes { get; set; } = [];
    /// <summary>Keyed by "yyyy-MM-dd_themeId"</summary>
    public Dictionary<string, List<MealPlanEntry>> EntryMap { get; set; } = [];
    public List<Cookbook> Cookbooks { get; set; } = [];

    public List<MealPlanEntry> GetEntries(DateOnly date, int themeId)
    {
        var key = $"{date:yyyy-MM-dd}_{themeId}";
        return EntryMap.TryGetValue(key, out var list) ? list : [];
    }
}
