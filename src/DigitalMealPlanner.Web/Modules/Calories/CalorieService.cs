using DigitalMealPlanner.Web.Infrastructure.Data;
using DigitalMealPlanner.Web.Modules.Calories.Models;
using Microsoft.EntityFrameworkCore;

namespace DigitalMealPlanner.Web.Modules.Calories;

public class CalorieService(AppDbContext db) : ICalorieService
{
    public async Task<DayCalorieSummary> GetDaySummaryAsync(string userId, DateOnly date, int targetCalories)
    {
        var entries = await db.MealPlanEntries
            .Where(e => e.UserId == userId && e.Date == date)
            .Include(e => e.Recipe)
            .ToListAsync();

        var total = entries.Sum(e => e.Recipe.CaloriesPerServing * e.ServingsPlanned);
        return new DayCalorieSummary(date, total, targetCalories, targetCalories - total);
    }

    public async Task<WeekCalorieSummary> GetWeekSummaryAsync(string userId, DateOnly weekStart, int targetCalories)
    {
        var weekEnd = weekStart.AddDays(6);
        var entries = await db.MealPlanEntries
            .Where(e => e.UserId == userId && e.Date >= weekStart && e.Date <= weekEnd)
            .Include(e => e.Recipe)
            .ToListAsync();

        var days = Enumerable.Range(0, 7)
            .Select(i =>
            {
                var date = weekStart.AddDays(i);
                var dayEntries = entries.Where(e => e.Date == date).ToList();
                var total = dayEntries.Sum(e => e.Recipe.CaloriesPerServing * e.ServingsPlanned);
                return new DayCalorieSummary(date, total, targetCalories, targetCalories - total);
            })
            .ToList();

        return new WeekCalorieSummary(weekStart, targetCalories, days);
    }
}
