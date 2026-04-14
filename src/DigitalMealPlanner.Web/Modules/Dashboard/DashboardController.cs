using DigitalMealPlanner.Web.Modules.Cookbooks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DigitalMealPlanner.Web.Modules.Dashboard;

[Authorize]
public class DashboardController(ICookbookService cookbookService) : Controller
{
    public async Task<IActionResult> Index()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var cookbooks = await cookbookService.GetAllAsync(userId);
        return View(cookbooks.Take(6));
    }
}
