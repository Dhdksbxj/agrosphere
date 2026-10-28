using Microsoft.AspNetCore.Mvc;
using AgroSphere.Data;
using AgroSphere.Models;

namespace AgroSphere.Controllers;

public class FinanceController : Controller
{
    private readonly AppDbContext _db;
    public FinanceController(AppDbContext db) => _db = db;

    private int? Uid => HttpContext.Session.GetInt32("UserId");

    public IActionResult Index()
    {
        if (Uid == null) return RedirectToAction("Login", "Account");
        var txns = _db.Transactions.Where(t => t.UserId == Uid).OrderByDescending(t => t.Date).ToList();
        ViewBag.Transactions = txns;
        ViewBag.Income = (decimal)txns.Where(t => t.Type == "Income").Sum(t => t.Amount);
        ViewBag.Expense = (decimal)txns.Where(t => t.Type == "Expense").Sum(t => t.Amount);
        return View();
    }

    [HttpPost]
    public IActionResult AddTransaction(string type, string description, double amount, string category)
    {
        if (Uid == null) return RedirectToAction("Login", "Account");
        _db.Transactions.Add(new Transaction { UserId = Uid.Value, Type = type, Description = description, Amount = amount, Category = category });
        _db.SaveChanges();
        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult DeleteTransaction(int id)
    {
        var t = _db.Transactions.Find(id);
        if (t != null && t.UserId == Uid) { _db.Transactions.Remove(t); _db.SaveChanges(); }
        return RedirectToAction("Index");
    }

    public IActionResult Forecast()
    {
        if (Uid == null) return RedirectToAction("Login", "Account");
        ViewBag.Forecasts = new List<dynamic>
        {
            new { Crop = "Wheat", Price = 28.5, Trend = "up", Change = "+4.2%" },
            new { Crop = "Basmati Rice", Price = 62.0, Trend = "up", Change = "+1.8%" },
            new { Crop = "Cotton", Price = 74.0, Trend = "down", Change = "-2.5%" },
            new { Crop = "Mustard", Price = 54.5, Trend = "up", Change = "+3.1%" },
            new { Crop = "Potato", Price = 16.0, Trend = "down", Change = "-5.0%" },
            new { Crop = "Soybean", Price = 48.0, Trend = "up", Change = "+2.4%" }
        };
        return View();
    }

    public IActionResult Subsidies()
    {
        if (Uid == null) return RedirectToAction("Login", "Account");
        ViewBag.Subsidies = new List<dynamic>
        {
            new { Name = "PM-KISAN Samman Nidhi", Description = "Direct income support of ₹6,000 per year in 3 equal installments to small and marginal farmers.", Eligibility = "All landholding farmer families with cultivable land.", Amount = "₹6,000/year" },
            new { Name = "Sub-Mission on Agricultural Mechanization (SMAM)", Description = "Financial assistance for purchasing tractors, power tillers, and harvesting machinery.", Eligibility = "Individual farmers, FPOs, and cooperative societies.", Amount = "40% to 50% subsidy" },
            new { Name = "Pradhan Mantri Krishi Sinchayee Yojana (PMKSY)", Description = "Subsidy for installing micro-irrigation systems like drip and sprinkler setups.", Eligibility = "Farmers with assured water source and land ownership.", Amount = "Up to 55% subsidy" },
            new { Name = "Paramparagat Krishi Vikas Yojana (PKVY)", Description = "Financial support for adopting organic farming, certification, and eco-friendly manure.", Eligibility = "Farmers forming clusters of 50+ acres for organic farming.", Amount = "₹50,000/hectare" }
        };
        return View();
    }
}
