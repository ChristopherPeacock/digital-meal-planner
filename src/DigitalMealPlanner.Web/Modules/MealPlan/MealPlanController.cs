using System.Security.Claims;
using DigitalMealPlanner.Web.Modules.Cookbooks;
using DigitalMealPlanner.Web.Modules.MealPlan.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalMealPlanner.Web.Modules.MealPlan;

[Authorize]
public class MealPlanController(
    IMealPlanService mealPlanService,
    ICookbookService cookbookService) : Controller
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet("/mealplan")]
    public async Task<IActionResult> Index(string? week)
    {
        // Determine week start (Monday)
        var today = DateOnly.FromDateTime(DateTime.Today);
        DateOnly weekStart;
        if (week is not null && DateOnly.TryParse(week, out var parsed))
            weekStart = parsed;
        else
            weekStart = today;

        // Snap to Monday
        var daysFromMonday = ((int)weekStart.DayOfWeek + 6) % 7;
        weekStart = weekStart.AddDays(-daysFromMonday);

        var days = Enumerable.Range(0, 7).Select(i => weekStart.AddDays(i)).ToList();

        var entries = await mealPlanService.GetWeekAsync(UserId, weekStart);
        var themes = await mealPlanService.GetThemesAsync(UserId);
        var cookbooks = await cookbookService.GetAllAsync(UserId);

        var entryMap = new Dictionary<string, List<Infrastructure.Data.Entities.MealPlanEntry>>();
        foreach (var entry in entries)
        {
            var key = $"{entry.Date:yyyy-MM-dd}_{entry.MealThemeId}";
            if (!entryMap.TryGetValue(key, out var list))
                entryMap[key] = list = [];
            list.Add(entry);
        }

        var vm = new MealPlanWeekViewModel
        {
            WeekStart = weekStart,
            Days = days,
            Themes = themes.ToList(),
            EntryMap = entryMap,
            Cookbooks = cookbooks.ToList()
        };

        return View("~/Modules/MealPlan/Views/Index.cshtml", vm);
    }

    [HttpPost("/mealplan/assign")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Assign([FromForm] string date, [FromForm] int themeId, [FromForm] int recipeId)
    {
        if (!DateOnly.TryParse(date, out var dateOnly))
            return BadRequest(new { error = "Invalid date." });

        try
        {
            var entry = await mealPlanService.AssignAsync(UserId, dateOnly, themeId, recipeId, 1);
            return Ok(new
            {
                entryId   = entry.Id,
                title     = entry.Recipe.Title,
                themeColor = entry.MealTheme.ColourHex
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("/mealplan/remove")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove([FromForm] int entryId)
    {
        try
        {
            await mealPlanService.RemoveAsync(entryId, UserId);
            return Ok();
        }
        catch (UnauthorizedAccessException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
