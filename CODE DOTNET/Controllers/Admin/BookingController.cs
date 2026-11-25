
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TruckLink.Data;
using TruckLink.Models;
using TruckLink.Support.Helpers;
using TruckLink.Support.Middleware;
using TruckLink.Support.Services;
using TruckLink.ViewModels.Admin;

namespace TruckLink.Controllers.Admin;


[Route("admin/bookings"), TypeFilter(typeof(Authenticate), Arguments = new object[] { "admin" })]   
public class BookingController : Controller
{
    
    public static readonly List<(string Key, string Label, string[] Statuses)> CHIPS = new()
    {
        ("pending", "Pending", new[] { "Pending" }),
        ("accepted", "Accepted", new[] { "Accepted" }),
        ("in-transit", "In Transit", new[] { "Picked Up", "In Transit", "Out for Delivery" }),
        ("completed", "Completed", new[] { "Completed" }),
        ("rejected", "Rejected", new[] { "Rejected" }),
    };

    public const int PER_PAGE = 6;
    private const string SEEN = "admin_seen_bookings";

    private readonly BookingAcceptance _acceptance;
    private readonly BookingRejection _rejection;
    private readonly AppDbContext _db;
    private readonly SessionStore _session;
    private readonly IConfiguration _config;

    public BookingController(BookingAcceptance acceptance, BookingRejection rejection, AppDbContext db, SessionStore session, IConfiguration config)
    {
        _acceptance = acceptance; _rejection = rejection; _db = db; _session = session; _config = config;
    }

    
    public class Filters
    {
        public string? Status { get; set; }
        public string Q { get; set; } = "";
        public string? Month { get; set; }
        public string? City { get; set; }
        public bool Newest { get; set; }
        public List<int> Ids { get; set; } = new();
    }

    
    [HttpGet("", Name = "admin.bookings.index")]
    public IActionResult Index(string? status, string? q, string? month, string? city, string? sort, string? ids, int page = 1)
    {
        var filters = MakeFilters(status, q, month, city, sort, ids);
        var bookings = Paginator<Booking>.Paginate(Query(filters).Include(b => b.Customer), page, PER_PAGE);
        var counts = _db.Bookings.GroupBy(b => b.BookingStatus).Select(g => new { g.Key, N = g.Count() }).ToDictionary(x => x.Key, x => x.N);

        
        var seen = _session.Get<List<int>>(SEEN) ?? new List<int>();
        var newest = Booking.ScopePending(_db.Bookings).Where(b => !seen.Contains(b.BookingId)).OrderByDescending(b => b.CreatedAt)
            .Select(b => (int?)b.BookingId).FirstOrDefault();

        return View("~/Views/Admin/Bookings/Index.cshtml", new
        {
            bookings,
            filters,
            chipCounts = CHIPS.ToDictionary(c => c.Key, c => c.Statuses.Sum(s => counts.GetValueOrDefault(s, 0))),
            total = counts.Values.Sum(),
            months = _db.Bookings.Select(b => b.PickupDatetime).ToList().Select(d => d.ToString("yyyy-MM")).Distinct().OrderByDescending(m => m).ToList(),   
            cities = _db.Bookings.Select(b => b.PickupCity).Distinct().OrderBy(c => c).ToList(),
            newest,
        });
    }

    
    [HttpGet("export", Name = "admin.bookings.export")]
    public IActionResult Export(string? status, string? q, string? month, string? city, string? sort, string? ids)
    {
        var rows = Query(MakeFilters(status, q, month, city, sort, ids)).Include(b => b.Customer).ToList();
        var sb = new StringBuilder();
        string Csv(object? v) => "\"" + Convert.ToString(v, CultureInfo.InvariantCulture)?.Replace("\"", "\"\"") + "\"";
        sb.AppendLine(string.Join(",", new[] { "Booking", "Submitted", "Customer", "Route", "Pickup", "Volume (m3)", "Recommended", "Total (SAR)", "Status" }.Select(Csv)));
        foreach (var b in rows)
            sb.AppendLine(string.Join(",", new object?[] { b.Reference, b.CreatedAt.ToString("yyyy-MM-dd HH:mm"), b.Customer.FullName,
                b.PickupCity + " → " + b.DropoffCity, b.PickupDatetime.ToString("yyyy-MM-dd HH:mm"), b.TotalVolumeM3, b.RecommendedVehicleType, b.TotalAmount, b.BookingStatus }.Select(Csv)));
        return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv; charset=UTF-8", "trucklink-bookings-" + DateTime.Now.ToString("yyyyMMdd-HHmm") + ".csv");
    }

    
    [HttpGet("{booking:int}", Name = "admin.bookings.show")]
    public IActionResult Show(int booking, [FromServices] VehicleRecommender recommender)
    {
        var model = _db.Bookings.Find(booking);
        if (model == null) return NotFound();

        var seen = _session.Get<List<int>>(SEEN) ?? new List<int>();   
        seen.Add(model.BookingId);
        _session.Put(SEEN, seen);

        _db.Entry(model).Reference(b => b.Customer).Load();
        _db.Entry(model).Collection(b => b.Items).Query().Include(i => i.Category).Include(i => i.Photos).Load();
        _db.Entry(model).Collection(b => b.BookingServices).Query().Include(l => l.Service).Load();
        _db.Entry(model).Collection(b => b.Payments).Load();
        _db.Entry(model).Reference(b => b.Vehicle).Load();
        _db.Entry(model).Reference(b => b.Driver).Load();
        _db.Entry(model).Reference(b => b.StorageRequest).Load();
        if (model.StorageRequest != null) _db.Entry(model.StorageRequest).Reference(s => s.Warehouse).Load();

        var customer = model.Customer;
        var type = model.RecommendedVehicleType;
        var vehicleOptions = type != null ? _acceptance.VehicleOptions(model) : new List<VehicleOption>();

        var data = new Dictionary<string, object?>
        {
            ["booking"] = model,
            ["history"] = new
            {
                completed = _db.Bookings.Count(b => b.CustomerId == customer.CustomerId && b.BookingStatus == "Completed"),
                cancelled = _db.Bookings.Count(b => b.CustomerId == customer.CustomerId && b.BookingStatus == "Cancelled"),
                rating = _db.Reviews.Where(r => r.CustomerId == customer.CustomerId).Average(r => (double?)r.Rating),
                bookings = _db.Bookings.Where(b => b.CustomerId == customer.CustomerId).OrderByDescending(b => b.CreatedAt).ToList(),
            },
            ["capacity"] = type != null && recommender.Capacities().TryGetValue(type, out var c) ? (double?)c.Capacity : null,
            ["load"] = type != null ? recommender.LoadPercent(type, (double)(model.TotalVolumeM3 ?? 0)) : (int?)null,
            ["available"] = new[] { vehicleOptions.Count(o => o.Reason == null), vehicleOptions.Count },
        };

        if (model.IsPending())
        {
            var alternatives = _rejection.AlternativeDates(model);
            data["vehicleOptions"] = vehicleOptions;
            data["driverOptions"] = _acceptance.DriverOptions(model);
            data["moverOptions"] = _acceptance.MoverOptions(model);
            data["crewSize"] = _acceptance.RecommendedCrew(model);
            data["defaultCrew"] = _acceptance.DefaultCrew(model);
            data["withAssembly"] = _acceptance.WithAssembly(model);
            data["warehouse"] = _acceptance.HasStorage(model) ? _acceptance.WarehouseFor(model) : null;
            data["draft"] = _session.Get<Dictionary<string, object?>>("accept_draft." + model.BookingId) ?? new();
            data["reasons"] = BookingRejection.REASONS;
            data["alternatives"] = alternatives;
            data["alternativesText"] = _rejection.DatesText(alternatives);
            data["defaultMessage"] = _rejection.DefaultMessage(model, alternatives);
        }
        return View("~/Views/Admin/Bookings/Show.cshtml", data);
    }

    
    [HttpPost("{booking:int}/accept", Name = "admin.bookings.accept"), ValidateAntiForgeryToken]
    public IActionResult Accept(int booking, AcceptBookingRequest request, [FromForm(Name = "intent")] string? intent)
    {
        var model = _db.Bookings.Find(booking);
        if (model == null) return NotFound();
        if (!ModelState.IsValid) return BackWithErrors();

        if (intent == "draft")
        {
            _session.Put("accept_draft." + model.BookingId, new Dictionary<string, object?>
                { ["vehicle_id"] = request.VehicleId, ["driver_id"] = request.DriverId, ["crew"] = request.Crew });   
            With("status", "Draft saved. Nothing has been sent yet.");
            return RedirectToRoute("admin.bookings.show", new { booking = model.BookingId });
        }

        var crewIds = request.Crew ?? new List<int>();
        var movers = _db.Staff.Where(s => crewIds.Contains(s.StaffId)).ToList();
        var sent = _acceptance.Accept(model, _db.Vehicles.First(v => v.VehicleId == request.VehicleId),   
            _db.Staff.First(s => s.StaffId == request.DriverId), movers, AuthManager.Admin()!);
        _session.Forget("accept_draft." + model.BookingId);

        
        TempData["accepted_crew"] = string.Join(",", movers.Select(m => m.StaffId));
        TempData["accepted_notifications"] = string.Join(",", sent.Select(a => a.AnnouncementId));
        return RedirectToRoute("admin.bookings.accepted", new { booking = model.BookingId });
    }

    
    [HttpPost("{booking:int}/reject", Name = "admin.bookings.reject"), ValidateAntiForgeryToken]
    public IActionResult Reject(int booking, RejectBookingRequest request)
    {
        var model = _db.Bookings.Include(b => b.Customer).FirstOrDefault(b => b.BookingId == booking);
        if (model == null) return NotFound();
        if (!ModelState.IsValid) return BackWithErrors();

        _rejection.Reject(model, request.RejectionReason, request.CustomerMessage.Trim(), request.OfferDates == true, AuthManager.Admin()!);
        With("status", model.Reference + " is rejected and " + model.Customer.FullName + " has been notified.");
        return RedirectToRoute("admin.bookings.index");
    }

    
    [HttpGet("{booking:int}/accepted", Name = "admin.bookings.accepted")]
    public IActionResult Accepted(int booking)
    {
        var model = _db.Bookings.Find(booking);
        if (model == null) return NotFound();
        if (model.BookingStatus != "Accepted") return RedirectToRoute("admin.bookings.show", new { booking = model.BookingId });

        _db.Entry(model).Reference(b => b.Customer).Load();
        _db.Entry(model).Reference(b => b.Vehicle).Load();
        _db.Entry(model).Reference(b => b.Driver).Load();
        _db.Entry(model).Reference(b => b.StorageRequest).Load();
        if (model.StorageRequest != null)
        {
            _db.Entry(model.StorageRequest).Reference(s => s.Warehouse).Load();
            _db.Entry(model.StorageRequest.Warehouse).Reference(w => w.Keeper).Load();
        }

        var crewIds = ParseIds(TempData["accepted_crew"] as string);
        var notificationIds = TempData["accepted_notifications"] as string;
        var ids = ParseIds(notificationIds);
        var notifications = notificationIds != null && ids.Count > 0
            ? _db.Announcements.Include(a => a.Customer).Include(a => a.Staff).Where(a => ids.Contains(a.AnnouncementId)).OrderBy(a => a.AnnouncementId).ToList()
            : _db.Announcements.Include(a => a.Customer).Include(a => a.Staff)
                .Where(a => (a.AnnouncementType == "Booking Accepted" || a.AnnouncementType == "Job Assigned") && EF.Functions.Like(a.Content, model.Reference + "%"))
                .OrderBy(a => a.AnnouncementId).ToList();

        return View("~/Views/Admin/Bookings/Accepted.cshtml", new
        {
            booking = model,
            crew = _db.Staff.Where(s => crewIds.Contains(s.StaffId)).ToList(),
            justAccepted = notificationIds != null,
            notifications,
            next = Booking.ScopePending(_db.Bookings).OrderBy(b => b.CreatedAt).ThenBy(b => b.BookingId).FirstOrDefault(),
        });
    }

