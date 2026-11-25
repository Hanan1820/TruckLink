
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using TruckLink.Support.Helpers;

namespace TruckLink.Models;

[Table("vehicles")]
public class Vehicle
{
    
    public static readonly string[] TYPES = { "Small Pickup", "Medium Truck", "Large Freight", "Temp-Controlled" };

    public static readonly string[] STATUSES = { "Available", "On Job", "Maintenance", "Out of Service" };

    
    public static readonly Dictionary<string, string> ICONS = new()
    {
        ["Small Pickup"] = "bi-truck-front",
        ["Medium Truck"] = "bi-truck",
        ["Large Freight"] = "bi-truck-flatbed",
        ["Temp-Controlled"] = "bi-snow",
    };

    
    public static readonly Dictionary<string, decimal> DEFAULT_CAPACITY = new()
    {
        ["Small Pickup"] = 3.5m,
        ["Medium Truck"] = 12m,
        ["Large Freight"] = 34m,
        ["Temp-Controlled"] = 9.6m,
    };

    
    public static readonly Dictionary<string, string[]> STATUS_ICONS = new()
    {
        ["Available"] = new[] { "bi-check-circle", "--ok" },
        ["On Job"] = new[] { "bi-geo-alt", "--in" },
        ["Maintenance"] = new[] { "bi-tools", "--wn" },
        ["Out of Service"] = new[] { "bi-x-circle", "--dg" },
    };

    
    public static readonly Dictionary<string, string> BODIES = new()
    {
        ["Small Pickup"] = "Open bed",
        ["Medium Truck"] = "Closed body",
        ["Large Freight"] = "Closed body",
        ["Temp-Controlled"] = "Refrigerated body",
    };

    
    public static readonly Dictionary<string, string> ILLUSTRATIONS = new()
    {
        ["Small Pickup"] = "Svg/FleetSmallPickup",
        ["Medium Truck"] = "Svg/FleetMediumTruck",
        ["Large Freight"] = "Svg/FleetLargeFreight",
        ["Temp-Controlled"] = "Svg/FleetTempControlled",
    };

    [Key, Column("vehicle_id")] public int VehicleId { get; set; }
    [Column("admin_id")] public int AdminId { get; set; }

    private string _plateNumber = "";
    
    [Column("plate_number")]
    public string PlateNumber
    {
        get => _plateNumber;
        set => _plateNumber = value == null ? "" : Regex.Replace(value, @"\s+", " ").Trim().ToUpperInvariant();
    }

    [Column("vehicle_type")] public string VehicleType { get; set; } = "";
    [Column("capacity_volume_m3")] public decimal CapacityVolumeM3 { get; set; }  
    [Column("max_weight_kg")] public decimal MaxWeightKg { get; set; }            
    [Column("is_temperature_controlled")] public bool IsTemperatureControlled { get; set; }
    [Column("vehicle_status")] public string VehicleStatus { get; set; } = "Available";

    
    [Column("created_at")] public DateTime CreatedAt { get; set; }
    [Column("updated_at")] public DateTime UpdatedAt { get; set; }

    public virtual Admin Admin { get; set; } = null!;                                 
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>(); 

    
    public static IQueryable<Vehicle> ScopeAvailable(IQueryable<Vehicle> q) => q.Where(v => v.VehicleStatus == "Available");

    
    [NotMapped] public string Code => $"VEH-{VehicleId:D4}";

    [NotMapped] public string Icon => ICONS.GetValueOrDefault(VehicleType, "bi-truck");

    
    public static string StatusIcon(string? status, bool filled = false)
    {
        var icon = status != null && STATUS_ICONS.TryGetValue(status, out var s) ? s[0] : "bi-circle";
        return filled && icon != "bi-tools" ? icon + "-fill" : icon;
    }

    
    public static Dictionary<string, Dictionary<string, object?>> TypeOptions() =>
        TYPES.ToDictionary(t => t, t => new Dictionary<string, object?>
        {
            ["label"] = t, ["icon"] = ICONS[t], ["meta"] = Helpers.Num(DEFAULT_CAPACITY[t]) + " m³",
        });

    
    public static Dictionary<string, Dictionary<string, object?>> StatusOptions() =>
        STATUSES.ToDictionary(s => s, s => new Dictionary<string, object?>
        {
            ["label"] = s, ["icon"] = STATUS_ICONS[s][0], ["colour"] = STATUS_ICONS[s][1], ["boxIcon"] = s != "Available",
        });
}
