
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using TruckLink.Models.Concerns;
using TruckLink.Support.Auth;
using TruckLink.Support.Rules;

namespace TruckLink.Models;

[Table("admins")]
public class Admin : Authenticatable, IHasPersonName, IRemembersWithoutColumn
{
    public static readonly string[] ROLES = { "Super Admin", "Operations Admin" };

    
    [Key, Column("admin_id")] public int AdminId { get; set; }

    
    [Column("first_name")] public string FirstName { get; set; } = "";
    [Column("last_name")] public string LastName { get; set; } = "";

    private string _email = "";
    
    [Column("email")] public string Email { get => _email; set => _email = value?.Trim().ToLowerInvariant() ?? ""; }

    
    [Column("password"), JsonIgnore] public string Password { get; set; } = "";

    private string _phone = "";
    
    [Column("phone")] public string Phone { get => _phone; set => _phone = TruckLinkPhone.Canonical(value); }

    [Column("admin_role")] public string AdminRole { get; set; } = "Operations Admin";

    
    [Column("created_at")] public DateTime CreatedAt { get; set; }

    
    public virtual ICollection<Staff> Staff { get; set; } = new List<Staff>();                         
    public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();                  
    public virtual ICollection<ItemCategory> ItemCategories { get; set; } = new List<ItemCategory>(); 
    public virtual ICollection<Service> Services { get; set; } = new List<Service>();                  
    public virtual ICollection<Review> ModeratedReviews { get; set; } = new List<Review>();            
    public virtual ICollection<Announcement> Announcements { get; set; } = new List<Announcement>();   

    [NotMapped, JsonIgnore] public override int AuthIdentifier => AdminId;
    [NotMapped, JsonIgnore] public override string AuthPassword => Password;
    [NotMapped, JsonIgnore] public override string Guard => "admin";

    
    [NotMapped] public string FullName => ((IHasPersonName)this).FullName;
    [NotMapped] public string Initials => ((IHasPersonName)this).Initials;
    [NotMapped] public string PhoneDisplay => ((IHasPersonName)this).PhoneDisplay;
    [NotMapped] public string PhoneLocal => ((IHasPersonName)this).PhoneLocal;

    
    [NotMapped] public string Code => $"ADM-{AdminId:D3}";

    public bool IsSuperAdmin() => AdminRole == "Super Admin";
}
