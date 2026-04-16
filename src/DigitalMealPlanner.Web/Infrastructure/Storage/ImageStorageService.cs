namespace DigitalMealPlanner.Web.Infrastructure.Storage;

public class ImageStorageService : IImageStorageService
{
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];

    public async Task<string> SaveAsync(IFormFile file)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            throw new InvalidOperationException($"File type '{ext}' is not allowed.");

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var base64 = Convert.ToBase64String(ms.ToArray());
        var mediaType = GetMediaType(ext);
        return $"data:{mediaType};base64,{base64}";
    }

    public void Delete(string dataUri)
    {
        // Images are stored as base64 data URIs in the database — nothing to delete from disk.
    }

    private static string GetMediaType(string ext) =>
        ext switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png"            => "image/png",
            ".webp"           => "image/webp",
            _                 => "image/jpeg"
        };
}
