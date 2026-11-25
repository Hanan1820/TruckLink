
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TruckLink.Data;
using TruckLink.Models;
using TruckLink.Support.Exceptions;
using TruckLink.Support.Helpers;
using TruckLink.Support.Middleware;
using TruckLink.Support.Services;
using TruckLink.ViewModels.Admin;

namespace TruckLink.Controllers.Admin;


[Route("admin"), TypeFilter(typeof(Authenticate), Arguments = new object[] { "admin" })]
public class VehicleController : Controller
{
    public const int PER_PAGE = 7;

    public static readonly Dictionary<string, string> SORTS = new() { ["plate"] = "Plate A–Z", ["newest"] = "Newest first", ["capacity"] = "Largest first" };

    private readonly BookingQueue _queue;
    private readonly AppDbContext _db;

    public VehicleController(BookingQueue queue, AppDbContext db) { _queue = queue; _db = db; }

    public class Filters
    {
        public string? Type { get; set; }
        public string? Status { get; set; }
        public string Sort { get; set; } = "plate";
        public string Q { get; set; } = "";
        public List<int> Ids { get; set; } = new();
    }

    
    [HttpGet("vehicles", Name = "admin.vehicles.index")]
    public IActionResult Index(string? type, string? status, string? sort, string? q, string? ids, int page = 1)
    {
        var filters = MakeFilters(type, status, sort, q, ids);
        var byType = _db.Vehicles.GroupBy(v => v.VehicleType).Select(g => new { g.Key, N = g.Count() }).ToDictionary(x => x.Key, x => x.N);
        var byStatus = _db.Vehicles.GroupBy(v => v.VehicleStatus).Select(g => new { g.Key, N = g.Count() }).ToDictionary(x => x.Key, x => x.N);

        return View("~/Views/Admin/Vehicles/Index.cshtml", new
        {
            vehicles = Paginator<Vehicle>.Paginate(Query(filters), page, PER_PAGE),
            filters,
            byType,
            total = byType.Values.Sum(),
            kpis = new
            {
                available = byStatus.GetValueOrDefault("Available"),
                onJob = byStatus.GetValueOrDefault("On Job"),
                @out = byStatus.GetValueOrDefault("Maintenance") + byStatus.GetValueOrDefault("Out of Service"),
            },
            
            assigned = _db.Bookings.Where(b => BookingAcceptance.HOLDING.Contains(b.BookingStatus) && b.VehicleId != null).Select(b => b.VehicleId).Distinct().Count(),
            highlight = TempData["vehicle_saved"],
        });
    }