    private static List<int> ParseIds(string? csv) =>
        (csv ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();

    
    private Filters MakeFilters(string? status, string? q, string? month, string? city, string? sort, string? ids)
    {
        q = (q ?? "").Trim();
        var cities = _config.GetSection("TruckLink:Cities").Get<string[]>() ?? Array.Empty<string>();
        return new Filters
        {
            Status = status != null && CHIPS.Any(c => c.Key == status) ? status : (q != "" ? null : "pending"),
            Q = q,
            Month = Regex.IsMatch(month ?? "", @"^\d{4}-\d{2}$") ? month : null,
            City = cities.Contains(city) ? city : null,
            Newest = sort == "newest",
            
            Ids = (ids ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => int.TryParse(s, out var n) ? n : 0).Where(n => n != 0).ToList(),
        };
    }

    private IQueryable<Booking> Query(Filters f)
    {
        var query = _db.Bookings.AsQueryable();
        if (f.Status != null) { var statuses = CHIPS.First(c => c.Key == f.Status).Statuses; query = query.Where(b => statuses.Contains(b.BookingStatus)); }
        if (f.Q != "")
        {
            var term = f.Q;
            var digits = Regex.Replace(Regex.Replace(term, @"^BK-\d{4}-", "", RegexOptions.IgnoreCase), @"\D", "");
            var number = digits == "" ? 0 : int.Parse(digits);
            query = query.Where(b => EF.Functions.Like(b.PickupCity, "%" + term + "%") || EF.Functions.Like(b.DropoffCity, "%" + term + "%")
                || EF.Functions.Like(b.Customer.FirstName + " " + b.Customer.LastName, "%" + term + "%")
                || (b.Vehicle != null && EF.Functions.Like(b.Vehicle.PlateNumber, "%" + term + "%"))
                || (number != 0 && b.BookingId == number));
        }
        if (f.Month != null)
        {
            var y = int.Parse(f.Month[..4]); var m = int.Parse(f.Month.Substring(5, 2));
            query = query.Where(b => b.PickupDatetime.Year == y && b.PickupDatetime.Month == m);
        }
        if (f.City != null) query = query.Where(b => b.PickupCity == f.City);
        if (f.Ids.Count > 0) query = query.Where(b => f.Ids.Contains(b.BookingId));
        return f.Newest ? query.OrderByDescending(b => b.CreatedAt).ThenByDescending(b => b.BookingId) : query.OrderBy(b => b.CreatedAt).ThenBy(b => b.BookingId);
    }
}
