using Microsoft.AspNetCore.Mvc;
using AgroSphere.Data;
using AgroSphere.Models;

namespace AgroSphere.Controllers;

public class MarketController : Controller
{
    private readonly AppDbContext _db;
    public MarketController(AppDbContext db) => _db = db;

    private int? Uid => HttpContext.Session.GetInt32("UserId");

    public IActionResult Index()
    {
        if (Uid == null) return RedirectToAction("Login", "Account");
        ViewBag.Listings = _db.Listings.OrderByDescending(l => l.PostedOn).ToList();
        ViewBag.UserId = Uid;
        return View();
    }

    [HttpPost]
    public IActionResult AddListing(string cropName, double quantity, double pricePerKg, string farmName)
    {
        if (Uid == null) return RedirectToAction("Login", "Account");
        _db.Listings.Add(new Listing {
            UserId = Uid.Value,
            CropName = cropName,
            Quantity = quantity,
            PricePerUnit = pricePerKg,
            FarmName = string.IsNullOrWhiteSpace(farmName) ? "Local Farm" : farmName,
            Unit = "kg"
        });
        _db.SaveChanges();
        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult Buy(int listingId, double qty = 10)
    {
        if (Uid == null) return RedirectToAction("Login", "Account");
        var listing = _db.Listings.Find(listingId);
        if (listing == null) return RedirectToAction("Index");

        var actualQty = qty > 0 ? qty : listing.Quantity;
        var totalAmount = actualQty * listing.PricePerUnit;
        var batch = "AGRO-" + DateTime.Now.ToString("yyyyMMdd") + "-" + new Random().Next(1000, 9999);

        // 1. Create the Order with Estimated Delivery
        _db.Orders.Add(new Order
        {
            ListingId = listingId,
            BuyerId = Uid.Value,
            CropName = listing.CropName,
            Quantity = actualQty,
            TotalPrice = totalAmount,
            BatchCode = batch,
            Status = "Confirmed",
            DeliveryAddress = "District Farm Station, Sector 4",
            EstimatedDelivery = DateTime.Now.AddDays(2),
            DeliveryPartner = "AgroExpress Logistics",
            TrackingStatus = "Order Placed & Dispatched"
        });

        // 2. Interconnection: Add to Buyer's Farm Inventory automatically
        var userFarm = _db.Farms.FirstOrDefault(f => f.UserId == Uid.Value);
        if (userFarm != null)
        {
            _db.InventoryItems.Add(new InventoryItem
            {
                FarmId = userFarm.Id,
                ItemName = listing.CropName + " (Purchased)",
                Category = "Produce",
                Quantity = (int)Math.Round(actualQty),
                Unit = "kg"
            });
        }

        // 3. Interconnection: Record Expense in Buyer's Financial Ledger
        _db.Transactions.Add(new Transaction
        {
            UserId = Uid.Value,
            Type = "Expense",
            Description = "Purchase: " + listing.CropName + " (" + actualQty + "kg)",
            Amount = totalAmount,
            Category = "Marketplace",
            Date = DateTime.Today
        });

        // 4. Interconnection: If seller is a platform user, record Income in Seller's Ledger
        if (listing.UserId != Uid.Value)
        {
            _db.Transactions.Add(new Transaction
            {
                UserId = listing.UserId,
                Type = "Income",
                Description = "Sale: " + listing.CropName + " (" + actualQty + "kg)",
                Amount = totalAmount,
                Category = "Sales",
                Date = DateTime.Today
            });
        }

        _db.SaveChanges();
        return RedirectToAction("Orders");
    }

    [HttpPost]
    public IActionResult CancelOrder(int id)
    {
        var o = _db.Orders.Find(id);
        if (o != null && o.BuyerId == Uid && o.Status != "Cancelled")
        {
            o.Status = "Cancelled";
            o.TrackingStatus = "Order Cancelled by Buyer";

            // Interconnection: Refund in Financial Ledger
            _db.Transactions.Add(new Transaction
            {
                UserId = Uid.Value,
                Type = "Income",
                Description = "Refund: Cancelled Order #" + o.Id + " (" + o.CropName + ")",
                Amount = o.TotalPrice,
                Category = "Refund",
                Date = DateTime.Today
            });

            _db.SaveChanges();
        }
        return RedirectToAction("Orders");
    }

    [HttpPost]
    public IActionResult UpdateOrder(int id, string status)
    {
        var o = _db.Orders.Find(id);
        if (o != null) { o.Status = status; _db.SaveChanges(); }
        return RedirectToAction("Orders");
    }

    [HttpPost]
    public IActionResult DeleteListing(int id)
    {
        var l = _db.Listings.Find(id);
        if (l != null && l.UserId == Uid) { _db.Listings.Remove(l); _db.SaveChanges(); }
        return RedirectToAction("Index");
    }

    public IActionResult Orders()
    {
        if (Uid == null) return RedirectToAction("Login", "Account");
        ViewBag.Orders = _db.Orders.Where(o => o.BuyerId == Uid).OrderByDescending(o => o.OrderDate).ToList();
        return View();
    }

    [HttpPost]
    public IActionResult UpdateOrderStatus(int id, string status)
    {
        var o = _db.Orders.Find(id);
        if (o != null) { o.Status = status; _db.SaveChanges(); }
        return RedirectToAction("Orders");
    }

    public IActionResult Trace(string batchId)
    {
        if (Uid == null) return RedirectToAction("Login", "Account");
        ViewBag.BatchId = batchId ?? "";
        if (!string.IsNullOrEmpty(batchId))
        {
            var ord = _db.Orders.FirstOrDefault(o => o.BatchCode == batchId);
            ViewBag.TraceResult = new
            {
                BatchId = batchId,
                Crop = ord != null ? ord.CropName : "Wheat (Organic)",
                Farm = "Samarth Organic Farm #1",
                HarvestDate = "15 Oct 2026",
                PackDate = "18 Oct 2026",
                Grade = "Grade A+",
                Status = ord != null ? ord.Status : "Verified Origin"
            };
        }
        else
        {
            ViewBag.TraceResult = null;
        }
        return View();
    }
}
