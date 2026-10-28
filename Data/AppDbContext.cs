using Microsoft.EntityFrameworkCore;
using AgroSphere.Models;

namespace AgroSphere.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Farm> Farms => Set<Farm>();
    public DbSet<Crop> Crops => Set<Crop>();
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<Listing> Listings => Set<Listing>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<ForumPost> ForumPosts => Set<ForumPost>();
    public DbSet<ForumReply> ForumReplies => Set<ForumReply>();
}

public static class SeedData
{
    public static void Initialize(AppDbContext db)
    {
        if (db.Users.Any()) return;

        var u1 = new User { Username = "samarth", Password = "pass123", FullName = "Samarth Amin", Role = "Admin" };
        var u2 = new User { Username = "panth", Password = "pass123", FullName = "Panth Dholakiya", Role = "Farmer" };
        db.Users.AddRange(u1, u2);
        db.SaveChanges();

        db.Farms.AddRange(
            new Farm { UserId = u1.Id, Name = "Green Valley Farm", Location = "Ahmedabad, Gujarat", AreaAcres = 25, FarmType = "Organic" },
            new Farm { UserId = u2.Id, Name = "Sunrise Fields", Location = "Rajkot, Gujarat", AreaAcres = 40, FarmType = "Mixed" }
        );
        db.SaveChanges();

        var f1 = db.Farms.First();
        db.Crops.AddRange(
            new Crop { FarmId = f1.Id, Name = "Wheat", Season = "Rabi", AreaSown = 10, Status = "Growing" },
            new Crop { FarmId = f1.Id, Name = "Cotton", Season = "Kharif", AreaSown = 8, Status = "Harvested" }
        );

        db.InventoryItems.AddRange(
            new InventoryItem { FarmId = f1.Id, ItemName = "Urea Fertilizer", Category = "Fertilizer", Quantity = 50, Unit = "kg" },
            new InventoryItem { FarmId = f1.Id, ItemName = "Wheat Seeds", Category = "Seed", Quantity = 200, Unit = "kg" }
        );

        db.Listings.Add(new Listing { UserId = u1.Id, CropName = "Wheat", Quantity = 500, PricePerUnit = 25, Unit = "kg" });

        db.Transactions.AddRange(
            new Transaction { UserId = u1.Id, Type = "Income", Description = "Wheat sale", Amount = 12500, Category = "Sales" },
            new Transaction { UserId = u1.Id, Type = "Expense", Description = "Fertilizer purchase", Amount = 3000, Category = "Input" }
        );

        db.ForumPosts.Add(new ForumPost { UserId = u1.Id, AuthorName = "Samarth", Title = "Best practices for organic wheat", Content = "Sharing my experience with organic wheat farming methods." });

        db.SaveChanges();
    }
}
