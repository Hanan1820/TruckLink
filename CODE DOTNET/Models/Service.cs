
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TruckLink.Models;


[Table("services")]
public class Service
{
    public static readonly string[] TYPES = { "Transport", "Storage", "Value-Added" };

    public static readonly string[] UNITS = { "per job", "per item", "per worker", "per day", "per week", "per month", "first 7 days" };

    
    public static readonly Dictionary<string, string> ICONS = new()
    {
        ["Assembly & Disassembly"] = "bi-tools",
        ["Packing & Protection"] = "bi-box2-fill",
        ["Extra Loading Labor"] = "bi-person-arms-up",
    };

    
    public static readonly Dictionary<string, string> ILLUSTRATIONS = new()
    {
        ["Single Move"] = "Svg/SvcSingleMove",
        ["Multi-Stop Move"] = "Svg/SvcMultiStop",
        ["Temperature-Controlled"] = "Svg/SvcTemperatureControlled",
        ["Full Home / Office Move"] = "Svg/SvcFullHomeOffice",
    };

    [Key, Column("service_id")] public int ServiceId { get; set; }
    [Column("admin_id")] public int AdminId { get; set; }
    [Column("service_name")] public string ServiceName { get; set; } = "";
    [Column("service_type")] public string ServiceType { get; set; } = "";
    [Column("description")] public string? Description { get; set; }
    [Column("base_price")] public decimal BasePrice { get; set; }      
    [Column("pricing_unit")] public string PricingUnit { get; set; } = "";
    [Column("is_active")] public bool IsActive { get; set; } = true;

    public virtual Admin Admin { get; set; } = null!;                                   

    
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public virtual ICollection<BookingService> BookingServices { get; set; } = new List<BookingService>(); 

    
    public static IQueryable<Service> ScopeActive(IQueryable<Service> q) => q.Where(s => s.IsActive);
    public static IQueryable<Service> ScopeOfType(IQueryable<Service> q, string type) => q.Where(s => s.ServiceType == type);

    
    public static IQueryable<Service> ScopeStorageExtension(IQueryable<Service> q, string extensionType)
    {
        var unit = new Dictionary<string, string> { ["Daily"] = "per day", ["Weekly"] = "per week", ["Monthly"] = "per month" }[extensionType];
        return q.Where(s => s.ServiceType == "Storage" && s.PricingUnit == unit);
    }

    
    [NotMapped] public string Code => $"SRV-{ServiceId:D3}";
}
