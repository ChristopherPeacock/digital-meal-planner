using DigitalMealPlanner.Web.Infrastructure.Data;
using DigitalMealPlanner.Web.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace DigitalMealPlanner.Web.Modules.Cookbooks;

public class CookbookService(AppDbContext db) : ICookbookService
{
    public async Task<IEnumerable<Cookbook>> GetAllAsync(string userId) =>
        await db.Cookbooks
            .Where(c => c.UserId == userId)
            .Include(c => c.Recipes)
            .OrderByDescending(c => c.UpdatedAt)
            .ToListAsync();

    public async Task<Cookbook?> GetByIdAsync(int id, string userId) =>
        await db.Cookbooks
            .Where(c => c.Id == id && c.UserId == userId)
            .Include(c => c.Recipes)
            .ThenInclude(r => r.Ingredients)
            .FirstOrDefaultAsync();

    public async Task<Cookbook> CreateAsync(string userId, string name, string description, string coverImagePath = "")
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Cookbook name is required.", nameof(name));

        var cookbook = new Cookbook
        {
            UserId = userId,
            Name = name.Trim(),
            Description = description.Trim(),
            CoverImagePath = coverImagePath,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Cookbooks.Add(cookbook);
        await db.SaveChangesAsync();
        return cookbook;
    }

    public async Task UpdateAsync(int id, string userId, string name, string description, string? coverImagePath = null)
    {
        var cookbook = await db.Cookbooks.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId)
            ?? throw new UnauthorizedAccessException("Cookbook not found or access denied.");

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Cookbook name is required.", nameof(name));

        cookbook.Name = name.Trim();
        cookbook.Description = description.Trim();
        cookbook.UpdatedAt = DateTime.UtcNow;

        if (coverImagePath is not null)
            cookbook.CoverImagePath = coverImagePath;

        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id, string userId)
    {
        var cookbook = await db.Cookbooks.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId)
            ?? throw new UnauthorizedAccessException("Cookbook not found or access denied.");

        db.Cookbooks.Remove(cookbook);
        await db.SaveChangesAsync();
    }
}
