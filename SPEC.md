# Digital Meal Planner — Full Specification

## Vision

A Progressive Web App built with ASP.NET Core MVC — monolithic, modular, test-driven.
A household cookbook manager where the user uploads TikTok recipe screenshots, GPT-4o
extracts the structured recipe data, recipes are organised into named cookbooks, and a
weekly calendar lets the user meal-plan with live calorie tracking per day and per week.

---

## Design Language (from PoC)

| Token | Value | Use |
|---|---|---|
| `--red` | `#e63946` | Primary accent, labels, highlights |
| `--bg` | `#f7f2eb` | Warm cream page background |
| `--dark` | `#1a1a1a` | Body text, dark tiles |
| `--muted` | `#888` | Secondary text, separators |
| `--accent` | `#f4a261` | Orange accent, borders |
| Heading font | `Bebas Neue` | All section labels, titles, macro values |
| Body font | `DM Sans` | All body text, ingredients, steps |
| Cards | White, `border-radius: .75rem`, `box-shadow: 0 4px 16px rgba(0,0,0,.12)` | Recipe cards, form panels |
| Ingredient rows | White, `border-left: 3px solid var(--accent)`, `border-radius: .5rem` | Ingredient lists |
| Macro tiles | `background: var(--dark)` or `var(--red)`, white text | Calorie/macro grid |

---

## Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core MVC (.NET 9) |
| Views | Razor Views + Tag Helpers + Partial Views |
| Interactivity | HTMX + vanilla JS (drag-and-drop) |
| ORM | Entity Framework Core 9 |
| Database | SQLite (`App_Data/mealplanner.db`) |
| Auth | ASP.NET Core Identity (cookie-based) |
| AI | OpenAI GPT-4o Vision (`OpenAI` NuGet) |
| PDF Export | QuestPDF (community licence) |
| Testing | xUnit + Moq + EF Core InMemory |
| CSS | Bootstrap 5 + custom CSS variables |
| PWA | Web App Manifest + Service Worker |

---

## Architecture — Monolithic Modular

One project. One deployable binary. Feature modules own their own controllers, services,
interfaces, view-models, and views. Infrastructure (EF Core, OpenAI, Identity) is shared.

```
Controllers call interfaces.
Interfaces are implemented by services.
Services use EF Core AppDbContext.
Tests mock the interfaces — never the DB directly.
```

### Modules

| Module | Responsibility |
|---|---|
| `Modules/Auth` | Login, logout, identity middleware |
| `Modules/Dashboard` | Home — recent cookbooks, quick stats |
| `Modules/Cookbooks` | Full CRUD for named cookbook collections |
| `Modules/Recipes` | Import via image + GPT-4o, CRUD, detail view |
| `Modules/Themes` | Meal slot themes (Breakfast/Dinner/Tea/Dessert + custom) |
| `Modules/MealPlan` | Weekly calendar, assign recipes to day+slot |
| `Modules/Calories` | Per-day and per-week calorie aggregation |

### Infrastructure (shared, not a module)

| Folder | Contents |
|---|---|
| `Infrastructure/Data` | `AppDbContext`, EF migrations, seeding |
| `Infrastructure/AI` | `IGptVisionService` + `GptVisionService` |
| `Infrastructure/Storage` | `IImageStorageService` + `ImageStorageService` |
| `Infrastructure/Pdf` | `ICookbookPdfService` + `CookbookPdfService` |
| `Infrastructure/Identity` | `ApplicationUser`, seed logic |

---

## Project Structure

