
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using TruckLink.Models.Concerns;
using TruckLink.Support.Auth;
using TruckLink.Support.Rules;

namespace TruckLink.Models;

[Table("staff")]
public class Staff : Authenticatable, IHasPersonName, IRemembersWithoutColumn
{
    public static readonly string[] TITLES = { "Driver", "Mover", "Warehouse Keeper" };

    public static readonly string[] STATUSES = { "Active", "On Leave", "Inactive" };

    
    public static readonly Dictionary<string, string[]> STATUS_ICONS = new()
    {
        ["Active"] = new[] { "bi-check-circle", "--ok" },
        ["On Leave"] = new[] { "bi-calendar-x", "--wn" },
        ["Inactive"] = new[] { "bi-x-circle", "--dg" },
    };

    
    public static readonly string[] SPECIALTIES = { "assembly", "packing", "loading" };

    
    public static readonly Dictionary<string, string> TITLE_ICONS = new()
    {
        ["Driver"] = "bi-truck", ["Mover"] = "bi-box-seam", ["Warehouse Keeper"] = "bi-buildings",
    };

    [Key, Column("staff_id")] public int StaffId { get; set; }
    [Column("admin_id")] public int AdminId { get; set; }                       
    [Column("first_name")] public string FirstName { get; set; } = "";
    [Column("last_name")] public string LastName { get; set; } = "";

    private string _email = "";
    [Column("email")] public string Email { get => _email; set => _email = value?.Trim().ToLowerInvariant() ?? ""; }

    [Column("password"), JsonIgnore] public string Password { get; set; } = ""; 

    private string _phone = "";
    [Column("phone")] public string Phone { get => _phone; set => _phone = TruckLinkPhone.Canonical(value); }

    [Column("job_title")] public string JobTitle { get; set; } = "";
    [Column("license_number")] public string? LicenseNumber { get; set; }
    [Column("hire_date")] public DateTime HireDate { get; set; }                
    [Column("staff_status")] public string StaffStatus { get; set; } = "Active";
    [Column("specialty")] public string? Specialty { get; set; }
    [Column("shift_start")] public TimeSpan? ShiftStart { get; set; }           
    [Column("shift_end")] public TimeSpan? ShiftEnd { get; set; }

    public virtual Admin Admin { get; set; } = null!;                           
    
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    
    public virtual ICollection<Warehouse> Warehouses { get; set; } = new List<Warehouse>();
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
    public virtual ICollection<Announcement> Announcements { get; set; } = new List<Announcement>();

    [NotMapped, JsonIgnore] public override int AuthIdentifier => StaffId;
    [NotMapped, JsonIgnore] public override string AuthPassword => Password;
    [NotMapped, JsonIgnore] public override string Guard => "staff";

    [NotMapped] public string FullName => ((IHasPersonName)this).FullName;
    [NotMapped] public string Initials => ((IHasPersonName)this).Initials;
    [NotMapped] public string PhoneDisplay => ((IHasPersonName)this).PhoneDisplay;
    [NotMapped] public string PhoneLocal => ((IHasPersonName)this).PhoneLocal;

    
    public static IQueryable<Staff> ScopeActive(IQueryable<Staff> q) => q.Where(s => s.StaffStatus == "Active");
    public static IQueryable<Staff> ScopeTitled(IQueryable<Staff> q, string title) => q.Where(s => s.JobTitle == title);

    
    public bool CanSignIn() => StaffStatus != "Inactive";

    public bool IsKeeper() => JobTitle == "Warehouse Keeper";

    
    [NotMapped] public string? ShiftStartLabel => ShiftStart?.ToString(@"hh\:mm");
    [NotMapped] public string? ShiftEndLabel => ShiftEnd?.ToString(@"hh\:mm");

    
    public bool IsOnShiftAt(DateTime moment)
    {
        if (ShiftStart == null || ShiftEnd == null) return true;
        var t = moment.ToString("HH:mm");
        return string.CompareOrdinal(t, ShiftStartLabel) >= 0 && string.CompareOrdinal(t, ShiftEndLabel) < 0;
    }

    
    [NotMapped] public string Code => $"STF-{StaffId:D3}";
}
