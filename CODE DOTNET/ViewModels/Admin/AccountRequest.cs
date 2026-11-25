
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using TruckLink.Support.Auth;
using TruckLink.Support.Rules;

namespace TruckLink.ViewModels.Admin;





public abstract class AccountRequest : Request
{
    protected const string NAME_MESSAGE = "The {0} may only contain letters, spaces, hyphens and apostrophes.";

    
    public abstract Authenticatable? Record { get; set; }

    [FromForm(Name = "first_name"), Display(Name = "First name")]
    [Required, StringLength(50, MinimumLength = 2), RegularExpression(ViewModels.Auth.RegisterRequest.NAME_REGEX, ErrorMessage = NAME_MESSAGE)]
    public string FirstName { get; set; } = "";

    [FromForm(Name = "last_name"), Display(Name = "Last name")]
    [Required, StringLength(50, MinimumLength = 2), RegularExpression(ViewModels.Auth.RegisterRequest.NAME_REGEX, ErrorMessage = NAME_MESSAGE)]
    public string LastName { get; set; } = "";

    [FromForm(Name = "email"), Display(Name = "Email address")]
    [Required, EmailAddress, StringLength(100), AccountIdentifierFree("email")]
    public string Email { get; set; } = "";

    [FromForm(Name = "phone"), Display(Name = "Phone number")]
    [Required, TruckLinkPhone, AccountIdentifierFree("phone")]
    public string Phone { get; set; } = "";

    
    
    [FromForm(Name = "password"), Display(Name = "Password")]
    [StringLength(20), StrongPassword]
    public string? Password { get; set; }

    [FromForm(Name = "password_confirmation"), Display(Name = "Confirm password")]
    public string? PasswordConfirmation { get; set; }

    public override IEnumerable<ValidationResult> Validate(ValidationContext ctx)
    {
        if (Record == null)
        {
            if (!Filled(Password)) yield return Fail("password", "The Password field is required.");
            if (Password != PasswordConfirmation) yield return Fail("password", "The Password confirmation does not match.");
        }
    }

    
    public Dictionary<string, object?> Account()
    {
        var data = new Dictionary<string, object?>
        {
            ["first_name"] = FirstName.Trim(), ["last_name"] = LastName.Trim(), ["email"] = Email, ["phone"] = Phone,
        };
        if (Record == null) data["password"] = Password;
        return data;
    }
}
