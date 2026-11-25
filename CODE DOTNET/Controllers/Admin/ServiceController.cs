
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TruckLink.Controllers.Concerns;
using TruckLink.Data;
using TruckLink.Models;
using TruckLink.Support.Helpers;
using TruckLink.Support.Middleware;
using TruckLink.ViewModels.Admin;

namespace TruckLink.Controllers.Admin;


[Route("admin"), TypeFilter(typeof(Authenticate), Arguments = new object[] { "admin" })]
public class ServiceController : ConfirmsDeletion
{
    public static readonly Dictionary<string, string> TYPE_ICONS = new() { ["Transport"] = "bi-truck", ["Storage"] = "bi-buildings", ["Value-Added"] = "bi-stars" };
    public static readonly Dictionary<string, string[]> STATUS_ICONS = new() { ["1"] = new[] { "bi-check-circle", "--ok" }, ["0"] = new[] { "bi-x-circle", "--dg" } };
    public static readonly Dictionary<string, string> STATUS_LABELS = new() { ["1"] = "Active", ["0"] = "Inactive" };
    public static readonly Dictionary<string, string> SORTS = new() { ["type"] = "Type", ["name"] = "Name A–Z", ["price"] = "Highest price", ["used"] = "Most booked" };

    
    public static readonly string[] EARNING = { "Accepted", "Picked Up", "In Transit", "In Storage", "Out for Delivery", "Completed" };

    private readonly AppDbContext _db;
    public ServiceController(AppDbContext db) => _db = db;

    [HttpGet("services", Name = "admin.services.index")]
    public IActionResult Index(string? type, string? status, string? sort, string? q, int page = 1)
    {
        var f = new
        {
            type = Models.Service.TYPES.Contains(type) ? type : null,
            status = status is "1" or "0" ? status : null,
            sort = sort != null && SORTS.ContainsKey(sort) ? sort : "type",
            q = (q ?? "").Trim(),
        };
        var byType = _db.Services.GroupBy(s => s.ServiceType).Select(g => new { g.Key, N = g.Count() }).ToDictionary(x => x.Key, x => x.N);
        var lines = _db.BookingServices.Where(l => EARNING.Contains(l.Booking.BookingStatus));

        var query = _db.Services.AsQueryable();
        if (f.type != null) query = query.Where(s => s.ServiceType == f.type);
        if (f.status != null) { var active = f.status == "1"; query = query.Where(s => s.IsActive == active); }
        if (f.q != "") query = query.Where(s => EF.Functions.Like(s.ServiceName, "%" + f.q + "%") || EF.Functions.Like(s.ServiceType, "%" + f.q + "%") || EF.Functions.Like(s.Description, "%" + f.q + "%"));
        query = f.sort switch
        {
            "name" => query.OrderBy(s => s.ServiceName),
            "price" => query.OrderByDescending(s => s.BasePrice).ThenBy(s => s.ServiceName),
            "used" => query.OrderByDescending(s => s.BookingServices.Count).ThenBy(s => s.ServiceName),   
            _ => query.OrderBy(s => s.ServiceType == "Transport" ? 1 : s.ServiceType == "Storage" ? 2 : 3).ThenBy(s => s.ServiceId),
        };
        var services = Paginator<Models.Service>.Paginate(query, page, 12);

        return View("~/Views/Admin/Services/Index.cshtml", new
        {
            services,
            bookingCounts = services.Items.ToDictionary(s => s.ServiceId, s => _db.BookingServices.Count(l => l.ServiceId == s.ServiceId)),   
            byType,
            total = byType.Values.Sum(),
            active = Models.Service.ScopeActive(_db.Services).Count(),
            booked = lines.Count(),
            revenue = (double)lines.Sum(l => (decimal?)l.Subtotal ?? 0m),
            highlight = TempData["saved.id"],
        });
    }

    [HttpGet("services/create", Name = "admin.services.create")]
    public IActionResult Create() => View("~/Views/Admin/Services/Create.cshtml", new { service = new Models.Service { IsActive = true } });

    [HttpPost("services", Name = "admin.services.store"), ValidateAntiForgeryToken]
    public IActionResult Store(ServiceRequest request)
    {
        if (!ModelState.IsValid) return BackWithErrors();
        var d = request.Service();
        var service = new Models.Service
        {
            ServiceName = (string)d["service_name"]!, ServiceType = (string)d["service_type"]!, BasePrice = (decimal)d["base_price"]!,
            PricingUnit = (string)d["pricing_unit"]!, Description = d["description"] as string, IsActive = (bool)d["is_active"]!,
            AdminId = AuthManager.Admin()!.AdminId,
        };
        _db.Services.Add(service);
        _db.SaveChanges();

        Saved("service:" + service.ServiceId, service.ServiceId, created: true);
        return RedirectToRoute("admin.services.show", new { service = service.ServiceId });
    }

