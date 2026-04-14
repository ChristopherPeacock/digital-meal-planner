using DigitalMealPlanner.Tests.Helpers;
using DigitalMealPlanner.Web.Infrastructure.Data.Entities;
using DigitalMealPlanner.Web.Modules.Cookbooks;
using Xunit;

namespace DigitalMealPlanner.Tests.Cookbooks;

public class CookbookServiceTests
{
    private const string UserId = "user-123";
    private const string OtherUserId = "other-456";

    private static CookbookService CreateService() =>
        new(TestDbContextFactory.Create());

    // --- CreateAsync ---

    [Fact]
    public async Task CreateAsync_ValidInput_ReturnsCookbookWithId()
    {
        var svc = CreateService();
        var result = await svc.CreateAsync(UserId, "My Cookbook", "A description");
        Assert.True(result.Id > 0);
        Assert.Equal("My Cookbook", result.Name);
        Assert.Equal(UserId, result.UserId);
    }

    [Fact]
    public async Task CreateAsync_EmptyName_ThrowsArgumentException()
    {
        var svc = CreateService();
        await Assert.ThrowsAsync<ArgumentException>(() =>
            svc.CreateAsync(UserId, "   ", "desc"));
    }

    [Fact]
    public async Task CreateAsync_TrimsName()
    {
        var svc = CreateService();
        var result = await svc.CreateAsync(UserId, "  My Cookbook  ", "");
        Assert.Equal("My Cookbook", result.Name);
    }

    // --- GetAllAsync ---

    [Fact]
    public async Task GetAllAsync_UserHasNoCookbooks_ReturnsEmptyList()
    {
        var svc = CreateService();
        var result = await svc.GetAllAsync(UserId);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllAsync_OnlyReturnsCurrentUsersBooks()
    {
        var db = TestDbContextFactory.Create();
        db.Cookbooks.AddRange(
            new Cookbook { UserId = UserId,      Name = "Mine",    CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Cookbook { UserId = OtherUserId, Name = "Not Mine", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        );
        await db.SaveChangesAsync();

        var svc = new CookbookService(db);
        var result = await svc.GetAllAsync(UserId);

        Assert.Single(result);
        Assert.Equal("Mine", result.First().Name);
    }

    // --- GetByIdAsync ---

    [Fact]
    public async Task GetByIdAsync_WrongUser_ReturnsNull()
    {
        var db = TestDbContextFactory.Create();
        db.Cookbooks.Add(new Cookbook { UserId = OtherUserId, Name = "Other", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();
        var id = db.Cookbooks.First().Id;

        var svc = new CookbookService(db);
        var result = await svc.GetByIdAsync(id, UserId);
        Assert.Null(result);
    }

    // --- DeleteAsync ---

    [Fact]
    public async Task DeleteAsync_WrongUserId_ThrowsUnauthorizedAccessException()
    {
        var db = TestDbContextFactory.Create();
        db.Cookbooks.Add(new Cookbook { UserId = OtherUserId, Name = "Other", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();
        var id = db.Cookbooks.First().Id;

        var svc = new CookbookService(db);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            svc.DeleteAsync(id, UserId));
    }

    [Fact]
    public async Task DeleteAsync_ValidOwner_RemovesCookbook()
    {
        var db = TestDbContextFactory.Create();
        db.Cookbooks.Add(new Cookbook { UserId = UserId, Name = "Mine", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();
        var id = db.Cookbooks.First().Id;

        var svc = new CookbookService(db);
        await svc.DeleteAsync(id, UserId);

        Assert.Empty(db.Cookbooks);
    }

    // --- UpdateAsync ---

    [Fact]
    public async Task UpdateAsync_ValidOwner_UpdatesFields()
    {
        var db = TestDbContextFactory.Create();
        db.Cookbooks.Add(new Cookbook { UserId = UserId, Name = "Old", Description = "Old desc", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();
        var id = db.Cookbooks.First().Id;

        var svc = new CookbookService(db);
        await svc.UpdateAsync(id, UserId, "New Name", "New desc");

        var updated = await svc.GetByIdAsync(id, UserId);
        Assert.Equal("New Name", updated!.Name);
        Assert.Equal("New desc", updated.Description);
    }

    [Fact]
    public async Task UpdateAsync_EmptyName_ThrowsArgumentException()
    {
        var db = TestDbContextFactory.Create();
        db.Cookbooks.Add(new Cookbook { UserId = UserId, Name = "Mine", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();
        var id = db.Cookbooks.First().Id;

        var svc = new CookbookService(db);
        await Assert.ThrowsAsync<ArgumentException>(() =>
            svc.UpdateAsync(id, UserId, "", ""));
    }
}