```
digital-meal-planner/
├── SPEC.md
├── DigitalMealPlanner.sln
├── src/
│   └── DigitalMealPlanner.Web/
│       ├── Program.cs
│       ├── appsettings.json
│       ├── appsettings.Development.json
│       │
│       ├── Modules/
│       │   ├── Auth/
│       │   │   ├── AuthController.cs
│       │   │   ├── Models/
│       │   │   │   └── LoginViewModel.cs
│       │   │   └── Views/
│       │   │       └── Login.cshtml
│       │   │
│       │   ├── Dashboard/
│       │   │   ├── DashboardController.cs
│       │   │   └── Views/
│       │   │       └── Index.cshtml
│       │   │
│       │   ├── Cookbooks/
│       │   │   ├── CookbooksController.cs
│       │   │   ├── ICookbookService.cs
│       │   │   ├── CookbookService.cs
│       │   │   ├── Models/
│       │   │   │   ├── CookbookViewModel.cs
│       │   │   │   └── CookbookFormViewModel.cs
│       │   │   └── Views/
│       │   │       ├── Index.cshtml
│       │   │       ├── Detail.cshtml
│       │   │       ├── Create.cshtml
│       │   │       └── Edit.cshtml
│       │   │
│       │   ├── Recipes/
│       │   │   ├── RecipesController.cs
│       │   │   ├── RecipeImportController.cs
│       │   │   ├── IRecipeService.cs
│       │   │   ├── RecipeService.cs
│       │   │   ├── Models/
│       │   │   │   ├── RecipeViewModel.cs
│       │   │   │   ├── RecipeImportViewModel.cs
│       │   │   │   ├── RecipeDraft.cs
│       │   │   │   └── RecipeEditViewModel.cs
│       │   │   └── Views/
│       │   │       ├── Index.cshtml
│       │   │       ├── Detail.cshtml
│       │   │       ├── Import.cshtml
│       │   │       ├── Review.cshtml
│       │   │       ├── Edit.cshtml
│       │   │       └── Partials/
│       │   │           ├── _RecipeCard.cshtml
│       │   │           └── _IngredientRow.cshtml
│       │   │
│       │   ├── Themes/
│       │   │   ├── ThemesController.cs
│       │   │   ├── IThemeService.cs
│       │   │   ├── ThemeService.cs
│       │   │   ├── Models/
│       │   │   │   └── ThemeViewModel.cs
│       │   │   └── Views/
│       │   │       ├── Index.cshtml
│       │   │       └── Manage.cshtml
│       │   │
│       │   ├── MealPlan/
│       │   │   ├── MealPlanController.cs
│       │   │   ├── IMealPlanService.cs
│       │   │   ├── MealPlanService.cs
│       │   │   ├── Models/
│       │   │   │   ├── WeekViewModel.cs
│       │   │   │   └── DaySlotViewModel.cs
│       │   │   └── Views/
│       │   │       ├── Week.cshtml
│       │   │       └── Partials/
│       │   │           └── _DayColumn.cshtml
│       │   │
│       │   └── Calories/
│       │       ├── CaloriesController.cs
│       │       ├── ICalorieService.cs
│       │       ├── CalorieService.cs
│       │       └── Models/
│       │           ├── DayCalorieViewModel.cs
│       │           └── WeekSummaryViewModel.cs
│       │
│       ├── Infrastructure/
│       │   ├── Data/
│       │   │   ├── AppDbContext.cs
│       │   │   ├── DatabaseSeeder.cs
│       │   │   └── Migrations/
│       │   ├── AI/
│       │   │   ├── IGptVisionService.cs
│       │   │   └── GptVisionService.cs
│       │   ├── Storage/
│       │   │   ├── IImageStorageService.cs
│       │   │   └── ImageStorageService.cs
│       │   └── Pdf/
│       │       ├── ICookbookPdfService.cs
│       │       └── CookbookPdfService.cs
│       │
│       ├── wwwroot/
│       │   ├── manifest.json
│       │   ├── service-worker.js
│       │   ├── css/
│       │   │   └── site.css
│       │   ├── js/
│       │   │   ├── site.js
│       │   │   └── dropzone.js
│       │   ├── uploads/
│       │   └── icons/
│       │
│       └── Views/
│           ├── Shared/
│           │   ├── _Layout.cshtml
│           │   ├── _Nav.cshtml
│           │   └── Error.cshtml
│           └── _ViewImports.cshtml
│
└── tests/
    └── DigitalMealPlanner.Tests/
        ├── DigitalMealPlanner.Tests.csproj
        ├── Cookbooks/
        │   └── CookbookServiceTests.cs
        ├── Recipes/
        │   └── RecipeServiceTests.cs
        ├── MealPlan/
        │   └── MealPlanServiceTests.cs
        ├── Calories/
        │   └── CalorieServiceTests.cs
        └── Helpers/
            └── TestDbContextFactory.cs
```

---

## Data Models

### ApplicationUser (Identity)
```csharp
public class ApplicationUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;
    public ICollection<Cookbook> Cookbooks { get; set; } = [];
}
```

