using Microsoft.AspNetCore.Mvc;
using AgroSphere.Data;

namespace AgroSphere.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _db;
    public HomeController(AppDbContext db) => _db = db;

    public IActionResult Index()
    {
        var uid = HttpContext.Session.GetInt32("UserId");
        if (uid == null) return RedirectToAction("Login", "Account");

        ViewBag.UserName = HttpContext.Session.GetString("UserName");
        ViewBag.FarmCount = _db.Farms.Count(f => f.UserId == uid);
        ViewBag.CropCount = _db.Crops.Count();
        ViewBag.ListingCount = _db.Listings.Count();
        ViewBag.PostCount = _db.ForumPosts.Count();
        return View();
    }
}
