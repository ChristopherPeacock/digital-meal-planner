using DigitalMealPlanner.Web.Infrastructure.Data.Entities;

namespace DigitalMealPlanner.Web.Modules.Cookbooks;

public interface ICookbookService
{
    Task<IEnumerable<Cookbook>> GetAllAsync(string userId);
    Task<Cookbook?> GetByIdAsync(int id, string userId);
    Task<Cookbook> CreateAsync(string userId, string name, string description, string coverImagePath = "");
    Task UpdateAsync(int id, string userId, string name, string description, string? coverImagePath = null);
    Task DeleteAsync(int id, string userId);
}
