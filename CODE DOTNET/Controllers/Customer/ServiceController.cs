
using Microsoft.AspNetCore.Mvc;
using TruckLink.Data;
using TruckLink.Models;
using TruckLink.Support.Middleware;

namespace TruckLink.Controllers.Customer;


[Route("customer"), TypeFilter(typeof(Authenticate), Arguments = new object[] { "customer" })]
public class ServiceController : Controller
{
    private readonly AppDbContext _db;
    public ServiceController(AppDbContext db) => _db = db;

    [HttpGet("services", Name = "customer.services")]
    public IActionResult Index(string? type)
    {
        type = Service.TYPES.Contains(type) ? type : null;
        var active = Service.ScopeActive(_db.Services);

        var q = type != null ? active.Where(s => s.ServiceType == type) : active;   
        return View("~/Views/Customer/Services.cshtml", new
        {
            services = q.OrderBy(s => s.ServiceType == "Transport" ? 1 : s.ServiceType == "Value-Added" ? 2 : 3).ThenBy(s => s.ServiceId).ToList(),
            counts = active.GroupBy(s => s.ServiceType).Select(g => new { g.Key, N = g.Count() }).ToDictionary(x => x.Key, x => x.N),
            type,
        });
    }
}
