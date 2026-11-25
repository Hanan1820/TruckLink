
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TruckLink.Data;
using TruckLink.Models;
using TruckLink.Support.Services;

namespace TruckLink.Controllers;


public class HomeController : Controller
{
    
    private static readonly List<Dictionary<string, object?>> FALLBACK_TESTIMONIALS = new()
    {
        new() { ["first"] = "Nora", ["last"] = "Al-Qahtani", ["line"] = "Al Olaya → Al Yasmin, Riyadh", ["stars"] = 5.0,
            ["text"] = "I uploaded eight photos and they told me exactly which truck I needed. No arguing at the door about the price, no second trip. The whole flat moved in one go." },
        new() { ["first"] = "Faisal", ["last"] = "Al-Harbi", ["line"] = "Jeddah", ["stars"] = 5.0,
            ["text"] = "My new villa was two weeks late. TruckLink kept everything in their warehouse - the first week cost nothing and the rest was 30 riyals a day. That saved the whole move." },
        new() { ["first"] = "Maha", ["last"] = "Al-Dosari", ["line"] = "Dammam · business account", ["stars"] = 4.5,
            ["text"] = "We ship skincare stock every month and the refrigerated van has never let us down. Same company, same drivers, one invoice per delivery." },
    };

    private readonly AppDbContext _db;
    private readonly VehicleRecommender _fleet;

    public HomeController(AppDbContext db, VehicleRecommender fleet) { _db = db; _fleet = fleet; }

    
    [HttpGet("/", Name = "home")]
    public IActionResult Invoke()
    {
        var services = Service.ScopeActive(_db.Services).ToList();
        Service? Named(string name) => services.FirstOrDefault(s => s.ServiceName == name);
        Service? Unit(string unit) => services.FirstOrDefault(s => s.ServiceType == "Storage" && s.PricingUnit == unit);

        return View("~/Views/Public/Home.cshtml", new
        {
            estimateUrl = AuthManager.Check("customer") ? "/customer/bookings/create" : "/register",
            
            moving = Named("Single Move") ?? services.Where(s => s.ServiceType == "Transport").OrderBy(s => s.BasePrice).FirstOrDefault(),
            temperature = Named("Temperature-Controlled"),
            storage = Unit("first 7 days"),
            valueAddedFrom = services.Where(s => s.ServiceType == "Value-Added").Select(s => (decimal?)s.BasePrice).Min(),
            extension = new { day = Unit("per day"), week = Unit("per week"), month = Unit("per month") },
            capacity = _fleet.Capacities().ToDictionary(kv => kv.Key, kv => kv.Value.Capacity),
            testimonials = Testimonials(),
        });
    }

    
    private List<Dictionary<string, object?>> Testimonials()
    {
        var reviews = _db.Reviews.Include(r => r.Customer).Include(r => r.Booking)
            .Where(r => r.Rating == 5 && r.Comment != null)
            .OrderByDescending(r => r.CreatedAt).Take(3).ToList()
            .Select(r => new Dictionary<string, object?>
            {
                ["first"] = r.Customer.FirstName,
                ["last"] = r.Customer.LastName,
                ["line"] = r.Booking.PickupCity == r.Booking.DropoffCity
                    ? r.Booking.PickupCity
                    : r.Booking.PickupCity + " → " + r.Booking.DropoffCity,
                ["stars"] = (double)r.Rating,
                ["text"] = r.Comment,
            }).ToList();
        return reviews.Concat(FALLBACK_TESTIMONIALS.Skip(reviews.Count)).ToList();   
    }
}
