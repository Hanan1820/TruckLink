
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace TruckLink.ViewModels.Auth;


public class LoginRequest : Request
{
    [FromForm(Name = "login")]
    [Display(Name = "Email or phone number")]
    [Required, StringLength(100)]                      
    public string Login { get; set; } = "";

    [FromForm(Name = "password")]
    [Display(Name = "Password")]
    [Required]                                         
    public string Password { get; set; } = "";

    
    [FromForm(Name = "remember")]
    public bool Remember { get; set; }
}
