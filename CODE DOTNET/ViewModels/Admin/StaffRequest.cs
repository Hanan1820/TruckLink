
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using TruckLink.Support.Auth;
using TruckLink.Support.Rules;

namespace TruckLink.ViewModels.Admin;


public class StaffRequest : AccountRequest
{
    
    public override Authenticatable? Record { get; set; }

    [FromForm(Name = "job_title"), Display(Name = "Job title")]
    [Required, In("Driver", "Mover", "Warehouse Keeper")]       
    public string JobTitle { get; set; } = "";

    
    [FromForm(Name = "license_number"), Display(Name = "Licence number")]
    [StringLength(30)]
    public string? LicenseNumber { get; set; }

    [FromForm(Name = "specialty"), Display(Name = "Specialty")]
    [In("assembly", "packing", "loading")]                      
    public string? Specialty { get; set; }

    [FromForm(Name = "hire_date"), Display(Name = "Hire date")]
    [Required, RelativeToToday(RelativeToTodayAttribute.Mode.BeforeOrEqual)]   
    public DateTime? HireDate { get; set; }

    [FromForm(Name = "staff_status"), Display(Name = "Status")]
    [Required, In("Active", "On Leave", "Inactive")]            
    public string StaffStatus { get; set; } = "";

    
    [FromForm(Name = "shift_start"), Display(Name = "Shift start")]
    [DateFormat("HH:mm")]
    public string? ShiftStart { get; set; }

    
    [FromForm(Name = "shift_end"), Display(Name = "Shift end")]
    [DateFormat("HH:mm")]
    public string? ShiftEnd { get; set; }

    public override IEnumerable<ValidationResult> Validate(ValidationContext ctx)
    {
        foreach (var r in base.Validate(ctx)) yield return r;
        if (JobTitle == "Driver" && !Filled(LicenseNumber)) yield return Fail("license_number", "A driver needs a licence number.");
        if (Filled(ShiftEnd) && !Filled(ShiftStart)) yield return Fail("shift_start", "The Shift start field is required when Shift end is present.");
        if (Filled(ShiftStart) && !Filled(ShiftEnd)) yield return Fail("shift_end", "The Shift end field is required when Shift start is present.");
        if (Filled(ShiftStart) && Filled(ShiftEnd) && string.CompareOrdinal(ShiftEnd, ShiftStart) <= 0)
            yield return Fail("shift_end", "The shift must end after it starts.");
    }

    
    public Dictionary<string, object?> Staff()
    {
        var data = Account();
        data["job_title"] = JobTitle;
        data["license_number"] = JobTitle == "Driver" ? LicenseNumber!.Trim().ToUpperInvariant() : null;
        data["specialty"] = JobTitle == "Mover" ? Specialty : null;
        data["hire_date"] = HireDate;
        data["staff_status"] = StaffStatus;
        data["shift_start"] = Filled(ShiftStart) ? ShiftStart : null;
        data["shift_end"] = Filled(ShiftEnd) ? ShiftEnd : null;
        return data;
    }
}
