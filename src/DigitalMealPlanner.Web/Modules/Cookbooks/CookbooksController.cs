using DigitalMealPlanner.Web.Infrastructure.Storage;
using DigitalMealPlanner.Web.Modules.Cookbooks.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DigitalMealPlanner.Web.Modules.Cookbooks;

[Authorize]
public class CookbooksController(ICookbookService cookbookService, IImageStorageService imageStorage) : Controller
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    public async Task<IActionResult> Index() =>
        View(await cookbookService.GetAllAsync(UserId));

    public async Task<IActionResult> Detail(int id)
    {
        var cookbook = await cookbookService.GetByIdAsync(id, UserId);
        if (cookbook is null) return NotFound();
        return View(cookbook);
    }

    [HttpGet]
    public IActionResult Create() => View(new CookbookFormViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CookbookFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var imagePath = string.Empty;
        if (model.CoverImage is not null)
            imagePath = await imageStorage.SaveAsync(model.CoverImage);

        var cookbook = await cookbookService.CreateAsync(UserId, model.Name, model.Description, imagePath);
        return RedirectToAction(nameof(Detail), new { id = cookbook.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var cookbook = await cookbookService.GetByIdAsync(id, UserId);
        if (cookbook is null) return NotFound();

        return View(new CookbookFormViewModel
        {
            Id = cookbook.Id,
            Name = cookbook.Name,
            Description = cookbook.Description,
            ExistingCoverImagePath = cookbook.CoverImagePath
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CookbookFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        string? newImagePath = null;
        if (model.CoverImage is not null)
            newImagePath = await imageStorage.SaveAsync(model.CoverImage);

        await cookbookService.UpdateAsync(id, UserId, model.Name, model.Description, newImagePath);
        return RedirectToAction(nameof(Detail), new { id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await cookbookService.DeleteAsync(id, UserId);
        return RedirectToAction(nameof(Index));
    }
}
