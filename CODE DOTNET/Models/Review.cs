
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TruckLink.Models;


[Table("reviews")]
public class Review
{
    [Key, Column("review_id")] public int ReviewId { get; set; }
    [Column("booking_id")] public int BookingId { get; set; }
    [Column("customer_id")] public int CustomerId { get; set; }
    [Column("admin_id")] public int? AdminId { get; set; }          
    [Column("rating")] public int Rating { get; set; }              
    [Column("comment")] public string? Comment { get; set; }
    [Column("created_at")] public DateTime CreatedAt { get; set; }

    public virtual Booking Booking { get; set; } = null!;           
    public virtual Customer Customer { get; set; } = null!;         
    public virtual Admin? Moderator { get; set; }                   
}