    [HttpGet("vehicles/export", Name = "admin.vehicles.export")]
    public IActionResult Export(string? type, string? status, string? sort, string? q, string? ids)
    {
        var rows = Query(MakeFilters(type, status, sort, q, ids)).ToList();
        string Csv(object? v) => "\"" + Convert.ToString(v, System.Globalization.CultureInfo.InvariantCulture)?.Replace("\"", "\"\"") + "\"";
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(",", new[] { "Record", "Plate number", "Vehicle type", "Capacity (m3)", "Max weight (kg)", "Temp-controlled", "Status", "Registered" }.Select(Csv)));
        foreach (var v in rows)
            sb.AppendLine(string.Join(",", new object?[] { v.Code, v.PlateNumber, v.VehicleType, v.CapacityVolumeM3, v.MaxWeightKg,
                v.IsTemperatureControlled ? "Yes" : "No", v.VehicleStatus, v.CreatedAt.ToString("yyyy-MM-dd") }.Select(Csv)));
        return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv; charset=UTF-8", "trucklink-fleet-" + DateTime.Now.ToString("yyyyMMdd-HHmm") + ".csv");
    }

    
    [HttpGet("vehicles/create", Name = "admin.vehicles.create")]
    public IActionResult Create() =>
        View("~/Views/Admin/Vehicles/Create.cshtml", new { vehicle = new Vehicle { VehicleStatus = "Available", IsTemperatureControlled = false } });

    [HttpPost("vehicles", Name = "admin.vehicles.store"), ValidateAntiForgeryToken]
    public IActionResult Store(VehicleRequest request)
    {
        if (!ModelState.IsValid) return BackWithErrors();
        var vehicle = new Vehicle { AdminId = AuthManager.Admin()!.AdminId };
        Fill(vehicle, request);
        _db.Vehicles.Add(vehicle);
        _db.SaveChanges();

        TempData["vehicle_saved"] = vehicle.VehicleId;
        TempData["vehicle_created"] = true;
        return RedirectToRoute("admin.vehicles.show", new { vehicle = vehicle.VehicleId });
    }

    
    [HttpGet("vehicles/{vehicle:int}", Name = "admin.vehicles.show")]
    public IActionResult Show(int vehicle)
    {
        var model = _db.Vehicles.Include(v => v.Admin).FirstOrDefault(v => v.VehicleId == vehicle);
        if (model == null) return NotFound();
        var now = DateTime.Now;
        var from = new DateTime(now.Year, now.Month, 1); var to = from.AddMonths(1).AddTicks(-1);
        var month = _db.Bookings.Where(b => b.VehicleId == model.VehicleId && b.BookingStatus == "Completed" && b.PickupDatetime >= from && b.PickupDatetime <= to).ToList();

        return View("~/Views/Admin/Vehicles/Show.cshtml", new
        {
            vehicle = model,
            upcoming = _queue.UpcomingFor(model),
            month = new
            {
                trips = month.Count,
                km = month.Sum(b => (double)(b.DistanceKm ?? 0)),
                volume = month.Sum(b => (double)(b.TotalVolumeM3 ?? 0)),
                revenue = month.Sum(b => (double)(b.TransportFee ?? 0)),
                
                utilisation = month.Count > 0 && model.CapacityVolumeM3 > 0
                    ? (int)Math.Round(month.Average(b => (double)(b.TotalVolumeM3 ?? 0)) / (double)model.CapacityVolumeM3 * 100, MidpointRounding.AwayFromZero) : 0,
            },
        }, DeleteGuard(model));
    }

    
    [HttpGet("vehicles/{vehicle:int}/edit", Name = "admin.vehicles.edit")]
    public IActionResult Edit(int vehicle)
    {
        var model = _db.Vehicles.Find(vehicle);
        return model == null ? NotFound() : View("~/Views/Admin/Vehicles/Edit.cshtml", new { vehicle = model, upcoming = _queue.UpcomingFor(model) });
    }

    [HttpPut("vehicles/{vehicle:int}", Name = "admin.vehicles.update"), ValidateAntiForgeryToken]
    public IActionResult Update(int vehicle, VehicleRequest request)
    {
        var model = _db.Vehicles.Find(vehicle);
        if (model == null) return NotFound();
        request.IgnoreVehicleId = model.VehicleId;   
        if (!ModelState.IsValid) return BackWithErrors();

        List<Booking> returned;
        using (var tx = _db.Database.BeginTransaction())   
        {
            Fill(model, request);
            _db.SaveChanges();
            
            returned = model.VehicleStatus == "Available" ? new List<Booking>() : _queue.ReturnToQueue(model, AuthManager.Admin()!.AdminId);
            tx.Commit();
        }

        TempData["vehicle_saved"] = model.VehicleId;
        TempData["returned"] = string.Join(",", returned.Select(b => b.Reference));
        return RedirectToRoute("admin.vehicles.show", new { vehicle = model.VehicleId });
    }

    
    [HttpPost("vehicles/{vehicle:int}/out-of-service", Name = "admin.vehicles.out-of-service"), ValidateAntiForgeryToken]
    public IActionResult OutOfService(int vehicle)
    {
        var model = _db.Vehicles.Find(vehicle);
        if (model == null) return NotFound();

        List<Booking> returned;
        using (var tx = _db.Database.BeginTransaction())
        {
            model.VehicleStatus = "Out of Service";
            _db.SaveChanges();
            returned = _queue.ReturnToQueue(model, AuthManager.Admin()!.AdminId);
            tx.Commit();
        }

        TempData["vehicle_saved"] = model.VehicleId;
        TempData["returned"] = string.Join(",", returned.Select(b => b.Reference));
        return RedirectToRoute("admin.vehicles.show", new { vehicle = model.VehicleId });
    }

    
    [HttpDelete("vehicles/{vehicle:int}", Name = "admin.vehicles.destroy"), ValidateAntiForgeryToken]
    public IActionResult Destroy(int vehicle, [FromForm(Name = "confirm_plate")] string? confirmPlate)
    {
        var model = _db.Vehicles.Find(vehicle);
        if (model == null) return NotFound();

        if (string.IsNullOrWhiteSpace(confirmPlate)) throw ValidationException.WithMessages("confirm_plate", "Type the plate number exactly to confirm.");
        if (confirmPlate.Trim().ToUpperInvariant() != model.PlateNumber)
            throw ValidationException.WithMessages("confirm_plate", "Type the plate number exactly to confirm.");
        if (_db.Bookings.Any(b => b.VehicleId == model.VehicleId))
            throw ValidationException.WithMessages("confirm_plate", model.PlateNumber + " is used by bookings and can only be set Out of Service.");

        var plate = model.PlateNumber;
        _db.Vehicles.Remove(model);
        _db.SaveChanges();

        With("status", "Vehicle " + plate + " was deleted.");
        return RedirectToRoute("admin.vehicles.index");
    }

    
    private static void Fill(Vehicle v, VehicleRequest r)
    {
        v.PlateNumber = r.PlateNumber; v.VehicleType = r.VehicleType; v.CapacityVolumeM3 = r.CapacityVolumeM3!.Value;
        v.MaxWeightKg = r.MaxWeightKg!.Value; v.IsTemperatureControlled = r.IsTemperatureControlled!.Value; v.VehicleStatus = r.VehicleStatus;
    }

    
    private Dictionary<string, object?> DeleteGuard(Vehicle vehicle)
    {
        var counts = _db.Bookings.Where(b => b.VehicleId == vehicle.VehicleId).GroupBy(b => b.BookingStatus)
            .Select(g => new { g.Key, N = g.Count() }).ToDictionary(x => x.Key, x => x.N);
        return new()
        {
            ["references"] = counts.Values.Sum(),
            ["acceptedCount"] = counts.GetValueOrDefault("Accepted"),
            ["completedCount"] = counts.GetValueOrDefault("Completed"),
        };
    }

    private Filters MakeFilters(string? type, string? status, string? sort, string? q, string? ids) => new()
    {
        Type = Vehicle.TYPES.Contains(type) ? type : null,
        Status = Vehicle.STATUSES.Contains(status) ? status : null,
        Sort = sort != null && SORTS.ContainsKey(sort) ? sort : "plate",
        Q = (q ?? "").Trim(),
        Ids = (ids ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => int.TryParse(s, out var n) ? n : 0).Where(n => n != 0).ToList(),
    };

    private IQueryable<Vehicle> Query(Filters f)
    {
        var query = _db.Vehicles.AsQueryable();
        if (f.Type != null) query = query.Where(v => v.VehicleType == f.Type);
        if (f.Status != null) query = query.Where(v => v.VehicleStatus == f.Status);
        if (f.Q != "") query = query.Where(v => EF.Functions.Like(v.PlateNumber, "%" + f.Q + "%") || EF.Functions.Like(v.VehicleType, "%" + f.Q + "%") || EF.Functions.Like(v.VehicleStatus, "%" + f.Q + "%"));
        if (f.Ids.Count > 0) query = query.Where(v => f.Ids.Contains(v.VehicleId));
        return f.Sort switch
        {
            "newest" => query.OrderByDescending(v => v.CreatedAt).ThenByDescending(v => v.VehicleId),
            "capacity" => query.OrderByDescending(v => v.CapacityVolumeM3).ThenBy(v => v.PlateNumber),
            _ => query.OrderBy(v => v.PlateNumber),
        };
    }
}