Seeded user:
- Username: `robyn`
- Email: `robyn@mealplanner.local`
- Password: `C00kb00k$Pl@nn3r!Mx9#`

### Cookbook
```csharp
public class Cookbook
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CoverImagePath { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ICollection<Recipe> Recipes { get; set; } = [];
}
```

### MealTheme (Breakfast / Dinner / Tea / Dessert + custom)
```csharp
public class MealTheme
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;   // "Breakfast", "Dinner", etc.
    public string Icon { get; set; } = string.Empty;   // emoji or icon class
    public string ColourHex { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ICollection<MealPlanEntry> MealPlanEntries { get; set; } = [];
}
```

Default seeded themes: Breakfast 🌅, Lunch 🥗, Dinner 🍽️, Dessert 🍰

### Recipe
```csharp
public class Recipe
{
    public int Id { get; set; }
    public int CookbookId { get; set; }
    public Cookbook Cookbook { get; set; } = null!;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Servings { get; set; }
    public int PrepMinutes { get; set; }
    public int CookMinutes { get; set; }
    public string CuisineType { get; set; } = string.Empty;
    public int CaloriesPerServing { get; set; }
    public int ProteinG { get; set; }
    public int CarbsG { get; set; }
    public int FatG { get; set; }
    public string ImagePath { get; set; } = string.Empty;
    public string StepsJson { get; set; } = "[]";
    public string TagsJson { get; set; } = "[]";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ICollection<Ingredient> Ingredients { get; set; } = [];
    public ICollection<MealPlanEntry> MealPlanEntries { get; set; } = [];
}
```

### Ingredient
```csharp
public class Ingredient
{
    public int Id { get; set; }
    public int RecipeId { get; set; }
    public Recipe Recipe { get; set; } = null!;
    public string Name { get; set; } = string.Empty;
    public string Quantity { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int CaloriesPerUnit { get; set; }
}
```

### MealPlanEntry
```csharp
public class MealPlanEntry
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public int MealThemeId { get; set; }
    public MealTheme MealTheme { get; set; } = null!;
    public int RecipeId { get; set; }
    public Recipe Recipe { get; set; } = null!;
    public int ServingsPlanned { get; set; } = 1;
}
```

---

## Service Interfaces

```csharp
// Cookbooks
public interface ICookbookService
{
    Task<IEnumerable<Cookbook>> GetAllAsync(string userId);
    Task<Cookbook?> GetByIdAsync(int id, string userId);
    Task<Cookbook> CreateAsync(string userId, string name, string description);
    Task UpdateAsync(int id, string userId, string name, string description);
    Task DeleteAsync(int id, string userId);
}

// Recipes
public interface IRecipeService
{
    Task<IEnumerable<Recipe>> GetByCookbookAsync(int cookbookId, string userId);
    Task<Recipe?> GetByIdAsync(int id, string userId);
    Task<Recipe> CreateAsync(Recipe recipe);
    Task UpdateAsync(Recipe recipe, string userId);
    Task DeleteAsync(int id, string userId);
}

// Themes
public interface IThemeService
{
    Task<IEnumerable<MealTheme>> GetAllAsync(string userId);
    Task<MealTheme> CreateAsync(string userId, string name, string icon, string colourHex);
    Task DeleteAsync(int id, string userId);
}

// Meal Plan
public interface IMealPlanService
{
    Task<IEnumerable<MealPlanEntry>> GetWeekAsync(string userId, DateOnly weekStart);
    Task<MealPlanEntry> AssignAsync(string userId, DateOnly date, int themeId, int recipeId, int servings);
    Task RemoveAsync(int entryId, string userId);
}

// Calories
public interface ICalorieService
{
    Task<DayCalorieSummary> GetDaySummaryAsync(string userId, DateOnly date, int targetCalories);
    Task<WeekCalorieSummary> GetWeekSummaryAsync(string userId, DateOnly weekStart, int targetCalories);
}

// AI
public interface IGptVisionService
{
    Task<RecipeDraft> ParseRecipeFromImageAsync(string imagePath);
}

// Storage
public interface IImageStorageService
{
    Task<string> SaveAsync(IFormFile file);
    void Delete(string path);
}
```

---

## GPT-4o Vision Prompt

