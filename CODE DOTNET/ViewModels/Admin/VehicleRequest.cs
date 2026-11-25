
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using TruckLink.Support.Rules;

namespace TruckLink.ViewModels.Admin;


public class VehicleRequest : Request
{
    public const string PLATE_REGEX = "^[0-9]{1,4} [A-Z]{3}$";

    
    [BindNever] public int? IgnoreVehicleId { get; set; }

    private string _plate = "";

    
    [FromForm(Name = "plate_number"), Display(Name = "Plate number")]
    [Required, StringLength(20)]
    [RegularExpression(PLATE_REGEX, ErrorMessage = "Use the Saudi plate format: up to 4 digits, a space and 3 letters (e.g. 4821 TRK).")]
    [Unique("vehicles", "plate_number", "vehicle_id", IgnoreIdProperty = nameof(IgnoreVehicleId), Message = "Another vehicle already has this plate number.")]
    public string PlateNumber
    {
        get => _plate;
        set => _plate = Regex.Replace(value ?? "", @"\s+", " ").Trim().ToUpperInvariant();   
    }

    [FromForm(Name = "vehicle_type"), Display(Name = "Vehicle type")]
    [Required, In("Small Pickup", "Medium Truck", "Large Freight", "Temp-Controlled")]       
    public string VehicleType { get; set; } = "";

    [FromForm(Name = "capacity_volume_m3"), Display(Name = "Capacity volume")]
    [Required, GreaterThan(0, ErrorMessage = "Capacity volume must be greater than zero."), Range(0, 9999.99)]
    public decimal? CapacityVolumeM3 { get; set; }

    [FromForm(Name = "max_weight_kg"), Display(Name = "Maximum weight")]
    [Required, Range(100, 40000, ErrorMessage = "Maximum weight must be between 100 and 40,000 kg.")]
    public decimal? MaxWeightKg { get; set; }

    [FromForm(Name = "is_temperature_controlled"), Display(Name = "Temperature controlled")]
    [Required]
    public bool? IsTemperatureControlled { get; set; }

    [FromForm(Name = "vehicle_status"), Display(Name = "Vehicle status")]
    [Required, In("Available", "On Job", "Maintenance", "Out of Service")]                   
    public string VehicleStatus { get; set; } = "";

    public override IEnumerable<ValidationResult> Validate(ValidationContext ctx)
    {
        
        if (IsTemperatureControlled != null && IsTemperatureControlled.Value != (VehicleType == "Temp-Controlled"))
            yield return Fail("is_temperature_controlled", "Only Temp-Controlled vehicles are refrigerated.");
    }
}
