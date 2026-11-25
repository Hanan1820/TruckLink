
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using TruckLink.Support.Rules;

namespace TruckLink.ViewModels.Customer;


public class ServiceStepRequest : Request
{
    
    [FromForm(Name = "service_id"), Display(Name = "Service")]
    [Required(ErrorMessage = "Choose the service you need.")]
    [Exists("services", "service_id", WhereColumn = "service_type", WhereValue = "Transport", WhereColumn2 = "is_active", WhereValue2 = "1")]
    public int? ServiceId { get; set; }
}
