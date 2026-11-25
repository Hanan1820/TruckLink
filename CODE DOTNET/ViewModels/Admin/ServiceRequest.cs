
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using TruckLink.Support.Rules;

namespace TruckLink.ViewModels.Admin;


public class ServiceRequest : Request
{
    
    [BindNever] public int? IgnoreServiceId { get; set; }

    [FromForm(Name = "service_name"), Display(Name = "Service name")]
    [Required, StringLength(60)]
    [Unique("services", "service_name", "service_id", IgnoreIdProperty = nameof(IgnoreServiceId), Message = "Another service already has this name.")]
    public string ServiceName { get; set; } = "";

    [FromForm(Name = "service_type"), Display(Name = "Service type")]
    [Required, In("Transport", "Storage", "Value-Added")]                     
    public string ServiceType { get; set; } = "";

    [FromForm(Name = "base_price"), Display(Name = "Base price")]
    [Required, Range(0, 99999.99)]                                           
    public decimal? BasePrice { get; set; }

    [FromForm(Name = "pricing_unit"), Display(Name = "Pricing unit")]
    [Required, In("per job", "per item", "per worker", "per day", "per week", "per month", "first 7 days")]   
    public string PricingUnit { get; set; } = "";

    [FromForm(Name = "description"), Display(Name = "Description")]
    [StringLength(255)]
    public string? Description { get; set; }

    [FromForm(Name = "is_active"), Display(Name = "Availability")]
    [Required]
    public bool? IsActive { get; set; }

    public Dictionary<string, object?> Service() => new()
    {
        ["service_name"] = ServiceName.Trim(),
        ["service_type"] = ServiceType,
        ["base_price"] = BasePrice,
        ["pricing_unit"] = PricingUnit,
        ["description"] = Filled(Description) ? Description!.Trim() : null,
        ["is_active"] = IsActive,
    };
}
