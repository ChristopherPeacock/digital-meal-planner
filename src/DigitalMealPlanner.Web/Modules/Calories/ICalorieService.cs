using DigitalMealPlanner.Web.Modules.Calories.Models;

namespace DigitalMealPlanner.Web.Modules.Calories;

public interface ICalorieService
{
    Task<DayCalorieSummary> GetDaySummaryAsync(string userId, DateOnly date, int targetCalories);
    Task<WeekCalorieSummary> GetWeekSummaryAsync(string userId, DateOnly weekStart, int targetCalories);
}