    [HttpGet("services/{service:int}", Name = "admin.services.show")]
    public IActionResult Show(int service)
    {
        var model = _db.Services.Include(s => s.Admin).FirstOrDefault(s => s.ServiceId == service);
        if (model == null) return NotFound();
        var lines = _db.BookingServices.Where(l => l.ServiceId == model.ServiceId);
        var now = DateTime.Now;
        var from = new DateTime(now.Year, now.Month, 1); var to = from.AddMonths(1).AddTicks(-1);
        var month = lines.Where(l => EARNING.Contains(l.Booking.BookingStatus) && l.Booking.PickupDatetime >= from && l.Booking.PickupDatetime <= to);

        return View("~/Views/Admin/Services/Show.cshtml", new
        {
            service = model,
            recent = lines.Include(l => l.Booking).ThenInclude(b => b.Customer).OrderByDescending(l => l.BookingServiceId).Take(8).ToList(),
            month = new { lines = month.Count(), quantity = month.Sum(l => (int?)l.Quantity) ?? 0, revenue = (double)(month.Sum(l => (decimal?)l.Subtotal) ?? 0m) },
            total = new { lines = lines.Count(), revenue = (double)(lines.Where(l => EARNING.Contains(l.Booking.BookingStatus)).Sum(l => (decimal?)l.Subtotal) ?? 0m) },
        }, DeleteGuard(model));
    }

    [HttpGet("services/{service:int}/edit", Name = "admin.services.edit")]
    public IActionResult Edit(int service)
    {
        var model = _db.Services.Find(service);
        return model == null ? NotFound() : View("~/Views/Admin/Services/Edit.cshtml", new { service = model, open = OpenBookings(model) });
    }

    [HttpPut("services/{service:int}", Name = "admin.services.update"), ValidateAntiForgeryToken]
    public IActionResult Update(int service, ServiceRequest request)
    {
        var model = _db.Services.Find(service);
        if (model == null) return NotFound();
        request.IgnoreServiceId = model.ServiceId;
        if (!ModelState.IsValid) return BackWithErrors();

        var d = request.Service();
        model.ServiceName = (string)d["service_name"]!; model.ServiceType = (string)d["service_type"]!; model.BasePrice = (decimal)d["base_price"]!;
        model.PricingUnit = (string)d["pricing_unit"]!; model.Description = d["description"] as string; model.IsActive = (bool)d["is_active"]!;
        _db.SaveChanges();

        Saved("service:" + model.ServiceId, model.ServiceId, created: false);
        return RedirectToRoute("admin.services.show", new { service = model.ServiceId });
    }

    
    [HttpPost("services/{service:int}/deactivate", Name = "admin.services.deactivate"), ValidateAntiForgeryToken]
    public IActionResult Deactivate(int service)
    {
        var model = _db.Services.Find(service);
        if (model == null) return NotFound();
        model.IsActive = false;
        _db.SaveChanges();

        With("status", model.ServiceName + " is now Inactive and is no longer offered to customers.");
        return RedirectToRoute("admin.services.show", new { service = model.ServiceId });
    }

    [HttpDelete("services/{service:int}", Name = "admin.services.destroy"), ValidateAntiForgeryToken]
    public IActionResult Destroy(int service, [FromForm(Name = "confirm")] string? confirm)
    {
        var model = _db.Services.Find(service);
        if (model == null) return NotFound();

        ConfirmDeletion(confirm, model.ServiceName, "service name");
        if (((List<string>)DeleteGuard(model)["uses"]!).Count > 0)
            RefuseDeletion(model.ServiceName + " is used by bookings and can only be deactivated.");

        var name = model.ServiceName;
        _db.Services.Remove(model);
        _db.SaveChanges();

        With("status", "Service " + name + " was deleted.");
        return RedirectToRoute("admin.services.index");
    }

    
    private int OpenBookings(Models.Service service)
    {
        var open = new[] { "Pending", "Accepted", "Picked Up", "In Transit", "In Storage", "Out for Delivery" };
        return _db.BookingServices.Count(l => l.ServiceId == service.ServiceId && open.Contains(l.Booking.BookingStatus));
    }

    private Dictionary<string, object?> DeleteGuard(Models.Service service)
    {
        var lines = _db.BookingServices.Count(l => l.ServiceId == service.ServiceId);
        return new() { ["uses"] = lines > 0 ? new List<string> { "is a line in <b>" + lines + " " + Helpers.Plural("booking", lines) + "</b>" } : new List<string>() };
    }
}
