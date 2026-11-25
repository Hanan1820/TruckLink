
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TruckLink.Support;
using TruckLink.Support.Middleware;
using TruckLink.ViewModels.Auth;

namespace TruckLink.Controllers.Auth;


[TypeFilter(typeof(RedirectIfAuthenticated), Arguments = new object[] { new[] { "admin", "staff", "customer" } })]   
public class RegisterController : Controller
{
    private readonly IConfiguration _config;
    private readonly IPasswordHasher<object> _hasher;
    private readonly OneTimePassword _otp;

    public RegisterController(IConfiguration config, IPasswordHasher<object> hasher, OneTimePassword otp)
    {
        _config = config; _hasher = hasher; _otp = otp;
    }

    [HttpGet("/register", Name = "register")]
    public IActionResult Create() =>
        View("~/Views/Auth/Register.cshtml", new { cities = _config.GetSection("TruckLink:Cities").Get<string[]>() });

    
    [HttpPost("/register"), ValidateAntiForgeryToken]
    public IActionResult Store(RegisterRequest request)
    {
        if (!ModelState.IsValid) return BackWithErrors();

        var customer = request.Customer();
        customer["password"] = _hasher.HashPassword(null!, request.Password);   
        _otp.StartRegistration(customer);
        return RedirectToRoute("otp");
    }
}
