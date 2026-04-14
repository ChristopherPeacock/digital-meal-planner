using System.Text.Json;
using DigitalMealPlanner.Web.Modules.Recipes.Models;
using OpenAI.Chat;

namespace DigitalMealPlanner.Web.Infrastructure.AI;

public class GptVisionService(ChatClient chatClient, IWebHostEnvironment env, ILogger<GptVisionService> logger)
    : IGptVisionService
{
    private const string Prompt = """
        You are a recipe extraction assistant.
        The user has uploaded a screenshot of a recipe from TikTok or social media.
        Extract the full recipe and return ONLY valid JSON — no markdown, no explanation, no code block.

        Schema:
        {
          "title": "string",
          "description": "string",
          "servings": number,
          "prep_minutes": number,
          "cook_minutes": number,
          "cuisine_type": "string",
          "calories_per_serving": number,
          "protein_g": number,
          "carbs_g": number,
          "fat_g": number,
          "tags": ["string"],
          "ingredients": [
            {
              "name": "string",
              "quantity": "string",
              "unit": "string",
              "category": "produce|dairy|meat|seafood|pantry|spices|frozen|bakery|other",
              "calories_per_unit": number
            }
          ],
          "steps": ["string"]
        }

        Estimate any nutritional values not explicitly shown. Use null only if truly impossible to estimate.
        Return only the JSON object.
        """;

    public async Task<RecipeDraft> ParseRecipeFromImageAsync(string imagePath)
    {
        var fullPath = Path.Combine(env.WebRootPath, imagePath.TrimStart('/'));
        var imageBytes = await File.ReadAllBytesAsync(fullPath);
        var base64 = Convert.ToBase64String(imageBytes);
        var mimeType = GetMimeType(fullPath);

        var messages = new List<ChatMessage>
        {
            new UserChatMessage(
                ChatMessageContentPart.CreateTextPart(Prompt),
                ChatMessageContentPart.CreateImagePart(
                    new BinaryData(imageBytes),
                    mimeType))
        };

        var response = await chatClient.CompleteChatAsync(messages);
        var json = response.Value.Content[0].Text;

        logger.LogDebug("GPT-4o response: {Json}", json);

        return JsonSerializer.Deserialize<RecipeDraft>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? throw new InvalidOperationException("GPT-4o returned unparseable JSON.");
    }

    private static string GetMimeType(string path) =>
        Path.GetExtension(path).ToLower() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png"            => "image/png",
            ".webp"           => "image/webp",
            _                 => "image/jpeg"
        };
}
