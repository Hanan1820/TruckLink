
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using TruckLink.Support.Rules;

namespace TruckLink.ViewModels.Customer;


public class CorrectItemRequest : Request
{
    [FromForm(Name = "uid")]
    [Required]
    public string Uid { get; set; } = "";

    
    [FromForm(Name = "estimated_length_cm"), Display(Name = "Length")]
    [Required, GreaterThan(0), Range(0, 9999.99)]
    public decimal? EstimatedLengthCm { get; set; }

    [FromForm(Name = "estimated_width_cm"), Display(Name = "Width")]
    [Required, GreaterThan(0), Range(0, 9999.99)]
    public decimal? EstimatedWidthCm { get; set; }

    [FromForm(Name = "estimated_height_cm"), Display(Name = "Height")]
    [Required, GreaterThan(0), Range(0, 9999.99)]
    public decimal? EstimatedHeightCm { get; set; }

    [FromForm(Name = "estimated_weight_kg"), Display(Name = "Weight")]
    [Required, GreaterThan(0), Range(0, 40000)]
    public decimal? EstimatedWeightKg { get; set; }
}
