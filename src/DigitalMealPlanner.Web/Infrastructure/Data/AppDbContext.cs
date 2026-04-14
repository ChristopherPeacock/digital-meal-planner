using DigitalMealPlanner.Web.Infrastructure.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DigitalMealPlanner.Web.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Cookbook> Cookbooks => Set<Cookbook>();
    public DbSet<Recipe> Recipes => Set<Recipe>();
    public DbSet<Ingredient> Ingredients => Set<Ingredient>();
    public DbSet<MealTheme> MealThemes => Set<MealTheme>();
    public DbSet<MealPlanEntry> MealPlanEntries => Set<MealPlanEntry>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Cookbook>(e =>
        {
            e.HasOne(c => c.User)
             .WithMany(u => u.Cookbooks)
             .HasForeignKey(c => c.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(c => c.Recipes)
             .WithOne(r => r.Cookbook)
             .HasForeignKey(r => r.CookbookId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Recipe>(e =>
        {
            e.HasMany(r => r.Ingredients)
             .WithOne(i => i.Recipe)
             .HasForeignKey(i => i.RecipeId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<MealTheme>(e =>
        {
            e.HasOne(t => t.User)
             .WithMany(u => u.MealThemes)
             .HasForeignKey(t => t.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<MealPlanEntry>(e =>
        {
            e.HasOne(m => m.User)
             .WithMany(u => u.MealPlanEntries)
             .HasForeignKey(m => m.UserId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(m => m.Recipe)
             .WithMany(r => r.MealPlanEntries)
             .HasForeignKey(m => m.RecipeId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(m => m.MealTheme)
             .WithMany(t => t.MealPlanEntries)
             .HasForeignKey(m => m.MealThemeId)
             .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
