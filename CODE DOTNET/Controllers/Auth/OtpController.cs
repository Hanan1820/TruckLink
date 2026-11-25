
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TruckLink.Data;
using TruckLink.Models;
using TruckLink.Support;
using TruckLink.Support.Auth;
using TruckLink.Support.Helpers;
using TruckLink.Support.Middleware;

namespace TruckLink.Controllers.Auth;


public class OtpController : Controller
{
    private readonly OneTimePassword _otp;
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;
    private readonly SessionStore _session;

    public OtpController(OneTimePassword otp, AppDbContext db, IConfiguration config, SessionStore session)
    {
        _otp = otp; _db = db; _config = config; _session = session;
    }

    [HttpGet("/otp", Name = "otp")]
    [TypeFilter(typeof(RedirectIfAuthenticated), Arguments = new object[] { new[] { "admin", "staff", "customer" } })]
    public IActionResult Show()
    {
        if (_otp.Pending() == null) return RedirectToRoute("login");
        if (_otp.IsReset()) return RedirectToRoute("password.reset");   

        return View("~/Views/Auth/Otp.cshtml", new
        {
            phone = _otp.MaskedPhone(),
            registering = _otp.IsRegistration(),
            simulatedCode = _otp.SimulatedCode(),
        });
    }

    [HttpPost("/otp"), ValidateAntiForgeryToken]
    [TypeFilter(typeof(RedirectIfAuthenticated), Arguments = new object[] { new[] { "admin", "staff", "customer" } })]
    public async Task<IActionResult> Verify([FromForm(Name = "code")] string? code)
    {
        var pending = _otp.Pending();
        if (pending == null) return RedirectToRoute("login");
        if (_otp.IsReset()) return RedirectToRoute("password.reset");

        
        if (string.IsNullOrEmpty(code)) ModelState.AddModelError("code", "The One-time password field is required.");
        else if (!Regex.IsMatch(code, @"^\d{6}$")) ModelState.AddModelError("code", "The code is the 6 digits in the message we sent.");
        if (!ModelState.IsValid) return BackWithErrors();

        var error = _otp.Check(code!);
        if (error != null)
        {
            ModelState.AddModelError("code", error);   
            return BackWithErrors();
        }

        Authenticatable? user = pending.Register != null
            ? CreateCustomer(pending.Register)
            : RetrieveById(pending.Guard, pending.Id!.Value);   

        if (user == null)
        {
            _otp.Forget();
            WithErrors(new()
            {
                ["credentials"] = pending.Register != null
                    ? "An account with this email address or phone number was created in the meantime. Sign in instead."
                    : "This account is no longer available.",
            });
            return RedirectToRoute(pending.Register != null ? "register" : "login");
        }

        var reason = Areas.BlockedReason(pending.Guard, user, _config);
        if (reason != null)
        {
            _otp.Forget();
            WithErrors(new() { ["credentials"] = reason });
            return RedirectToRoute("login");
        }

        
        foreach (var guard in Areas.HOMES.Keys) await AuthManager.LogoutAsync(guard);

        
        await AuthManager.LoginAsync(pending.Guard, user, pending.Remember);
        _otp.Forget();
        
        var intended = _session.Pull<string>("url.intended");
        return Redirect(intended ?? Areas.Home(pending.Guard));
    }

    [HttpPost("/otp/resend", Name = "otp.resend"), ValidateAntiForgeryToken]
    [TypeFilter(typeof(RedirectIfAuthenticated), Arguments = new object[] { new[] { "admin", "staff", "customer" } })]
    public IActionResult Resend()
    {
        if (_otp.Pending() == null) return RedirectToRoute("login");

        if (_otp.Resend())
        {
            With("status", "We sent a new code to " + _otp.MaskedPhone() + ".");
            return Back();
        }
        ModelState.AddModelError("code", "Please wait " + OneTimePassword.RESEND_AFTER_SECONDS + " seconds before asking for a new code.");
        return BackWithErrors();
    }

    private Authenticatable? RetrieveById(string guard, int id) => guard switch
    {
        "customer" => _db.Customers.Find(id),
        "staff" => _db.Staff.Find(id),
        "admin" => _db.Admins.Find(id),
        _ => null,
    };

    
    private Authenticatable? CreateCustomer(Dictionary<string, object?> attributes)
    {
        try
        {
            var customer = new Models.Customer   
            {
                FirstName = (string)attributes["first_name"]!, LastName = (string)attributes["last_name"]!,
                Email = (string)attributes["email"]!, Phone = (string)attributes["phone"]!, City = (string)attributes["city"]!,
                Address = attributes["address"] as string, Password = (string)attributes["password"]!, AccountStatus = "Active",
            };
            _db.Customers.Add(customer);
            _db.SaveChanges();
            return customer;
        }
        catch (DbUpdateException e)   
        {
            
            if (e.ToString().Contains("UQ_customers_")) return null;
            throw;
        }
    }
}
