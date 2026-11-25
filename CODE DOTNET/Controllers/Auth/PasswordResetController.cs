
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TruckLink.Data;
using TruckLink.Models;
using TruckLink.Support;
using TruckLink.Support.Helpers;
using TruckLink.Support.Middleware;
using TruckLink.Support.Rules;

namespace TruckLink.Controllers.Auth;





[TypeFilter(typeof(RedirectIfAuthenticated), Arguments = new object[] { new[] { "admin", "staff", "customer" } })]   
public class PasswordResetController : Controller
{
    private readonly OneTimePassword _otp;
    private readonly RateLimiter _limiter;
    private readonly AppDbContext _db;
    private readonly IPasswordHasher<object> _hasher;

    public PasswordResetController(OneTimePassword otp, RateLimiter limiter, AppDbContext db, IPasswordHasher<object> hasher)
    {
        _otp = otp; _limiter = limiter; _db = db; _hasher = hasher;
    }

    [HttpGet("/password/forgot", Name = "password.forgot")]
    public IActionResult Create() => View("~/Views/Auth/Forgot.cshtml");

    [HttpPost("/password/forgot"), ValidateAntiForgeryToken]
    public IActionResult Store([FromForm(Name = "login")] string? login)
    {
        
        if (string.IsNullOrWhiteSpace(login)) ModelState.AddModelError("login", "The Email or phone number field is required.");
        else if (login.Length > 100) ModelState.AddModelError("login", "The Email or phone number must not be greater than 100 characters.");
        if (!ModelState.IsValid) return BackWithErrors();

        var identifier = LoginController.Normalise(login!);
        var key = "reset|" + identifier + "|" + HttpContext.Connection.RemoteIpAddress;
        if (_limiter.TooManyAttempts(key, LoginController.MAX_ATTEMPTS))
        {
            ModelState.AddModelError("login", "Too many requests. Please wait " + _limiter.AvailableIn(key) + " seconds and try again.");
            return BackWithErrors();
        }
        _limiter.Hit(key, 60);

        var (guard, user) = LoginController.FindAccount(identifier, _db);
        if (user == null)
        {
            ModelState.AddModelError("login", "We could not find an account with this email address or phone number.");
            return BackWithErrors();
        }

        _otp.StartReset(guard!, user);
        return RedirectToRoute("password.reset");
    }

    [HttpGet("/password/reset", Name = "password.reset")]
    public IActionResult Edit()
    {
        if (!_otp.IsReset()) return RedirectToRoute("password.forgot");

        return View("~/Views/Auth/Reset.cshtml", new { phone = _otp.MaskedPhone(), simulatedCode = _otp.SimulatedCode() });
    }

    [HttpPost("/password/reset"), ValidateAntiForgeryToken]
    public IActionResult Update([FromForm(Name = "code")] string? code, [FromForm(Name = "password")] string? password,
        [FromForm(Name = "password_confirmation")] string? passwordConfirmation)
    {
        if (!_otp.IsReset()) return RedirectToRoute("password.forgot");

        
        if (string.IsNullOrEmpty(code)) ModelState.AddModelError("code", "The One-time password field is required.");
        else if (!Regex.IsMatch(code, @"^\d{6}$")) ModelState.AddModelError("code", "The code is the 6 digits in the message we sent.");
        if (string.IsNullOrEmpty(password)) ModelState.AddModelError("password", "The New password field is required.");
        else
        {
            if (password != passwordConfirmation) ModelState.AddModelError("password", "The New password confirmation does not match.");
            if (password.Length > 20) ModelState.AddModelError("password", "The New password must not be greater than 20 characters.");
            if (!new StrongPasswordAttribute().IsValid(password)) ModelState.AddModelError("password", new StrongPasswordAttribute().FormatErrorMessage("New password"));
        }
        if (!ModelState.IsValid) return BackWithErrors();

        var error = _otp.Check(code!);
        if (error != null)
        {
            ModelState.AddModelError("code", error);   
            return BackWithErrors();
        }

        var pending = _otp.Pending()!;
        Support.Auth.Authenticatable? user = pending.Guard switch
        {
            "customer" => _db.Customers.Find(pending.Id),
            "staff" => _db.Staff.Find(pending.Id),
            "admin" => _db.Admins.Find(pending.Id),
            _ => null,
        };
        _otp.Forget();
        if (user == null) return NotFound();   

        var hash = _hasher.HashPassword(null!, password!);   
        switch (user)
        {
            case Models.Customer c: c.Password = hash; break;
            case Models.Staff s: s.Password = hash; break;
            case Models.Admin a: a.Password = hash; break;
        }
        _db.SaveChanges();

        With("status", "Your password is changed. Sign in with the new one.");
        return RedirectToRoute("login");
    }

    [HttpPost("/password/resend", Name = "password.resend"), ValidateAntiForgeryToken]
    public IActionResult Resend()
    {
        if (!_otp.IsReset()) return RedirectToRoute("password.forgot");

        if (_otp.Resend())
        {
            With("status", "We sent a new code to " + _otp.MaskedPhone() + ".");
            return Back();
        }
        ModelState.AddModelError("code", "Please wait " + OneTimePassword.RESEND_AFTER_SECONDS + " seconds before asking for a new code.");
        return BackWithErrors();
    }
}
