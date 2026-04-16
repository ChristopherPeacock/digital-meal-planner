using System.Text.Json;
using Anthropic.SDK;
using Anthropic.SDK.Messaging;
using DigitalMealPlanner.Web.Modules.Recipes.Models;

namespace DigitalMealPlanner.Web.Infrastructure.AI;

public class GptVisionService(AnthropicClient client, ILogger<GptVisionService> logger)
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

    public async Task<RecipeDraft> ParseRecipeFromImageAsync(IReadOnlyList<string> imageDataUris)
    {
        var contentParts = new List<ContentBase>();

        foreach (var dataUri in imageDataUris)
        {
            var (mediaType, base64) = ParseDataUri(dataUri);
            contentParts.Add(new ImageContent
            {
                Source = new ImageSource
                {
                    Type      = SourceType.base64,
                    MediaType = mediaType,
                    Data      = base64
                }
            });
        }

        contentParts.Add(new TextContent { Text = Prompt });

        var messages = new List<Message>
        {
            new()
            {
                Role = RoleType.User,
                Content = contentParts
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

    private static (string mediaType, string base64) ParseDataUri(string dataUri)
    {
        // Format: data:<mediaType>;base64,<data>
        var comma = dataUri.IndexOf(',');
        var header = dataUri[5..comma]; // strip leading "data:"
        var semicolon = header.IndexOf(';');
        return (header[..semicolon], dataUri[(comma + 1)..]);
    }
}
