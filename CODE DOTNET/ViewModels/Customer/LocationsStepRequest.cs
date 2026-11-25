
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using TruckLink.Support.Rules;

namespace TruckLink.ViewModels.Customer;


public class LocationsStepRequest : Request
{
    [FromForm(Name = "pickup_address"), Display(Name = "Pickup address")]
    [Required, StringLength(200)]
    public string PickupAddress { get; set; } = "";

    [FromForm(Name = "pickup_city"), Display(Name = "Pickup city")]
    [Required(ErrorMessage = "Pickup city is required"), InConfig("TruckLink:Cities")]   
    public string PickupCity { get; set; } = "";

    [FromForm(Name = "dropoff_address"), Display(Name = "Drop-off address")]
    [Required, StringLength(200)]
    public string DropoffAddress { get; set; } = "";

    [FromForm(Name = "dropoff_city"), Display(Name = "Drop-off city")]
    [Required(ErrorMessage = "Drop-off city is required"), InConfig("TruckLink:Cities")]
    public string DropoffCity { get; set; } = "";

    
    [FromForm(Name = "pickup_date"), Display(Name = "Pickup date")]
    [Required, DateFormat("yyyy-MM-dd")]
    public string PickupDate { get; set; } = "";

    [FromForm(Name = "pickup_time"), Display(Name = "Pickup time")]
    [Required, DateFormat("HH:mm")]
    public string PickupTime { get; set; } = "";

    [FromForm(Name = "customer_notes"), Display(Name = "Notes for the crew")]
    [StringLength(240)]
    public string? CustomerNotes { get; set; }

    
    
    [FromForm(Name = "pickup_lat")] [Range(16, 33)] public double? PickupLat { get; set; }
    [FromForm(Name = "pickup_lng")] [Range(34, 56)] public double? PickupLng { get; set; }
    [FromForm(Name = "dropoff_lat")] [Range(16, 33)] public double? DropoffLat { get; set; }
    [FromForm(Name = "dropoff_lng")] [Range(34, 56)] public double? DropoffLng { get; set; }

    public override IEnumerable<ValidationResult> Validate(ValidationContext ctx)
    {
        
        var dateOk = DateTime.TryParseExact(PickupDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d);
        if (dateOk && d.Date < DateTime.Today) yield return Fail("pickup_date", "The Pickup date cannot be in the past.");

        
        var timeOk = TimeSpan.TryParseExact(PickupTime, @"hh\:mm", CultureInfo.InvariantCulture, out var t);
        if (dateOk && timeOk && !(d.Date < DateTime.Today) && d.Date.Add(t) < DateTime.Now.AddHours(2))
            yield return Fail("pickup_time", "Pickup must be at least 2 hours from now.");
    }
}
