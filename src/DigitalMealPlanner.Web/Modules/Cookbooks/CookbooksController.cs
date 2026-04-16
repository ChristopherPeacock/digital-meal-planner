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

    [HttpGet("/cookbooks")]
    public async Task<IActionResult> Index() =>
        View(await cookbookService.GetAllAsync(UserId));

    [HttpGet("/cookbooks/{id}")]
    public async Task<IActionResult> Detail(int id)
    {
        var cookbook = await cookbookService.GetByIdAsync(id, UserId);
        if (cookbook is null) return NotFound();
        return View(cookbook);
    }

    [HttpGet("/cookbooks/create")]
    public IActionResult Create() => View(new CookbookFormViewModel());

    [HttpPost("/cookbooks/create"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CookbookFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var imagePath = string.Empty;
        if (model.CoverImage is not null)
            imagePath = await imageStorage.SaveAsync(model.CoverImage);

        var cookbook = await cookbookService.CreateAsync(UserId, model.Name, model.Description ?? string.Empty, imagePath);
        return Redirect($"/cookbooks/{cookbook.Id}");
    }

    [HttpGet("/cookbooks/{id}/edit")]
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

    [HttpPost("/cookbooks/{id}/edit"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CookbookFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        string? newImagePath = null;
        if (model.CoverImage is not null)
            newImagePath = await imageStorage.SaveAsync(model.CoverImage);

        await cookbookService.UpdateAsync(id, UserId, model.Name, model.Description ?? string.Empty, newImagePath);
        return Redirect($"/cookbooks/{id}");
    }

    [HttpPost("/cookbooks/{id}/delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await cookbookService.DeleteAsync(id, UserId);
        return Redirect("/cookbooks");
    }
}
