using Microsoft.AspNetCore.Mvc;
using AgroSphere.Data;
using AgroSphere.Models;

namespace AgroSphere.Controllers;

public class FarmController : Controller
{
    private readonly AppDbContext _db;
    public FarmController(AppDbContext db) => _db = db;

    private int? Uid => HttpContext.Session.GetInt32("UserId");

    public IActionResult Index()
    {
        if (Uid == null) return RedirectToAction("Login", "Account");
        ViewBag.Farms = _db.Farms.Where(f => f.UserId == Uid).ToList();
        return View();
    }

    [HttpPost]
    public IActionResult AddFarm(string name, string location, double area, string farmType)
    {
        if (Uid == null) return RedirectToAction("Login", "Account");
        _db.Farms.Add(new Farm { UserId = Uid.Value, Name = name, Location = location, AreaAcres = area, FarmType = farmType });
        _db.SaveChanges();
        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult DeleteFarm(int id)
    {
        var f = _db.Farms.Find(id);
        if (f != null && f.UserId == Uid) { _db.Farms.Remove(f); _db.SaveChanges(); }
        return RedirectToAction("Index");
    }

    public IActionResult Crops(int farmId)
    {
        if (Uid == null) return RedirectToAction("Login", "Account");
        ViewBag.FarmId = farmId;
        ViewBag.FarmName = _db.Farms.Find(farmId)?.Name ?? "Farm";
        ViewBag.Crops = _db.Crops.Where(c => c.FarmId == farmId).ToList();
        return View();
    }

    [HttpPost]
    public IActionResult AddCrop(int farmId, string name, string season, double areaSown)
    {
        _db.Crops.Add(new Crop { FarmId = farmId, Name = name, Season = season, AreaSown = areaSown });
        _db.SaveChanges();
        return RedirectToAction("Crops", new { farmId });
    }

    [HttpPost]
    public IActionResult DeleteCrop(int id, int farmId)
    {
        var c = _db.Crops.Find(id);
        if (c != null) { _db.Crops.Remove(c); _db.SaveChanges(); }
        return RedirectToAction("Crops", new { farmId });
    }

    public IActionResult Inventory(int farmId)
    {
        if (Uid == null) return RedirectToAction("Login", "Account");
        ViewBag.FarmId = farmId;
        ViewBag.FarmName = _db.Farms.Find(farmId)?.Name ?? "Farm";
        ViewBag.Items = _db.InventoryItems.Where(i => i.FarmId == farmId).ToList();
        return View();
    }

    [HttpPost]
    public IActionResult AddInventory(int farmId, string itemName, string category, int quantity, string unit)
    {
        _db.InventoryItems.Add(new InventoryItem { FarmId = farmId, ItemName = itemName, Category = category, Quantity = quantity, Unit = unit });
        _db.SaveChanges();
        return RedirectToAction("Inventory", new { farmId });
    }

    [HttpPost]
    public IActionResult DeleteInventory(int id, int farmId)
    {
        var item = _db.InventoryItems.Find(id);
        if (item != null) { _db.InventoryItems.Remove(item); _db.SaveChanges(); }
        return RedirectToAction("Inventory", new { farmId });
    }

    [HttpPost]
    public IActionResult ResellToMarket(int id, int farmId, double pricePerKg)
    {
        if (Uid == null) return RedirectToAction("Login", "Account");
        var item = _db.InventoryItems.Find(id);
        var farm = _db.Farms.Find(farmId);
        if (item != null && farm != null && farm.UserId == Uid)
        {
            var price = pricePerKg > 0 ? pricePerKg : 45.0;
            // 1. Add to Marketplace listings
            _db.Listings.Add(new Listing
            {
                UserId = Uid.Value,
                CropName = item.ItemName.Replace(" (Purchased)", ""),
                Quantity = item.Quantity,
                PricePerUnit = price,
                FarmName = farm.Name,
                Unit = item.Unit ?? "kg"
            });

            // 2. Remove / deduct from inventory
            _db.InventoryItems.Remove(item);
            _db.SaveChanges();

            return RedirectToAction("Index", "Market");
        }
        return RedirectToAction("Inventory", new { farmId });
    }
}
