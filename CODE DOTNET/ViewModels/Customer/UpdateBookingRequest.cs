
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using TruckLink.Models;

namespace TruckLink.ViewModels.Customer;


public class UpdateBookingRequest : LocationsStepRequest
{
    
    [BindNever] public Booking? Booking { get; set; }

    
    [FromForm(Name = "services")]
    public Dictionary<int, ExtrasStepRequest.ServiceLine>? Services { get; set; }

    
    public override IEnumerable<ValidationResult> Validate(ValidationContext ctx)
    {
        var unchanged = Booking != null && $"{PickupDate} {PickupTime}" == Booking.PickupDatetime.ToString("yyyy-MM-dd HH:mm");
        if (!unchanged)
            foreach (var r in base.Validate(ctx)) yield return r;      

        if (Services != null)
            foreach (var (id, line) in Services)
                if (line.Quantity is < 0 or > 20)
                    yield return Fail($"services.{id}.quantity", "Choose a quantity between 0 and 20.");
    }

    
    public Dictionary<int, int>? Extras()
    {
        if (Services == null) return null;
        return Services.ToDictionary(kv => kv.Key, kv => kv.Value.Selected == true ? Math.Max(1, kv.Value.Quantity ?? 1) : 0);
    }
}
