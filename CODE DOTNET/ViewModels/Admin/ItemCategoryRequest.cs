
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using TruckLink.Support.Rules;

namespace TruckLink.ViewModels.Admin;


public class ItemCategoryRequest : Request
{
    
    [BindNever] public int? IgnoreCategoryId { get; set; }

    [FromForm(Name = "category_name"), Display(Name = "Category name")]
    [Required, StringLength(60)]
    [Unique("item_categories", "category_name", "category_id", IgnoreIdProperty = nameof(IgnoreCategoryId), Message = "Another category already has this name.")]
    public string CategoryName { get; set; } = "";

    [FromForm(Name = "description"), Display(Name = "Description")]
    [StringLength(255)]
    public string? Description { get; set; }

    [FromForm(Name = "requires_temperature_control"), Display(Name = "Temperature control")]
    public bool RequiresTemperatureControl { get; set; }

    [FromForm(Name = "recommended_vehicle_type"), Display(Name = "Matched vehicle type")]
    [Required, In("Small Pickup", "Medium Truck", "Large Freight", "Temp-Controlled")]   
    public string RecommendedVehicleType { get; set; } = "";

    public override IEnumerable<ValidationResult> Validate(ValidationContext ctx)
    {
        if (RequiresTemperatureControl && RecommendedVehicleType != "Temp-Controlled")
            yield return Fail("recommended_vehicle_type", "Temperature-sensitive items must be matched with a Temp-Controlled vehicle.");
    }

    public Dictionary<string, object?> Category() => new()
    {
        ["category_name"] = CategoryName.Trim(),
        ["description"] = Filled(Description) ? Description!.Trim() : null,
        ["requires_temperature_control"] = RequiresTemperatureControl,
        ["recommended_vehicle_type"] = RecommendedVehicleType,
    };
}
