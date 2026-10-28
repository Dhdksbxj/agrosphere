using System.ComponentModel.DataAnnotations;

namespace AgroSphere.Models;

public class User
{
    public int Id { get; set; }

    [Required] public string Username { get; set; } = "";
    [Required] public string Password { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Role { get; set; } = "Farmer";
}

public class Farm
{
    public int Id { get; set; }
    public int UserId { get; set; }

    [Required] public string Name { get; set; } = "";
    public string Location { get; set; } = "";
    public double AreaAcres { get; set; }
    public string FarmType { get; set; } = "Mixed";
    public string Status { get; set; } = "Active";
}

public class Crop
{
    public int Id { get; set; }
    public int FarmId { get; set; }

    [Required] public string Name { get; set; } = "";
    public string Season { get; set; } = "";
    public double AreaSown { get; set; }
    public DateTime SowDate { get; set; } = DateTime.Today;
    public string Status { get; set; } = "Growing";
}

public class InventoryItem
{
    public int Id { get; set; }
    public int FarmId { get; set; }

    [Required] public string ItemName { get; set; } = "";
    public string Category { get; set; } = "Seed";
    public int Quantity { get; set; }
    public string Unit { get; set; } = "kg";
}

public class Listing
{
    public int Id { get; set; }
    public int UserId { get; set; }

    [Required] public string CropName { get; set; } = "";
    public double Quantity { get; set; }
    public double PricePerUnit { get; set; }
    public double PricePerKg { get => PricePerUnit; set => PricePerUnit = value; }
    public string FarmName { get; set; } = "Organic Farm";
    public string Unit { get; set; } = "kg";
    public string Status { get; set; } = "Available";
    public DateTime PostedOn { get; set; } = DateTime.Now;
}

public class Order
{
    public int Id { get; set; }
    public int ListingId { get; set; }
    public int BuyerId { get; set; }

    public string CropName { get; set; } = "Produce";
    public double Quantity { get; set; }
    public double TotalPrice { get; set; }
    public string Status { get; set; } = "Confirmed";
    public string BatchCode { get; set; } = "";
    public DateTime OrderDate { get; set; } = DateTime.Now;

    // Delivery tracking details
    public string DeliveryAddress { get; set; } = "Gujarat Agro District Hub";
    public DateTime EstimatedDelivery { get; set; } = DateTime.Now.AddDays(3);
    public string DeliveryPartner { get; set; } = "AgroExpress Logistics";
    public string TrackingStatus { get; set; } = "Dispatched from Farm Hub";
}

public class Transaction
{
    public int Id { get; set; }
    public int UserId { get; set; }

    public string Type { get; set; } = "Income";
    [Required] public string Description { get; set; } = "";
    public double Amount { get; set; }
    public string Category { get; set; } = "Sales";
    public DateTime Date { get; set; } = DateTime.Today;
}

public class ForumPost
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string AuthorName { get; set; } = "";
    public string Author { get => AuthorName; set => AuthorName = value; }

    [Required] public string Title { get; set; } = "";
    public string Content { get; set; } = "";
    public int Likes { get; set; }
    public DateTime PostedOn { get; set; } = DateTime.Now;
    public DateTime CreatedAt { get => PostedOn; set => PostedOn = value; }

    public List<ForumReply> Replies { get; set; } = new();
}

public class ForumReply
{
    public int Id { get; set; }
    public int ForumPostId { get; set; }
    public int UserId { get; set; }
    public string AuthorName { get; set; } = "";
    [Required] public string Content { get; set; } = "";
    public DateTime PostedOn { get; set; } = DateTime.Now;
    public DateTime RepliedOn { get => PostedOn; set => PostedOn = value; }
}
