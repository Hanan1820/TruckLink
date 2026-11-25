
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using TruckLink.Support.Rules;

namespace TruckLink.ViewModels.Auth;


public class RegisterRequest : Request
{
    public const string NAME_REGEX = @"^[\p{IsArabic}A-Za-z\s'\-]+$";
    private const string TAKEN = "An account with this :label already exists. Sign in instead.";
    private const string NAME_MESSAGE = "The {0} may only contain letters, spaces, hyphens and apostrophes.";

    
    [FromForm(Name = "first_name"), Display(Name = "First name")]
    [Required, StringLength(50, MinimumLength = 2), RegularExpression(NAME_REGEX, ErrorMessage = NAME_MESSAGE)]
    public string FirstName { get; set; } = "";

    [FromForm(Name = "last_name"), Display(Name = "Last name")]
    [Required, StringLength(50, MinimumLength = 2), RegularExpression(NAME_REGEX, ErrorMessage = NAME_MESSAGE)]
    public string LastName { get; set; } = "";

    
    
    [FromForm(Name = "email"), Display(Name = "Email address")]
    [Required, EmailAddress, StringLength(100), AccountIdentifierFree("email", TAKEN)]
    public string Email { get; set; } = "";

    
    [FromForm(Name = "phone"), Display(Name = "Phone number")]
    [Required, TruckLinkPhone, AccountIdentifierFree("phone", TAKEN)]
    public string Phone { get; set; } = "";

    [FromForm(Name = "city"), Display(Name = "City")]
    [Required, InConfig("TruckLink:Cities")]           
    public string City { get; set; } = "";

    [FromForm(Name = "address"), Display(Name = "Address")]
    [StringLength(200)]                                
    public string? Address { get; set; }

    
    [FromForm(Name = "password"), Display(Name = "Password")]
    [Required, StringLength(20), StrongPassword]
    public string Password { get; set; } = "";

    [FromForm(Name = "password_confirmation"), Display(Name = "Confirm password")]
    [Compare(nameof(Password), ErrorMessage = "The Password confirmation does not match.")]   
    public string PasswordConfirmation { get; set; } = "";

    [FromForm(Name = "terms"), Display(Name = "Terms of Service")]
    [Accepted(ErrorMessage = "Please accept the Terms of Service and the Privacy Policy to continue.")]
    public string? Terms { get; set; }

    
    public Dictionary<string, object?> Customer() => new()
    {
        ["first_name"] = FirstName.Trim(),
        ["last_name"] = LastName.Trim(),
        ["email"] = Email.Trim().ToLowerInvariant(),
        ["phone"] = TruckLinkPhone.Canonical(Phone),
        ["city"] = City,
        ["address"] = Filled(Address) ? Address!.Trim() : null,
    };
}
