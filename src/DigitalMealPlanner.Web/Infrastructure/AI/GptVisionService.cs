using System.Text.Json;
using Anthropic.SDK;
using Anthropic.SDK.Messaging;
using DigitalMealPlanner.Web.Modules.Recipes.Models;

namespace DigitalMealPlanner.Web.Infrastructure.AI;

public class GptVisionService(AnthropicClient client, IWebHostEnvironment env, ILogger<GptVisionService> logger)
    : IGptVisionService
{
    private const string Model = "claude-opus-4-6";

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
        var mediaType = GetMediaType(fullPath);

        var messages = new List<Message>
        {
            new()
            {
                Role = RoleType.User,
                Content =
                [
                    new ImageContent
                    {
                        Source = new ImageSource
                        {
                            Type      = SourceType.base64,
                            MediaType = mediaType,
                            Data      = base64
                        }
                    },
                    new TextContent { Text = Prompt }
                ]
            }
        };

        var request = new MessageParameters
        {
            Model    = Model,
            MaxTokens = 2048,
            Messages = messages
        };

        var response = await client.Messages.GetClaudeMessageAsync(request);
        var json = response.Content.OfType<TextContent>().FirstOrDefault()?.Text
            ?? throw new InvalidOperationException("Claude returned no text content.");

        // Strip markdown code fences if Claude wrapped it anyway
        json = json.Trim();
        if (json.StartsWith("```"))
        {
            json = json.Split('\n').Skip(1).TakeWhile(l => !l.StartsWith("```")).Aggregate((a, b) => a + "\n" + b);
        }

        logger.LogDebug("Claude vision response: {Json}", json);

        return JsonSerializer.Deserialize<RecipeDraft>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? throw new InvalidOperationException("Claude returned unparseable JSON.");
    }

    private static string GetMediaType(string path) =>
        Path.GetExtension(path).ToLower() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png"            => "image/png",
            ".webp"           => "image/webp",
            _                 => "image/jpeg"
        };
}
