
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TruckLink.Models;


[Table("booking_services")]
public class BookingService
{
    [Key, Column("booking_service_id")] public int BookingServiceId { get; set; }
    [Column("booking_id")] public int BookingId { get; set; }     
    [Column("service_id")] public int ServiceId { get; set; }     
    [Column("quantity")] public int Quantity { get; set; } = 1;   
    [Column("subtotal")] public decimal Subtotal { get; set; }    

    public virtual Booking Booking { get; set; } = null!;         
    public virtual Service Service { get; set; } = null!;         
}
