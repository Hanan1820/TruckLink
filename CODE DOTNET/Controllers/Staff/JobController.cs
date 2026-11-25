
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TruckLink.Data;
using TruckLink.Models;
using TruckLink.Support.Exceptions;
using TruckLink.Support.Helpers;
using TruckLink.Support.Middleware;
using TruckLink.Support.Services;

namespace TruckLink.Controllers.Staff;


[Route("staff"), TypeFilter(typeof(Authenticate), Arguments = new object[] { "staff" })]   
public class JobController : Controller
{
    public static readonly List<(string Key, string Label, string[] Statuses)> CHIPS = new()
    {
        ("upcoming", "Upcoming", new[] { "Accepted" }),
        ("active", "In progress", new[] { "Picked Up", "In Transit", "In Storage", "Out for Delivery" }),
        ("completed", "Completed", new[] { "Completed" }),
    };

    private readonly JobWorkflow _jobs;
    private readonly AppDbContext _db;

    public JobController(JobWorkflow jobs, AppDbContext db) { _jobs = jobs; _db = db; }

    [HttpGet("jobs", Name = "staff.jobs.index")]
    public IActionResult Index(string? status, string? q, int page = 1)
    {
        var staff = AuthManager.Staff()!;
        var chip = CHIPS.Any(c => c.Key == status) ? status : null;
        q = (q ?? "").Trim();
        var @base = _jobs.JobsQuery(staff);

        var counts = CHIPS.ToDictionary(c => c.Key, c => @base.Count(b => c.Statuses.Contains(b.BookingStatus)));
        
        var statuses = chip != null ? CHIPS.First(c => c.Key == chip).Statuses : CHIPS[0].Statuses.Concat(CHIPS[1].Statuses).ToArray();

        var query = @base.Include(b => b.Customer).Include(b => b.Vehicle).Include(b => b.StorageRequest).ThenInclude(s => s!.Warehouse)
            .Where(b => statuses.Contains(b.BookingStatus));
        if (q != "")
        {
            var digits = Regex.Replace(Regex.Replace(q, @"^BK-\d{4}-", "", RegexOptions.IgnoreCase), @"\D", "");
            var id = digits == "" ? 0 : int.Parse(digits);
            query = query.Where(b => EF.Functions.Like(b.PickupCity, "%" + q + "%") || EF.Functions.Like(b.DropoffCity, "%" + q + "%")
                || EF.Functions.Like(b.Customer.FirstName + " " + b.Customer.LastName, "%" + q + "%")   
                || b.BookingId == id);
        }
        query = chip == "completed" ? query.OrderByDescending(b => b.PickupDatetime) : query.OrderBy(b => b.PickupDatetime);
        var jobs = Paginator<Booking>.Paginate(query, page, 8);

        return View("~/Views/Staff/Jobs/Index.cshtml", new
        {
            jobs,
            counts,
            chip,
            today = @base.Count(b => (b.BookingStatus == "Accepted" || CHIPS[1].Statuses.Contains(b.BookingStatus)) && b.PickupDatetime.Date == DateTime.Today),
            fresh = jobs.Items.Where(b => b.BookingStatus == "Accepted" && !_jobs.IsAcknowledged(staff, b)).Select(b => b.BookingId).ToList(),
        });
    }

    [HttpGet("jobs/{booking:int}", Name = "staff.jobs.show")]
    public IActionResult Show(int booking)
    {
        var staff = AuthManager.Staff()!;
        var model = _db.Bookings.Find(booking);
        if (model == null || !_jobs.CanSee(staff, model)) return NotFound();

        _db.Entry(model).Reference(b => b.Customer).Load();
        _db.Entry(model).Collection(b => b.Items).Query().Include(i => i.Category).Include(i => i.Photos).Load();
        _db.Entry(model).Collection(b => b.BookingServices).Query().Include(l => l.Service).Load();
        _db.Entry(model).Reference(b => b.Vehicle).Load();
        _db.Entry(model).Reference(b => b.Driver).Load();
        _db.Entry(model).Reference(b => b.StorageRequest).Load();
        if (model.StorageRequest != null) _db.Entry(model.StorageRequest).Reference(s => s.Warehouse).Load();
        _db.Entry(model).Reference(b => b.Review).Load();

        var drives = _jobs.Drives(staff, model);
        return View("~/Views/Staff/Jobs/Show.cshtml", new
        {
            booking = model,
            drives,
            next = drives ? model.NextJobStatus() : null,
            acknowledged = _jobs.IsAcknowledged(staff, model),
            crew = _db.Announcements.Include(a => a.Staff)
                .Where(a => a.AnnouncementType == "Job Assigned" && EF.Functions.Like(a.Content, model.Reference + "%") && a.StaffId != null)
                .ToList().Select(a => a.Staff).Where(s => s != null).GroupBy(s => s!.StaffId).Select(g => g.First()).ToList(),
        });
    }

