
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using TruckLink.Models.Concerns;
using TruckLink.Support.Auth;
using TruckLink.Support.Rules;

namespace TruckLink.Models;

[Table("customers")]
public class Customer : Authenticatable, IHasPersonName, IRemembersWithoutColumn
{
    public static readonly string[] STATUSES = { "Active", "Suspended", "Blocked" };

    
    public static readonly Dictionary<string, string[]> STATUS_ICONS = new()
    {
        ["Active"] = new[] { "bi-check-circle", "--ok" },
        ["Suspended"] = new[] { "bi-pause-circle", "--wn" },
        ["Blocked"] = new[] { "bi-slash-circle", "--dg" },
    };

    public static readonly string[] CARD_TYPES = { "Mada", "Visa", "Mastercard" };

    [Key, Column("customer_id")] public int CustomerId { get; set; }

    [Column("first_name")] public string FirstName { get; set; } = "";
    [Column("last_name")] public string LastName { get; set; } = "";

    private string _email = "";
    [Column("email")] public string Email { get => _email; set => _email = value?.Trim().ToLowerInvariant() ?? ""; }

    [Column("password"), JsonIgnore] public string Password { get; set; } = ""; 

    private string _phone = "";
    [Column("phone")] public string Phone { get => _phone; set => _phone = TruckLinkPhone.Canonical(value); }

    [Column("address")] public string? Address { get; set; }
    [Column("city")] public string City { get; set; } = "";

    
    [Column("card_holder_name")] public string? CardHolderName { get; set; }
    [Column("card_number")] public string? CardNumber { get; set; }
    [Column("card_type")] public string? CardType { get; set; }
    [Column("expiry_date")] public string? ExpiryDate { get; set; }
    [Column("account_status")] public string AccountStatus { get; set; } = "Active";
    [Column("created_at")] public DateTime CreatedAt { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();                
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();                   
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();                
    public virtual ICollection<Announcement> Announcements { get; set; } = new List<Announcement>(); 

    [NotMapped, JsonIgnore] public override int AuthIdentifier => CustomerId;
    [NotMapped, JsonIgnore] public override string AuthPassword => Password;
    [NotMapped, JsonIgnore] public override string Guard => "customer";

    [NotMapped] public string FullName => ((IHasPersonName)this).FullName;
    [NotMapped] public string Initials => ((IHasPersonName)this).Initials;
    [NotMapped] public string PhoneDisplay => ((IHasPersonName)this).PhoneDisplay;
    [NotMapped] public string PhoneLocal => ((IHasPersonName)this).PhoneLocal;

    
    public bool IsActive() => AccountStatus == "Active";

    public bool HasSavedCard() => CardNumber != null;

    
    [NotMapped] public string Code => $"CUS-{CustomerId:D4}";
}
