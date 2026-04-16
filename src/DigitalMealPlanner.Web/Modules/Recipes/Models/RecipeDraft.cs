using System.Text.Json.Serialization;

namespace DigitalMealPlanner.Web.Modules.Recipes.Models;

public class RecipeDraft
{
    [JsonPropertyName("title")]        public string Title { get; set; } = string.Empty;
    [JsonPropertyName("description")]  public string Description { get; set; } = string.Empty;
    [JsonPropertyName("servings")]     public int Servings { get; set; } = 1;
    [JsonPropertyName("prep_minutes")] public int PrepMinutes { get; set; }
    [JsonPropertyName("cook_minutes")] public int CookMinutes { get; set; }
    [JsonPropertyName("cuisine_type")] public string CuisineType { get; set; } = string.Empty;
    [JsonPropertyName("calories_per_serving")] public double CaloriesPerServing { get; set; }
    [JsonPropertyName("protein_g")]    public double ProteinG { get; set; }
    [JsonPropertyName("carbs_g")]      public double CarbsG { get; set; }
    [JsonPropertyName("fat_g")]        public double FatG { get; set; }
    [JsonPropertyName("tags")]         public List<string> Tags { get; set; } = [];
    [JsonPropertyName("steps")]        public List<string> Steps { get; set; } = [];
    [JsonPropertyName("ingredients")]  public List<IngredientDraft> Ingredients { get; set; } = [];
}

public class IngredientDraft
{
    [JsonPropertyName("name")]              public string Name { get; set; } = string.Empty;
    [JsonPropertyName("quantity")]          public string Quantity { get; set; } = string.Empty;
    [JsonPropertyName("unit")]              public string Unit { get; set; } = string.Empty;
    [JsonPropertyName("category")]          public string Category { get; set; } = string.Empty;
    [JsonPropertyName("calories_per_unit")] public double CaloriesPerUnit { get; set; }
}
