
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TruckLink.Data;
using TruckLink.Support.Middleware;
using TruckLink.Support.Rules;

namespace TruckLink.Controllers.Staff;


[Route("staff"), TypeFilter(typeof(Authenticate), Arguments = new object[] { "staff" })]
public class ProfileController : Controller
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher<object> _hasher;

    public ProfileController(AppDbContext db, IPasswordHasher<object> hasher) { _db = db; _hasher = hasher; }

    [HttpGet("profile", Name = "staff.profile")]
    public IActionResult Edit()
    {
        var staff = AuthManager.Staff()!;
        _db.Entry(staff).Collection(s => s.Warehouses).Load();   
        return View("~/Views/Staff/Profile.cshtml", new { member = staff });
    }

    [HttpPut("profile", Name = "staff.profile.update"), ValidateAntiForgeryToken]
    public IActionResult Update([FromForm(Name = "email")] string? email, [FromForm(Name = "phone")] string? phone)
    {
        var staff = AuthManager.Staff()!;
        var holder = new Holder(staff);
        var ctx = new ValidationContext(holder, HttpContext.RequestServices, null);

        
        if (string.IsNullOrWhiteSpace(email)) ModelState.AddModelError("email", "The Email address field is required.");
        else if (email.Length > 100 || !new EmailAddressAttribute().IsValid(email)) ModelState.AddModelError("email", "The Email address must be a valid email address.");
        else
        {
            ctx.MemberName = "email"; ctx.DisplayName = "Email address";
            var r = new AccountIdentifierFree("email").GetValidationResult(email, ctx);
            if (r != ValidationResult.Success) ModelState.AddModelError("email", r!.ErrorMessage!);
        }

        
        if (string.IsNullOrWhiteSpace(phone)) ModelState.AddModelError("phone", "The Phone number field is required.");
        else
        {
            ctx.MemberName = "phone"; ctx.DisplayName = "Phone number";
            var bad = new TruckLinkPhone().GetValidationResult(phone, ctx);
            if (bad != ValidationResult.Success) ModelState.AddModelError("phone", bad!.ErrorMessage!);
            else
            {
                var r = new AccountIdentifierFree("phone").GetValidationResult(phone, ctx);
                if (r != ValidationResult.Success) ModelState.AddModelError("phone", r!.ErrorMessage!);
            }
        }
        if (!ModelState.IsValid) return BackWithErrors("profile");

        staff.Email = email!;
        staff.Phone = phone!;
        _db.SaveChanges();

        With("status", "Your contact details are saved.");
        return RedirectToRoute("staff.profile");
    }

    [HttpPut("profile/password", Name = "staff.profile.password"), ValidateAntiForgeryToken]
    public IActionResult Password([FromForm(Name = "current_password")] string? current, [FromForm(Name = "password")] string? password,
        [FromForm(Name = "password_confirmation")] string? confirmation)
    {
        var staff = AuthManager.Staff()!;

        if (string.IsNullOrEmpty(current)) ModelState.AddModelError("current_password", "The Current password field is required.");
        else if (_hasher.VerifyHashedPassword(null!, staff.Password, current) == PasswordVerificationResult.Failed)
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

        staff.Password = _hasher.HashPassword(null!, password!);
        _db.SaveChanges();

        With("status", "Your password is changed.");
        return RedirectToRoute("staff.profile");
    }

    
    private sealed class Holder
    {
        public Holder(Models.Staff record) => Record = record;
        public Support.Auth.Authenticatable Record { get; }
    }
}
