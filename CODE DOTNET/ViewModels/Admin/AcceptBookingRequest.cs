
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using TruckLink.Support.Rules;

namespace TruckLink.ViewModels.Admin;





public class AcceptBookingRequest : Request
{
    [FromForm(Name = "vehicle_id"), Display(Name = "Vehicle")]
    [Required(ErrorMessage = "Choose the vehicle for this job.")]
    [Exists("vehicles", "vehicle_id")]                                  
    public int? VehicleId { get; set; }

    [FromForm(Name = "driver_id"), Display(Name = "Driver")]
    [Required(ErrorMessage = "Choose the driver for this job.")]
    [Exists("staff", "staff_id", WhereColumn = "job_title", WhereValue = "Driver")]   
    public int? DriverId { get; set; }

    
    [FromForm(Name = "crew"), Display(Name = "Moving crew")]
    [MaxLength(4, ErrorMessage = "A moving crew has at most 4 members.")]
    public List<int>? Crew { get; set; }

    
    public override IEnumerable<ValidationResult> Validate(ValidationContext ctx)
    {
        if (Crew == null) yield break;
        if (Crew.Distinct().Count() != Crew.Count)
            yield return Fail("crew", "The crew field has a duplicate value.");
        var exists = new ExistsAttribute("staff", "staff_id") { WhereColumn = "job_title", WhereValue = "Mover", WhereColumn2 = "staff_status", WhereValue2 = "Active" };
        foreach (var id in Crew)
        {
            var r = exists.GetValidationResult(id, new ValidationContext(this, ctx, null) { MemberName = "crew", DisplayName = "Moving crew" });
            if (r != ValidationResult.Success) yield return Fail("crew", r!.ErrorMessage!);
        }
    }
}
