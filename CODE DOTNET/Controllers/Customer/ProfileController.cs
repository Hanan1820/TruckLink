
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TruckLink.Data;
using TruckLink.Support;
using TruckLink.Support.Middleware;
using TruckLink.Support.Rules;
using TruckLink.ViewModels.Auth;

namespace TruckLink.Controllers.Customer;


[Route("customer"), TypeFilter(typeof(Authenticate), Arguments = new object[] { "customer" })]
public class ProfileController : Controller
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;
    private readonly IPasswordHasher<object> _hasher;

    public ProfileController(AppDbContext db, IConfiguration config, IPasswordHasher<object> hasher)
    {
        _db = db; _config = config; _hasher = hasher;
    }

    [HttpGet("profile", Name = "customer.profile")]
    public IActionResult Edit() => View("~/Views/Customer/Profile.cshtml", new { customer = AuthManager.Customer() });

    
    [HttpPut("profile", Name = "customer.profile.update"), ValidateAntiForgeryToken]
    public IActionResult Update(
        [FromForm(Name = "first_name")] string? firstName, [FromForm(Name = "last_name")] string? lastName,
        [FromForm(Name = "email")] string? email, [FromForm(Name = "phone")] string? phone,
        [FromForm(Name = "city")] string? city, [FromForm(Name = "address")] string? address)
    {
        var customer = AuthManager.Customer()!;

        
        void Name(string field, string label, string? v)
        {
            if (string.IsNullOrWhiteSpace(v)) ModelState.AddModelError(field, $"The {label} field is required.");
            else if (v.Length < 2 || v.Length > 50) ModelState.AddModelError(field, $"The {label} must be between 2 and 50 characters.");
            else if (!System.Text.RegularExpressions.Regex.IsMatch(v, RegisterRequest.NAME_REGEX)) ModelState.AddModelError(field, $"The {label} may only contain letters, spaces, hyphens and apostrophes.");
        }
        Name("first_name", "First name", firstName);
        Name("last_name", "Last name", lastName);

        var ctx = new ValidationContext(new ProfileRecord(customer)) { MemberName = "email", DisplayName = "Email address", Items = { } };
        if (string.IsNullOrWhiteSpace(email)) ModelState.AddModelError("email", "The Email address field is required.");
        else if (email.Length > 100 || !new EmailAddressAttribute().IsValid(email)) ModelState.AddModelError("email", "The Email address must be a valid email address.");
        else
        {
            var taken = new AccountIdentifierFree("email", "An account with this :label already exists. Sign in instead.") { IgnoreProperty = "Record" }
                .GetValidationResult(email, new ValidationContext(new ProfileRecord(customer), HttpContext.RequestServices, null) { MemberName = "email" });
            if (taken != ValidationResult.Success) ModelState.AddModelError("email", taken!.ErrorMessage!);
        }

        if (string.IsNullOrWhiteSpace(phone)) ModelState.AddModelError("phone", "The Phone number field is required.");
        else
        {
            var pctx = new ValidationContext(new ProfileRecord(customer), HttpContext.RequestServices, null) { MemberName = "phone", DisplayName = "Phone number" };
            var bad = new TruckLinkPhone().GetValidationResult(phone, pctx);
            if (bad != ValidationResult.Success) ModelState.AddModelError("phone", bad!.ErrorMessage!);
            else
            {
                var taken = new AccountIdentifierFree("phone", "An account with this :label already exists. Sign in instead.") { IgnoreProperty = "Record" }.GetValidationResult(phone, pctx);
                if (taken != ValidationResult.Success) ModelState.AddModelError("phone", taken!.ErrorMessage!);
            }
        }

        var cities = _config.GetSection("TruckLink:Cities").Get<string[]>() ?? Array.Empty<string>();
        if (string.IsNullOrEmpty(city)) ModelState.AddModelError("city", "The City field is required.");
        else if (!cities.Contains(city)) ModelState.AddModelError("city", "The selected City is invalid.");
        if (address?.Length > 200) ModelState.AddModelError("address", "The Address must not be greater than 200 characters.");
        if (!ModelState.IsValid) return BackWithErrors("profile");

        customer.FirstName = firstName!.Trim(); customer.LastName = lastName!.Trim(); customer.Email = email!;
        customer.Phone = phone!; customer.City = city!;
        customer.Address = string.IsNullOrWhiteSpace(address) ? null : address.Trim();
        _db.SaveChanges();

        With("status", "Your details are saved.");
        return RedirectToRoute("customer.profile");
    }

    
    [HttpPut("profile/card", Name = "customer.profile.card"), ValidateAntiForgeryToken]
    public IActionResult SaveCard(
        [FromForm(Name = "card_holder_name")] string? holder, [FromForm(Name = "card_number")] string? number,
        [FromForm(Name = "expiry_date")] string? expiry, [FromForm(Name = "cvv")] string? cvv)
    {
        foreach (var (field, message) in Card.Validate("required", holder, number, expiry, cvv))   
            ModelState.AddModelError(field, message);
        if (!ModelState.IsValid) return BackWithErrors("card");

        var customer = AuthManager.Customer()!;
        customer.CardHolderName = holder!.Trim();
        customer.CardNumber = Card.Mask(number!);
        customer.CardType = Card.Type(number!);
        customer.ExpiryDate = expiry!.Trim();
        _db.SaveChanges();

        With("status", "Your card is saved. Only its last four digits are kept.");
        return RedirectToRoute("customer.profile");
    }

    [HttpDelete("profile/card", Name = "customer.profile.card.remove"), ValidateAntiForgeryToken]
    public IActionResult RemoveCard()
    {
        var customer = AuthManager.Customer()!;
        customer.CardHolderName = null; customer.CardNumber = null; customer.CardType = null; customer.ExpiryDate = null;
        _db.SaveChanges();

        With("status", "Your saved card is removed.");
        return RedirectToRoute("customer.profile");
    }

    [HttpPut("profile/password", Name = "customer.profile.password"), ValidateAntiForgeryToken]
    public IActionResult Password(
        [FromForm(Name = "current_password")] string? current, [FromForm(Name = "password")] string? password,
        [FromForm(Name = "password_confirmation")] string? confirmation)
    {
        var customer = AuthManager.Customer()!;

        
        if (string.IsNullOrEmpty(current)) ModelState.AddModelError("current_password", "The Current password field is required.");
        else if (_hasher.VerifyHashedPassword(null!, customer.Password, current) == PasswordVerificationResult.Failed)
            ModelState.AddModelError("current_password", "This is not your current password.");

        if (string.IsNullOrEmpty(password)) ModelState.AddModelError("password", "The New password field is required.");
        else
        {
            if (password != confirmation) ModelState.AddModelError("password", "The New password confirmation does not match.");
            if (password.Length > 20) ModelState.AddModelError("password", "The New password must not be greater than 20 characters.");
            var strong = new StrongPasswordAttribute();
            if (!strong.IsValid(password)) ModelState.AddModelError("password", strong.FormatErrorMessage("New password"));
            if (password == current) ModelState.AddModelError("password", "Choose a password you have not used here.");   
        }
        if (!ModelState.IsValid) return BackWithErrors("password");

        customer.Password = _hasher.HashPassword(null!, password!);   
        _db.SaveChanges();

        With("status", "Your password is changed.");
        return RedirectToRoute("customer.profile");
    }

    
    private sealed class ProfileRecord
    {
        public ProfileRecord(Models.Customer record) => Record = record;
        public Support.Auth.Authenticatable Record { get; }
    }
}
