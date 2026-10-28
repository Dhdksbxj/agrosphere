using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AgroSphere.Data;
using AgroSphere.Models;

namespace AgroSphere.Controllers;

public class CommunityController : Controller
{
    private readonly AppDbContext _db;
    public CommunityController(AppDbContext db) => _db = db;

    private int? Uid => HttpContext.Session.GetInt32("UserId");

    public IActionResult Index()
    {
        if (Uid == null) return RedirectToAction("Login", "Account");
        ViewBag.Posts = _db.ForumPosts
            .Include(p => p.Replies)
            .OrderByDescending(p => p.PostedOn)
            .ToList();
        return View();
    }

    [HttpPost]
    public IActionResult AddPost(string title, string content)
    {
        if (Uid == null) return RedirectToAction("Login", "Account");
        var name = HttpContext.Session.GetString("UserName") ?? "User";
        _db.ForumPosts.Add(new ForumPost { UserId = Uid.Value, AuthorName = name, Title = title, Content = content });
        _db.SaveChanges();
        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult AddReply(int postId, string content)
    {
        if (Uid == null) return RedirectToAction("Login", "Account");
        var name = HttpContext.Session.GetString("UserName") ?? "User";
        if (!string.IsNullOrWhiteSpace(content))
        {
            _db.ForumReplies.Add(new ForumReply
            {
                ForumPostId = postId,
                UserId = Uid.Value,
                AuthorName = name,
                Content = content,
                RepliedOn = DateTime.Now
            });
            _db.SaveChanges();
        }
        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult Like(int id)
    {
        var p = _db.ForumPosts.Find(id);
        if (p != null) { p.Likes++; _db.SaveChanges(); }
        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult DeletePost(int id)
    {
        var p = _db.ForumPosts.Find(id);
        if (p != null && p.UserId == Uid) { _db.ForumPosts.Remove(p); _db.SaveChanges(); }
        return RedirectToAction("Index");
    }
}
