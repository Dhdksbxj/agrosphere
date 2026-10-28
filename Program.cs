using Microsoft.EntityFrameworkCore;
using AgroSphere.Data;

var builder = WebApplication.CreateBuilder(args);

var dbPath = Path.Combine(builder.Environment.ContentRootPath, "agrosphere.db");

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite($"Data Source={dbPath}"));
builder.Services.AddSession(opt => {
    opt.IdleTimeout = TimeSpan.FromMinutes(30);
    opt.Cookie.HttpOnly = true;
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    void EnsureColumn(string table, string column, string typeDefinition)
    {
        try
        {
            using var cmd = db.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = $"PRAGMA table_info(\"{table}\");";
            db.Database.OpenConnection();
            using var reader = cmd.ExecuteReader();
            var exists = false;
            while (reader.Read())
            {
                if (string.Equals(reader["name"]?.ToString(), column, StringComparison.OrdinalIgnoreCase))
                {
                    exists = true;
                    break;
                }
            }
            reader.Close();
            if (!exists)
            {
                db.Database.ExecuteSqlRaw($"ALTER TABLE \"{table}\" ADD COLUMN \"{column}\" {typeDefinition};");
            }
        }
        catch { }
    }

    db.Database.ExecuteSqlRaw(@"
        CREATE TABLE IF NOT EXISTS ""ForumReplies"" (
            ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_ForumReplies"" PRIMARY KEY AUTOINCREMENT,
            ""ForumPostId"" INTEGER NOT NULL,
            ""UserId"" INTEGER NOT NULL,
            ""AuthorName"" TEXT NOT NULL,
            ""Content"" TEXT NOT NULL,
            ""PostedOn"" TEXT NOT NULL,
            ""RepliedOn"" TEXT DEFAULT CURRENT_TIMESTAMP,
            CONSTRAINT ""FK_ForumReplies_ForumPosts_ForumPostId"" FOREIGN KEY (""ForumPostId"") REFERENCES ""ForumPosts"" (""Id"") ON DELETE CASCADE
        );
    ");

    EnsureColumn("ForumReplies", "PostedOn", "TEXT DEFAULT CURRENT_TIMESTAMP");
    EnsureColumn("ForumReplies", "RepliedOn", "TEXT DEFAULT CURRENT_TIMESTAMP");
    EnsureColumn("Orders", "DeliveryAddress", "TEXT DEFAULT 'Gujarat Agro District Hub'");
    EnsureColumn("Orders", "EstimatedDelivery", "TEXT DEFAULT CURRENT_TIMESTAMP");
    EnsureColumn("Orders", "DeliveryPartner", "TEXT DEFAULT 'AgroExpress Logistics'");
    EnsureColumn("Orders", "TrackingStatus", "TEXT DEFAULT 'Dispatched from Farm Hub'");

    SeedData.Initialize(db);
}

app.UseStaticFiles();
app.UseRouting();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
