
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TruckLink.Models;

[Table("item_categories")]
public class ItemCategory
{
    [Key, Column("category_id")] public int CategoryId { get; set; }
    [Column("admin_id")] public int AdminId { get; set; }
    [Column("category_name")] public string CategoryName { get; set; } = "";
    [Column("description")] public string? Description { get; set; }
    [Column("requires_temperature_control")] public bool RequiresTemperatureControl { get; set; }
    [Column("recommended_vehicle_type")] public string RecommendedVehicleType { get; set; } = "";

    public virtual Admin Admin { get; set; } = null!;                              
    public virtual ICollection<Item> Items { get; set; } = new List<Item>();       

    
    [NotMapped]
    public string ShortName => new Dictionary<int, string>
    {
        [1] = "Furniture", [2] = "Appliances", [3] = "Temperature-sensitive", [4] = "Personal Items", [5] = "Heavy items",
    }.GetValueOrDefault(CategoryId, CategoryName);

    
    [NotMapped] public string Code => $"CAT-{CategoryId:D3}";
}