    [HttpPost("jobs/{booking:int}/accept", Name = "staff.jobs.acknowledge"), ValidateAntiForgeryToken]
    public IActionResult Acknowledge(int booking)
    {
        var staff = AuthManager.Staff()!;
        var model = _db.Bookings.Find(booking);
        if (model == null || !_jobs.CanSee(staff, model)) return NotFound();

        _jobs.Acknowledge(staff, model);
        With("status", "Job accepted - see you on " + model.PickupDatetime.ToString("dd MMM", CultureInfo.InvariantCulture) + " at " + model.PickupDatetime.ToString("HH:mm") + ".");
        return RedirectToRoute("staff.jobs.show", new { booking = model.BookingId });
    }

    [HttpPost("jobs/{booking:int}/decline", Name = "staff.jobs.decline"), ValidateAntiForgeryToken]
    public IActionResult Decline(int booking, [FromForm(Name = "reason")] string? reason)
    {
        var staff = AuthManager.Staff()!;
        var model = _db.Bookings.Find(booking);
        if (model == null || !_jobs.Drives(staff, model)) return NotFound();

        
        if (string.IsNullOrWhiteSpace(reason)) ModelState.AddModelError("reason", "The Reason field is required.");
        else if (reason.Length < 10) ModelState.AddModelError("reason", "The Reason must be at least 10 characters.");
        else if (reason.Length > 300) ModelState.AddModelError("reason", "The Reason must not be greater than 300 characters.");
        if (!ModelState.IsValid) return BackWithErrors("decline");

        _jobs.Decline(staff, model, reason!.Trim());
        With("status", model.Reference + " is declined and back with the operations team.");
        return RedirectToRoute("staff.jobs.index");
    }

    [HttpPost("jobs/{booking:int}/status", Name = "staff.jobs.advance"), ValidateAntiForgeryToken]
    public IActionResult Advance(int booking)
    {
        var staff = AuthManager.Staff()!;
        var model = _db.Bookings.Find(booking);
        if (model == null || !_jobs.CanSee(staff, model)) return NotFound();

        var status = _jobs.Advance(staff, model);
        With("status", model.Reference + " is now " + status + ". The customer can see it right away.");
        return RedirectToRoute("staff.jobs.show", new { booking = model.BookingId });
    }

    
    [HttpPost("vehicle-issue", Name = "staff.vehicle-issue"), ValidateAntiForgeryToken]
    public IActionResult VehicleIssue([FromForm(Name = "vehicle_id")] int? vehicleId, [FromForm(Name = "description")] string? description,
        [FromForm(Name = "off_road")] bool offRoad, [FromServices] BookingQueue queue)
    {
        var staff = AuthManager.Staff()!;

        
        if (vehicleId == null) ModelState.AddModelError("vehicle_id", "The Vehicle field is required.");
        if (string.IsNullOrWhiteSpace(description)) ModelState.AddModelError("description", "The What is wrong field is required.");
        else if (description.Length > 500) ModelState.AddModelError("description", "The What is wrong must not be greater than 500 characters.");
        if (!ModelState.IsValid) return BackWithErrors("issue");

        
        var vehicle = _db.Vehicles.FirstOrDefault(v => v.VehicleId == vehicleId && v.Bookings.Any(b => b.StaffId == staff.StaffId));
        if (vehicle == null) return NotFound();

        List<Booking> returned = new();
        using (var tx = _db.Database.BeginTransaction())   
        {
            _db.Announcements.Add(new Announcement
            {
                AdminId = null, CustomerId = null, StaffId = null, AnnouncementType = "Status Update",
                Title = "Vehicle issue · " + vehicle.PlateNumber + " · " + staff.FullName, Content = description!.Trim(), SentAt = DateTime.Now, IsRead = false,
            });
            _db.SaveChanges();
            if (offRoad)
            {
                vehicle.VehicleStatus = "Maintenance";
                _db.SaveChanges();
                returned = queue.ReturnToQueue(vehicle);
            }
            tx.Commit();
        }

        With("status", "Thank you - the operations team has your report on " + vehicle.PlateNumber + "."
            + (offRoad
                ? " The vehicle is set to Maintenance" + (returned.Count > 0
                    ? " and " + returned.Count + " upcoming " + (returned.Count == 1 ? "job" : "jobs") + " went back for a new vehicle"
                    : "") + "."
                : ""));
        return Back();
    }
}
