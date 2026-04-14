namespace DigitalMealPlanner.Web.Infrastructure.Storage;

public interface IImageStorageService
{
    Task<string> SaveAsync(IFormFile file);
    void Delete(string relativePath);
}