```
You are a recipe extraction assistant.
The user has uploaded a screenshot of a recipe from TikTok or social media.
Extract the full recipe and return ONLY valid JSON — no markdown, no explanation, no code block.

Schema:
{
  "title": "string",
  "description": "string (1–2 sentences)",
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
```

---

## Recipe Import Flow

1. User opens `/cookbooks/{id}/recipes/import`
2. Drag-and-drop zone or file picker — accepts JPG, PNG, WEBP
3. JS previews the image client-side immediately
4. User clicks **Extract with AI**
5. POST to `/cookbooks/{id}/recipes/import` — multipart form
6. Server: `IImageStorageService.SaveAsync()` — writes to `wwwroot/uploads/{guid}.jpg`
7. Server: `IGptVisionService.ParseRecipeFromImageAsync(path)` — calls GPT-4o Vision
8. Returns `RecipeDraft` JSON
9. Review page rendered with pre-filled form — all fields editable
10. User confirms → POST to `/cookbooks/{id}/recipes/import/save`
11. Recipe saved to SQLite, redirect to recipe detail

---

## Calorie Tracking Logic

### Per-day
- Sum `Recipe.CaloriesPerServing × MealPlanEntry.ServingsPlanned` for all entries on that date
- Display total consumed vs user's daily target
- Show deficit/surplus

### Per-week
- Sum all 7 days
- Average daily calories for the week
- Visual indicator: under/over target

### Calendar display
Each day column on the meal planner shows:
```
Mon 12 May
[Breakfast] Pizza Chicken Wrap    420 kcal  [×]
[Dinner]    Beef Tacos            610 kcal  [×]
─────────────────────────────────────────────
Total: 1,030 / 2,000 kcal  ▓▓▓▓▓▓░░░░ 52%
```

---

## MealPlan Calendar UI

- Week view: Monday to Sunday
- Navigate back/forward a week (HTMX partial refresh)
- Each day column lists assigned meals by theme slot
- Slide-in recipe picker: search/filter recipes from any cookbook
- Drag a recipe card onto a day slot (vanilla JS drag-and-drop)
- HTMX posts the assignment, refreshes only that day column
- Calorie bar updates via HTMX partial

---

## Routes

### Auth
| Method | Route | Description |
|---|---|---|
| GET | `/login` | Login page |
| POST | `/login` | Authenticate |
| POST | `/logout` | Sign out |

### Cookbooks
| Method | Route | Description |
|---|---|---|
| GET | `/cookbooks` | List all cookbooks |
| GET | `/cookbooks/create` | Create form |
| POST | `/cookbooks/create` | Save new cookbook |
| GET | `/cookbooks/{id}` | Cookbook detail + recipe list |
| GET | `/cookbooks/{id}/edit` | Edit form |
| POST | `/cookbooks/{id}/edit` | Save changes |
| POST | `/cookbooks/{id}/delete` | Delete cookbook |

### Recipes (nested under cookbook)
| Method | Route | Description |
|---|---|---|
| GET | `/cookbooks/{id}/recipes/{rid}` | Recipe detail |
| GET | `/cookbooks/{id}/recipes/import` | Import page |
| POST | `/cookbooks/{id}/recipes/import` | Process image + GPT-4o |
| POST | `/cookbooks/{id}/recipes/import/save` | Save parsed recipe |
| GET | `/cookbooks/{id}/recipes/{rid}/edit` | Edit form |
| POST | `/cookbooks/{id}/recipes/{rid}/edit` | Save edits |
| POST | `/cookbooks/{id}/recipes/{rid}/delete` | Delete |

### Themes
| Method | Route | Description |
|---|---|---|
| GET | `/themes` | List + manage themes |
| POST | `/themes/create` | Add custom theme |
| POST | `/themes/{id}/delete` | Delete custom theme |

### Meal Plan
| Method | Route | Description |
|---|---|---|
| GET | `/mealplan` | Redirect to current week |
| GET | `/mealplan/{weekStart}` | Week calendar view |
| POST | `/mealplan/assign` | HTMX — assign recipe to slot |
| POST | `/mealplan/remove/{id}` | HTMX — remove entry |
| GET | `/mealplan/daycalories` | HTMX — refresh calorie bar |

---

## PWA

