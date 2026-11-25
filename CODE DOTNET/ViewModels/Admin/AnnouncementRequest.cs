
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using TruckLink.Support.Rules;

namespace TruckLink.ViewModels.Admin;


public class AnnouncementRequest : Request
{
    public static readonly string[] AUDIENCES = { "All customers", "All staff", "One customer", "One staff member" };

    
    public static readonly string[] TYPES = { "Service Update", "Offer" };

    [FromForm(Name = "title"), Display(Name = "Title")]
    [Required, StringLength(100)]
    public string Title { get; set; } = "";

    [FromForm(Name = "content"), Display(Name = "Message")]
    [Required, StringLength(500)]
    public string Content { get; set; } = "";

    [FromForm(Name = "announcement_type"), Display(Name = "Type")]
    [Required, In("Service Update", "Offer")]
    public string AnnouncementType { get; set; } = "";

    [FromForm(Name = "audience"), Display(Name = "Send to")]
    [Required, In("All customers", "All staff", "One customer", "One staff member")]
    public string Audience { get; set; } = "";

    
    [FromForm(Name = "customer_id"), Display(Name = "Customer")]
    [Exists("customers", "customer_id")]
    public int? CustomerId { get; set; }

    
    [FromForm(Name = "staff_id"), Display(Name = "Staff member")]
    [Exists("staff", "staff_id")]
    public int? StaffId { get; set; }

    public override IEnumerable<ValidationResult> Validate(ValidationContext ctx)
    {
        if (Audience == "One customer" && CustomerId == null) yield return Fail("customer_id", "Choose the customer to send it to.");
        if (Audience == "One staff member" && StaffId == null) yield return Fail("staff_id", "Choose the staff member to send it to.");
    }
}
