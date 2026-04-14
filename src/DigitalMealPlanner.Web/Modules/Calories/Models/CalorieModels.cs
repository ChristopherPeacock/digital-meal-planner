namespace DigitalMealPlanner.Web.Modules.Calories.Models;

public record DayCalorieSummary(
    DateOnly Date,
    int TotalCalories,
    int TargetCalories,
    int Deficit)
{
    public double PercentageConsumed =>
        TargetCalories > 0 ? Math.Min(100, TotalCalories * 100.0 / TargetCalories) : 0;
}

public record WeekCalorieSummary(
    DateOnly WeekStart,
    int TargetCalories,
    IReadOnlyList<DayCalorieSummary> Days)
{
    public int TotalWeekCalories => Days.Sum(d => d.TotalCalories);
    public int AverageDailyCalories => Days.Count > 0 ? TotalWeekCalories / Days.Count : 0;
    public int WeeklyTarget => TargetCalories * 7;
    public int WeeklyDeficit => WeeklyTarget - TotalWeekCalories;
}
