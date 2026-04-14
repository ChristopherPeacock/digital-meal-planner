using System.ComponentModel.DataAnnotations;

namespace DigitalMealPlanner.Web.Modules.Cookbooks.Models;

public class CookbookFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Cookbook name is required")]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    public string? ExistingCoverImagePath { get; set; }
    public IFormFile? CoverImage { get; set; }
}