```json
{
  "name": "Digital Meal Planner",
  "short_name": "Meal Planner",
  "start_url": "/",
  "display": "standalone",
  "background_color": "#f7f2eb",
  "theme_color": "#e63946",
  "icons": [
    { "src": "/icons/icon-192.png", "sizes": "192x192", "type": "image/png" },
    { "src": "/icons/icon-512.png", "sizes": "512x512", "type": "image/png" }
  ]
}
```

Service worker caches: app shell, CSS, JS, previously viewed recipe pages.

---

## Testing Strategy

### Scope
- **Unit tests:** all service classes (`CookbookService`, `RecipeService`, `MealPlanService`, `CalorieService`)
- **Mock:** `IGptVisionService`, `IImageStorageService` — never hit real OpenAI in tests
- **DB:** EF Core InMemory provider via `TestDbContextFactory`
- **No controller tests** in Phase 1 — services are the contract

### Test naming convention
```
MethodName_StateUnderTest_ExpectedBehaviour
```

### Example tests
```csharp
// CookbookServiceTests
CreateAsync_ValidInput_ReturnsCookbookWithId()
CreateAsync_EmptyName_ThrowsArgumentException()
DeleteAsync_WrongUserId_ThrowsUnauthorizedAccessException()
GetAllAsync_UserHasNoCookbooks_ReturnsEmptyList()

// CalorieServiceTests
GetDaySummary_NoMeals_ReturnsZeroCalories()
GetDaySummary_TwoMeals_SumsCaloriesCorrectly()
GetWeekSummary_PartialWeek_AveragesCorrectly()
```

---

## Configuration

### appsettings.json
```json
{
  "OpenAI": {
    "ApiKey": "",
    "Model": "gpt-4o"
  },
  "Storage": {
    "UploadPath": "wwwroot/uploads"
  },
  "Calories": {
    "DefaultDailyTarget": 2000
  },
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=App_Data/mealplanner.db"
  }
}
```

API key from `appsettings.json` or env var `OpenAI__ApiKey`.

---

## Build Phases

### Stage 1 — Foundation ✅ BUILD NOW
- [ ] Solution + project scaffold
- [ ] Feature folder Razor view location convention
- [ ] EF Core + SQLite `AppDbContext` with all entities
- [ ] ASP.NET Core Identity with `ApplicationUser`
- [ ] `DatabaseSeeder` — robyn user + 4 default meal themes
- [ ] Shared layout (Bebas Neue + DM Sans, PoC colour palette)
- [ ] Login / logout flow
- [ ] Dashboard (home page, post-login)
- [ ] xUnit test project wired up, `TestDbContextFactory` helper

### Stage 2 — Cookbooks
- [ ] Full CRUD for cookbooks
- [ ] Cover image upload
- [ ] `CookbookService` + `ICookbookService`
- [ ] `CookbookServiceTests` (full suite)

### Stage 3 — Recipe Import
- [ ] Drag-and-drop image upload zone (JS)
- [ ] `IImageStorageService` + `ImageStorageService`
- [ ] `IGptVisionService` + `GptVisionService` (GPT-4o Vision)
- [ ] Review/edit form
- [ ] Recipe CRUD
- [ ] `RecipeServiceTests`

### Stage 4 — Themes
- [ ] Manage custom meal themes
- [ ] `ThemeService` + `IThemeService`
- [ ] Theme management UI

### Stage 5 — Meal Planner
- [ ] Weekly calendar view
- [ ] HTMX day slot partial refresh
- [ ] Recipe picker slide-in
- [ ] Drag-and-drop assign
- [ ] `MealPlanService` + `IMealPlanService`
- [ ] `MealPlanServiceTests`

### Stage 6 — Calorie Tracking
- [ ] Per-day calorie bar on calendar
- [ ] Weekly calorie summary
- [ ] `CalorieService` + `ICalorieService`
- [ ] `CalorieServiceTests`

### Stage 7 — Polish + PWA
- [ ] PWA manifest + service worker
- [ ] PDF cookbook export (QuestPDF)
- [ ] Mobile responsive pass
- [ ] Servings scaler on recipe detail
- [ ] Shopping list from week plan

---

## Seeded Data

| Field | Value |
|---|---|
| Username | `robyn` |
| Email | `robyn@mealplanner.local` |
| Password | `C00kb00k$Pl@nn3r!Mx9#` |
| Default themes | Breakfast 🌅, Lunch 🥗, Dinner 🍽️, Dessert 🍰 |
