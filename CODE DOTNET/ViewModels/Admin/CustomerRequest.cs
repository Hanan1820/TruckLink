
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using TruckLink.Support.Auth;
using TruckLink.Support.Rules;

namespace TruckLink.ViewModels.Admin;


public class CustomerRequest : AccountRequest
{
    
    public override Authenticatable? Record { get; set; }

    [FromForm(Name = "city"), Display(Name = "City")]
    [Required, InConfig("TruckLink:Cities")]
    public string City { get; set; } = "";

    [FromForm(Name = "address"), Display(Name = "Address")]
    [StringLength(200)]
    public string? Address { get; set; }

    [FromForm(Name = "account_status"), Display(Name = "Account status")]
    [Required, In("Active", "Suspended", "Blocked")]        
    public string AccountStatus { get; set; } = "";

    public Dictionary<string, object?> Customer()
    {
        var data = Account();
        data["city"] = City;
        data["address"] = Filled(Address) ? Address!.Trim() : null;
        data["account_status"] = AccountStatus;
        return data;
    }
}
