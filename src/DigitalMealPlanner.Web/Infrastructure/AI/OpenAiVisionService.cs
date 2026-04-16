using System.Text.Json;
using OpenAI.Chat;
using DigitalMealPlanner.Web.Modules.Recipes.Models;

namespace DigitalMealPlanner.Web.Infrastructure.AI;

public class OpenAiVisionService(IConfiguration configuration, ILogger<OpenAiVisionService> logger)
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

    public async Task<RecipeDraft> ParseRecipeFromImageAsync(string imageDataUri)
    {
        var apiKey =
            Environment.GetEnvironmentVariable("OPENAI_API", EnvironmentVariableTarget.Process)
            ?? Environment.GetEnvironmentVariable("OPENAI_API", EnvironmentVariableTarget.User)
            ?? Environment.GetEnvironmentVariable("OPENAI_API", EnvironmentVariableTarget.Machine)
            ?? configuration["OPENAI_API"]   // flat env var via ASP.NET Core config
            ?? configuration["OpenAI:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("OpenAI API key is not configured. Set the OPENAI_API_KEY environment variable or OpenAI:ApiKey in appsettings.");

        var model = configuration["OpenAI:Model"] ?? "gpt-4o";

        var (mediaType, imageBytes) = ParseDataUri(imageDataUri);

        var client = new ChatClient(model, apiKey);

        ChatCompletion completion = await client.CompleteChatAsync(
        [
            new UserChatMessage(
                ChatMessageContentPart.CreateImagePart(BinaryData.FromBytes(imageBytes), mediaType),
                ChatMessageContentPart.CreateTextPart(Prompt)
            )
        ]);

        var json = completion.Content[0].Text;

        // Strip markdown code fences if the model wrapped it anyway
        json = json.Trim();
        if (json.StartsWith("```"))
        {
            json = json.Split('\n').Skip(1).TakeWhile(l => !l.StartsWith("```")).Aggregate((a, b) => a + "\n" + b);
        }

        logger.LogDebug("OpenAI vision response: {Json}", json);

        return JsonSerializer.Deserialize<RecipeDraft>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? throw new InvalidOperationException("OpenAI returned unparseable JSON.");
    }

    private static (string mediaType, byte[] bytes) ParseDataUri(string dataUri)
    {
        // Format: data:<mediaType>;base64,<data>
        var comma = dataUri.IndexOf(',');
        var header = dataUri[5..comma]; // strip leading "data:"
        var semicolon = header.IndexOf(';');
        return (header[..semicolon], Convert.FromBase64String(dataUri[(comma + 1)..]));
    }
}
