
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace TruckLink.ViewModels;





public abstract class Request : IValidatableObject
{
    public static readonly Dictionary<string, string> ATTRIBUTES = new()
    {
        ["first_name"] = "First name", ["last_name"] = "Last name", ["email"] = "Email address",
        ["login"] = "Email or phone number", ["phone"] = "Phone number", ["city"] = "City", ["address"] = "Address",
        ["password"] = "Password", ["password_confirmation"] = "Confirm password", ["terms"] = "Terms of Service",
        ["card_holder_name"] = "Name on card", ["card_number"] = "Card number", ["expiry_date"] = "Expiry date", ["cvv"] = "CVV",
        ["service_id"] = "Service", ["pickup_address"] = "Pickup address", ["pickup_city"] = "Pickup city",
        ["dropoff_address"] = "Drop-off address", ["dropoff_city"] = "Drop-off city", ["pickup_date"] = "Pickup date",
        ["pickup_time"] = "Pickup time", ["customer_notes"] = "Notes for the crew",
        ["item_name"] = "Item name", ["category_id"] = "Category", ["quantity"] = "Quantity", ["photos"] = "Item photos", ["photos.*"] = "Item photo",
        ["estimated_length_cm"] = "Length", ["estimated_width_cm"] = "Width", ["estimated_height_cm"] = "Height", ["estimated_weight_kg"] = "Weight",
        ["end_date"] = "Planned release", ["extension_type"] = "Extension",
        ["vehicle_id"] = "Vehicle", ["driver_id"] = "Driver", ["crew"] = "Moving crew",
        ["rejection_reason"] = "Reason for rejection", ["customer_message"] = "Message to the customer",
        ["plate_number"] = "Plate number", ["vehicle_type"] = "Vehicle type", ["capacity_volume_m3"] = "Capacity volume",
        ["max_weight_kg"] = "Maximum weight", ["is_temperature_controlled"] = "Temperature controlled",
        ["vehicle_status"] = "Vehicle status", ["confirm_plate"] = "Plate number confirmation",
        ["job_title"] = "Job title", ["license_number"] = "Licence number", ["hire_date"] = "Hire date",
        ["rating"] = "Rating", ["comment"] = "Review", ["message_text"] = "Message", ["title"] = "Title", ["content"] = "Message",
    };

    
    public virtual Dictionary<string, string> Attributes() => new(ATTRIBUTES);

    
    public virtual string ErrorBag => "default";

    
    public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext) => Enumerable.Empty<ValidationResult>();

    
    protected static bool Filled(string? v) => !string.IsNullOrWhiteSpace(v);

    protected static ValidationResult Fail(string field, string message) => new(message, new[] { field });
}
