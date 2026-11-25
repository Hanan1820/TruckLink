
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace TruckLink.ViewModels.Customer;


public class ExtrasStepRequest : Request
{
    
    public class ServiceLine
    {
        [FromForm(Name = "selected")] public bool? Selected { get; set; }     
        [FromForm(Name = "quantity")] public int? Quantity { get; set; }      
    }

    [FromForm(Name = "services")]
    public Dictionary<int, ServiceLine> Services { get; set; } = new();

    [FromForm(Name = "storage")]
    public bool? Storage { get; set; }

    
    [FromForm(Name = "end_date"), Display(Name = "Planned release")]
    public string? EndDate { get; set; }

    
    [BindNever] public string? DraftPickupDate { get; set; }

    public override IEnumerable<ValidationResult> Validate(ValidationContext ctx)
    {
        foreach (var (id, line) in Services)
            if (line.Quantity is < 0 or > 20)
                yield return Fail($"services.{id}.quantity", "Choose a quantity between 0 and 20.");

        if (Storage != true) yield break;     
        if (string.IsNullOrWhiteSpace(EndDate)) { yield return Fail("end_date", "The Planned release field is required."); yield break; }
        if (!DateTime.TryParseExact(EndDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var end))
        { yield return Fail("end_date", "The Planned release does not match the format Y-m-d."); yield break; }
        var min = DraftPickupDate != null ? DateTime.Parse(DraftPickupDate, CultureInfo.InvariantCulture) : DateTime.Today;
        if (end.Date < min.Date) yield return Fail("end_date", "The Planned release cannot be before the pickup date.");
    }
}
