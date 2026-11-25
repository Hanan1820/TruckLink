
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TruckLink.Models;

[Table("bookings")]
public class Booking
{
    
    public static readonly string[] STATUSES =
    {
        "Pending", "Accepted", "Rejected", "Picked Up", "In Transit", "In Storage",
        "Out for Delivery", "Completed", "Cancelled",
    };

    
    public static readonly string[] JOB_SEQUENCE = { "Accepted", "Picked Up", "In Transit", "In Storage", "Out for Delivery", "Completed" };

    
    public static readonly string[] ON_THE_ROAD = { "Picked Up", "In Transit", "Out for Delivery" };

    [Key, Column("booking_id")] public int BookingId { get; set; }
    [Column("customer_id")] public int CustomerId { get; set; }
    [Column("staff_id")] public int? StaffId { get; set; }          
    [Column("vehicle_id")] public int? VehicleId { get; set; }
    [Column("booking_status")] public string BookingStatus { get; set; } = "Pending";
    [Column("pickup_address")] public string PickupAddress { get; set; } = "";
    [Column("pickup_city")] public string PickupCity { get; set; } = "";
    [Column("dropoff_address")] public string DropoffAddress { get; set; } = "";
    [Column("dropoff_city")] public string DropoffCity { get; set; } = "";
    [Column("pickup_datetime")] public DateTime PickupDatetime { get; set; }
    [Column("total_volume_m3")] public decimal? TotalVolumeM3 { get; set; }
    [Column("recommended_vehicle_type")] public string? RecommendedVehicleType { get; set; }
    [Column("transport_fee")] public decimal? TransportFee { get; set; }
    [Column("total_amount")] public decimal? TotalAmount { get; set; }
    [Column("expected_delivery_datetime")] public DateTime? ExpectedDeliveryDatetime { get; set; }
    [Column("created_at")] public DateTime CreatedAt { get; set; }
    [Column("customer_notes")] public string? CustomerNotes { get; set; }
    [Column("rejection_reason")] public string? RejectionReason { get; set; }
    [Column("distance_km")] public decimal? DistanceKm { get; set; }

    public virtual Customer Customer { get; set; } = null!;            
    
    public virtual Staff? Driver { get; set; }                         
    public virtual Vehicle? Vehicle { get; set; }                      
    public virtual ICollection<Item> Items { get; set; } = new List<Item>();                          
    public virtual ICollection<BookingService> BookingServices { get; set; } = new List<BookingService>(); 
    
    public virtual ICollection<Service> Services { get; set; } = new List<Service>();
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();                 
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();                 
    public virtual StorageRequest? StorageRequest { get; set; }                                       
    public virtual Review? Review { get; set; }                                                       

    
    public static IQueryable<Booking> ScopePending(IQueryable<Booking> q) => q.Where(b => b.BookingStatus == "Pending");
    public static IQueryable<Booking> ScopeStatus(IQueryable<Booking> q, params string[] status) => q.Where(b => status.Contains(b.BookingStatus));

    
    [NotMapped]
    public string Reference => $"BK-{(CreatedAt == default ? DateTime.Now : CreatedAt):yyyy}-{BookingId:D6}";

    public bool IsPending() => BookingStatus == "Pending";

    
    public bool CanBeCancelled() =>
        BookingStatus == "Pending" || (BookingStatus == "Accepted" && DateTime.Now < PickupDatetime);

    
    public bool CanBeUpdated() => BookingStatus is "Pending" or "Accepted";

    
    
    public bool CanBeReviewed() => BookingStatus == "Completed" && Review == null;

    
    
    public string? NextJobStatus()
    {
        var i = Array.IndexOf(JOB_SEQUENCE, BookingStatus);
        if (i < 0 || i == JOB_SEQUENCE.Length - 1) return null;
        var next = JOB_SEQUENCE[i + 1];

        
        if (next == "In Storage" && StorageRequest == null) next = "Out for Delivery";

        return next;
    }
}
