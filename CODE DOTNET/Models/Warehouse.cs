
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TruckLink.Models;

[Table("warehouses")]
public class Warehouse
{
    [Key, Column("warehouse_id")] public int WarehouseId { get; set; }
    [Column("keeper_id")] public int KeeperId { get; set; }                  
    [Column("warehouse_name")] public string WarehouseName { get; set; } = "";
    [Column("address")] public string Address { get; set; } = "";
    [Column("city")] public string City { get; set; } = "";
    [Column("total_capacity_m3")] public decimal TotalCapacityM3 { get; set; } 
    [Column("used_capacity_m3")] public decimal UsedCapacityM3 { get; set; }   
    [Column("opening_hours")] public string OpeningHours { get; set; } = "";

    public virtual Staff Keeper { get; set; } = null!;                                               
    public virtual ICollection<StorageRequest> StorageRequests { get; set; } = new List<StorageRequest>(); 

    
    [NotMapped]
    public int OccupancyPercent => TotalCapacityM3 > 0
        ? (int)Math.Round(UsedCapacityM3 / TotalCapacityM3 * 100, MidpointRounding.AwayFromZero)
        : 0;
}
