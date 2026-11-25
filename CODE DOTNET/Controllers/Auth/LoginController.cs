
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TruckLink.Data;
using TruckLink.Models;
using TruckLink.Support;
using TruckLink.Support.Auth;
using TruckLink.Support.Helpers;
using TruckLink.Support.Middleware;
using TruckLink.Support.Rules;
using TruckLink.ViewModels.Auth;

namespace TruckLink.Controllers.Auth;


public class LoginController : Controller
{
    
    private static readonly string[] ACCOUNTS = { "admin", "staff", "customer" };

    public const int MAX_ATTEMPTS = 5;

    private readonly OneTimePassword _otp;
    private readonly RateLimiter _limiter;
    private readonly IPasswordHasher<object> _hasher;
    private readonly IConfiguration _config;
    private readonly AppDbContext _db;

    public LoginController(OneTimePassword otp, RateLimiter limiter, IPasswordHasher<object> hasher, IConfiguration config, AppDbContext db)
    {
        _otp = otp; _limiter = limiter; _hasher = hasher; _config = config; _db = db;
    }

    [HttpGet("/login", Name = "login")]
    [TypeFilter(typeof(RedirectIfAuthenticated), Arguments = new object[] { new[] { "admin", "staff", "customer" } })]   
    public IActionResult Create() => View("~/Views/Auth/Login.cshtml");

    [HttpPost("/login"), ValidateAntiForgeryToken]
    [TypeFilter(typeof(RedirectIfAuthenticated), Arguments = new object[] { new[] { "admin", "staff", "customer" } })]
    public IActionResult Store(LoginRequest request)
    {
        if (!ModelState.IsValid) return BackWithErrors();   

        var identifier = Normalise(request.Login);
        var throttleKey = "login|" + identifier + "|" + HttpContext.Connection.RemoteIpAddress;

        
        if (_limiter.TooManyAttempts(throttleKey, MAX_ATTEMPTS))
        {
            ModelState.AddModelError("credentials", "Too many sign-in attempts. Please wait " + _limiter.AvailableIn(throttleKey) + " seconds and try again.");
            return BackWithErrors();
        }

        var (guard, user) = FindAccount(identifier, _db);
        if (user == null || _hasher.VerifyHashedPassword(null!, user.AuthPassword, request.Password) == PasswordVerificationResult.Failed)
        {
            _limiter.Hit(throttleKey, 60);
            ModelState.AddModelError("credentials", "The email, phone number or password is incorrect.");
            return BackWithErrors();
        }

        var reason = Areas.BlockedReason(guard!, user, _config);
        if (reason != null)
        {
            ModelState.AddModelError("credentials", reason);
            return BackWithErrors();
        }

        _limiter.Clear(throttleKey);
        _otp.StartLogin(guard!, user, request.Remember);
        return RedirectToRoute("otp");
    }

    [HttpPost("/logout", Name = "logout"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Destroy()
    {
        foreach (var guard in Areas.HOMES.Keys) await AuthManager.LogoutAsync(guard);
        HttpContext.Session.Clear();   
        return RedirectToRoute("login");
    }

    
    public static string Normalise(string login)
    {
        login = login.Trim();
        if (login.Contains('@')) return login.ToLowerInvariant();
        return Regex.IsMatch(TruckLinkPhone.National(login), @"^5\d{8}$") ? TruckLinkPhone.Canonical(login) : login.ToLowerInvariant();
    }

    
    public static (string? Guard, Authenticatable? User) FindAccount(string identifier, AppDbContext db)
    {
        var byEmail = identifier.Contains('@');
        foreach (var guard in ACCOUNTS)
        {
            Authenticatable? user = guard switch
            {
                "admin" => byEmail ? db.Admins.FirstOrDefault(a => a.Email == identifier) : db.Admins.FirstOrDefault(a => a.Phone == identifier),
                "staff" => byEmail ? db.Staff.FirstOrDefault(a => a.Email == identifier) : db.Staff.FirstOrDefault(a => a.Phone == identifier),
                _ => byEmail ? db.Customers.FirstOrDefault(a => a.Email == identifier) : db.Customers.FirstOrDefault(a => a.Phone == identifier),
            };
            if (user != null) return (guard, user);
        }
        return (null, null);
    }
}
