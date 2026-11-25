
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TruckLink.Models;


[Table("storage_requests")]
public class StorageRequest
{
    public static readonly string[] EXTENSIONS = { "Daily", "Weekly", "Monthly" };

    public static readonly string[] STATUSES = { "Reserved", "Free Period", "Extended", "Released" };

    [Key, Column("storage_id")] public int StorageId { get; set; }
    [Column("booking_id")] public int BookingId { get; set; }
    [Column("warehouse_id")] public int WarehouseId { get; set; }
    [Column("storage_location")] public string StorageLocation { get; set; } = "";
    [Column("start_date", TypeName = "date")] public DateTime StartDate { get; set; }
    [Column("free_period_end_date", TypeName = "date")] public DateTime FreePeriodEndDate { get; set; }
    [Column("end_date", TypeName = "date")] public DateTime? EndDate { get; set; }
    [Column("extension_type")] public string? ExtensionType { get; set; }
    [Column("extension_units")] public int? ExtensionUnits { get; set; }
    [Column("storage_fee")] public decimal StorageFee { get; set; }
    [Column("storage_status")] public string StorageStatus { get; set; } = "Reserved";

    public virtual Booking Booking { get; set; } = null!;       
    public virtual Warehouse Warehouse { get; set; } = null!;   
}
