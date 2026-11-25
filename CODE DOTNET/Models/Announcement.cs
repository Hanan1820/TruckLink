
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TruckLink.Models;





[Table("announcements")]
public class Announcement
{
    public static readonly string[] TYPES =
    {
        "Booking Accepted", "Booking Rejected", "Status Update", "Payment Confirmed",
        "Storage Reminder", "Job Assigned", "Service Update", "Offer",
    };

    [Key, Column("announcement_id")] public int AnnouncementId { get; set; }
    [Column("admin_id")] public int? AdminId { get; set; }
    [Column("customer_id")] public int? CustomerId { get; set; }
    [Column("staff_id")] public int? StaffId { get; set; }
    [Column("announcement_type")] public string AnnouncementType { get; set; } = "";
    [Column("title")] public string Title { get; set; } = "";
    [Column("content")] public string Content { get; set; } = "";
    [Column("sent_at")] public DateTime SentAt { get; set; }
    [Column("is_read")] public bool IsRead { get; set; }

    public virtual Admin? Admin { get; set; }         
    public virtual Customer? Customer { get; set; }   
    public virtual Staff? Staff { get; set; }         

    
    public static IQueryable<Announcement> ScopeUnread(IQueryable<Announcement> q) => q.Where(a => !a.IsRead);
}
