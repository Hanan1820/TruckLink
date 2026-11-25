
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using TruckLink.Models;
using TruckLink.Support.Auth;
using TruckLink.Support.Rules;

namespace TruckLink.ViewModels.Admin;


public class AdminAccountRequest : AccountRequest
{
    
    public override Authenticatable? Record { get; set; }

    [FromForm(Name = "admin_role"), Display(Name = "Role")]
    [Required, In("Super Admin", "Operations Admin")]       
    public string AdminRole { get; set; } = "";

    
    public static bool Authorize(Models.Admin? user) => user?.IsSuperAdmin() == true;
}
