using Microsoft.AspNetCore.Mvc;
using AgroSphere.Data;

namespace AgroSphere.Controllers;

public class AccountController : Controller
{
    private readonly AppDbContext _db;
    public AccountController(AppDbContext db) => _db = db;

    public IActionResult Login() => View();

    [HttpPost]
    public IActionResult Login(string username, string password)
    {
        var user = _db.Users.FirstOrDefault(u => u.Username == username && u.Password == password);
        if (user == null)
        {
            ViewBag.Error = "Invalid credentials";
            return View();
        }
        HttpContext.Session.SetInt32("UserId", user.Id);
        HttpContext.Session.SetString("UserName", user.FullName);
        HttpContext.Session.SetString("Role", user.Role);
        return RedirectToAction("Index", "Home");
    }

    public IActionResult Register() => View();

    [HttpPost]
    public IActionResult Register(string username, string password, string fullname)
    {
        if (_db.Users.Any(u => u.Username == username))
        {
            ViewBag.Error = "Username taken";
            return View();
        }
        var user = new Models.User { Username = username, Password = password, FullName = fullname };
        _db.Users.Add(user);
        _db.SaveChanges();
        return RedirectToAction("Login");
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }
}
