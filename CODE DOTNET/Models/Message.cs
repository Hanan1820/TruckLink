
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TruckLink.Models;


[Table("messages")]
public class Message
{
    public static readonly string[] SENDERS = { "Customer", "Staff" };

    [Key, Column("message_id")] public int MessageId { get; set; }
    [Column("booking_id")] public int BookingId { get; set; }
    [Column("customer_id")] public int CustomerId { get; set; }
    [Column("staff_id")] public int StaffId { get; set; }
    [Column("sender_type")] public string SenderType { get; set; } = "";
    [Column("message_text")] public string MessageText { get; set; } = "";
    [Column("sent_at")] public DateTime SentAt { get; set; }
    [Column("is_read")] public bool IsRead { get; set; }

    public virtual Booking Booking { get; set; } = null!;     
    public virtual Customer Customer { get; set; } = null!;   
    public virtual Staff Staff { get; set; } = null!;         
}
