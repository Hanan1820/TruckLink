
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TruckLink.Controllers.Concerns;
using TruckLink.Data;
using TruckLink.Models;
using TruckLink.Support.Exceptions;
using TruckLink.Support.Helpers;
using TruckLink.Support.Middleware;
using TruckLink.Support.Services;
using TruckLink.ViewModels.Admin;

namespace TruckLink.Controllers.Admin;


[Route("admin"), TypeFilter(typeof(Authenticate), Arguments = new object[] { "admin" })]
public class StaffController : ConfirmsDeletion
{
    public const int PER_PAGE = 8;

    public static readonly Dictionary<string, string> SORTS = new() { ["name"] = "Name A–Z", ["newest"] = "Newest hire", ["title"] = "Job title" };

    
    public static readonly string[] OPEN_JOBS = { "Accepted", "Picked Up", "In Transit", "In Storage", "Out for Delivery" };

    private readonly BookingQueue _queue;
    private readonly AppDbContext _db;
    private readonly Microsoft.AspNetCore.Identity.IPasswordHasher<object> _hasher;

    public StaffController(BookingQueue queue, AppDbContext db, Microsoft.AspNetCore.Identity.IPasswordHasher<object> hasher)
    {
        _queue = queue; _db = db; _hasher = hasher;
    }

    [HttpGet("staff", Name = "admin.staff.index")]
    public IActionResult Index(string? title, string? status, string? sort, string? q, int page = 1)
    {
        var f = new
        {
            title = Models.Staff.TITLES.Contains(title) ? title : null,
            status = Models.Staff.STATUSES.Contains(status) ? status : null,
            sort = sort != null && SORTS.ContainsKey(sort) ? sort : "name",
            q = (q ?? "").Trim(),
        };
        var byTitle = _db.Staff.GroupBy(s => s.JobTitle).Select(g => new { g.Key, N = g.Count() }).ToDictionary(x => x.Key, x => x.N);
        var byStatus = _db.Staff.GroupBy(s => s.StaffStatus).Select(g => new { g.Key, N = g.Count() }).ToDictionary(x => x.Key, x => x.N);

        var query = _db.Staff.AsQueryable();
        if (f.title != null) query = query.Where(s => s.JobTitle == f.title);
        if (f.status != null) query = query.Where(s => s.StaffStatus == f.status);
        if (f.q != "") query = Search(query, f.q);
        query = f.sort switch
        {
            "newest" => query.OrderByDescending(s => s.HireDate).ThenByDescending(s => s.StaffId),
            "title" => query.OrderBy(s => s.JobTitle).ThenBy(s => s.FirstName),
            _ => query.OrderBy(s => s.FirstName).ThenBy(s => s.LastName),
        };

        return View("~/Views/Admin/Staff/Index.cshtml", new
        {
            staff = Paginator<Models.Staff>.Paginate(query, page, PER_PAGE),
            filters = f,
            byTitle,
            total = byTitle.Values.Sum(),
            kpis = new
            {
                active = byStatus.GetValueOrDefault("Active"),
                leave = byStatus.GetValueOrDefault("On Leave"),
                
                onJob = _db.Bookings.Where(b => OPEN_JOBS.Contains(b.BookingStatus) && b.StaffId != null).Select(b => b.StaffId).Distinct().Count(),
            },
            highlight = TempData["saved.id"],
        });
    }

    [HttpGet("staff/create", Name = "admin.staff.create")]
    public IActionResult Create() =>
        View("~/Views/Admin/Staff/Create.cshtml", new { member = new Models.Staff { StaffStatus = "Active", HireDate = DateTime.Today } });

    [HttpPost("staff", Name = "admin.staff.store"), ValidateAntiForgeryToken]
    public IActionResult Store(StaffRequest request)
    {
        if (!ModelState.IsValid) return BackWithErrors();
        var member = new Models.Staff { AdminId = AuthManager.Admin()!.AdminId, Password = _hasher.HashPassword(null!, request.Password!) };
        Fill(member, request.Staff());
        _db.Staff.Add(member);
        _db.SaveChanges();

        Saved("staff:" + member.StaffId, member.StaffId, created: true);
        return RedirectToRoute("admin.staff.show", new { staff = member.StaffId });
    }

    [HttpGet("staff/{staff:int}", Name = "admin.staff.show")]
    public IActionResult Show(int staff)
    {
        var model = _db.Staff.Include(s => s.Admin).Include(s => s.Warehouses).FirstOrDefault(s => s.StaffId == staff);
        if (model == null) return NotFound();
        var jobs = _db.Bookings.Where(b => b.StaffId == model.StaffId);
        var rating = _db.Reviews.Where(r => jobs.Select(b => b.BookingId).Contains(r.BookingId));

        return View("~/Views/Admin/Staff/Show.cshtml", new
        {
            member = model,
            upcoming = _db.Bookings.Include(b => b.Customer).Where(b => b.StaffId == model.StaffId && OPEN_JOBS.Contains(b.BookingStatus)).OrderBy(b => b.PickupDatetime).ToList(),
            record = new
            {
                completed = jobs.Count(b => b.BookingStatus == "Completed"),
                km = (double)(jobs.Where(b => b.BookingStatus == "Completed").Sum(b => b.DistanceKm) ?? 0m),
                rating = rating.Average(r => (double?)r.Rating),
                reviews = rating.Count(),
            },
        }, DeleteGuard(model));
    }

    [HttpGet("staff/{staff:int}/edit", Name = "admin.staff.edit")]
    public IActionResult Edit(int staff)
    {
        var model = _db.Staff.Find(staff);
        if (model == null) return NotFound();
        return View("~/Views/Admin/Staff/Edit.cshtml", new
        {
            member = model,
            upcoming = _queue.UpcomingFor(model),
            keeps = _db.Warehouses.Where(w => w.KeeperId == model.StaffId).Select(w => w.WarehouseName).ToList(),
        });
    }

    [HttpPut("staff/{staff:int}", Name = "admin.staff.update"), ValidateAntiForgeryToken]
    public IActionResult Update(int staff, StaffRequest request)
    {
        var model = _db.Staff.Find(staff);
        if (model == null) return NotFound();
        request.Record = model;   
        if (!ModelState.IsValid) return BackWithErrors();

        var data = request.Staff();
        KeepWarehouseKeeper(model, (string)data["job_title"]!, (string)data["staff_status"]!);

        List<Booking> returned;
        using (var tx = _db.Database.BeginTransaction())   
        {
            Fill(model, data);
            _db.SaveChanges();
            
            returned = model.JobTitle == "Driver" && model.StaffStatus == "Active" ? new List<Booking>() : _queue.ReturnToQueue(model, AuthManager.Admin()!.AdminId);
            tx.Commit();
        }

        Saved("staff:" + model.StaffId, model.StaffId, created: false);
        TempData["saved.note"] = ReturnedNote(returned);
        return RedirectToRoute("admin.staff.show", new { staff = model.StaffId });
    }

    
    [HttpPost("staff/{staff:int}/deactivate", Name = "admin.staff.deactivate"), ValidateAntiForgeryToken]
    public IActionResult Deactivate(int staff)
    {
        var model = _db.Staff.Find(staff);
        if (model == null) return NotFound();
        KeepWarehouseKeeper(model, model.JobTitle, "Inactive", "confirm");

        List<Booking> returned;
        using (var tx = _db.Database.BeginTransaction())
        {
            model.StaffStatus = "Inactive";
            _db.SaveChanges();
            returned = _queue.ReturnToQueue(model, AuthManager.Admin()!.AdminId);
            tx.Commit();
        }

        With("status", model.FullName + " is now Inactive and can no longer sign in. " + ReturnedNote(returned));
        return RedirectToRoute("admin.staff.show", new { staff = model.StaffId });
    }

    [HttpDelete("staff/{staff:int}", Name = "admin.staff.destroy"), ValidateAntiForgeryToken]
    public IActionResult Destroy(int staff, [FromForm(Name = "confirm")] string? confirm)
    {
        var model = _db.Staff.Find(staff);
        if (model == null) return NotFound();

        ConfirmDeletion(confirm, model.FullName, "full name");
        if (((List<string>)DeleteGuard(model)["uses"]!).Count > 0)
            RefuseDeletion(model.FullName + " is referenced by other records and can only be set Inactive.");

        var name = model.FullName;
        _db.Staff.Remove(model);
        _db.SaveChanges();

        With("status", "Staff member " + name + " was deleted.");
        return RedirectToRoute("admin.staff.index");
    }

    
    private static void Fill(Models.Staff m, Dictionary<string, object?> d)
    {
        m.FirstName = (string)d["first_name"]!; m.LastName = (string)d["last_name"]!; m.Email = (string)d["email"]!; m.Phone = (string)d["phone"]!;
        m.JobTitle = (string)d["job_title"]!; m.LicenseNumber = d["license_number"] as string; m.Specialty = d["specialty"] as string;
        m.HireDate = ((DateTime?)d["hire_date"])!.Value; m.StaffStatus = (string)d["staff_status"]!;
        m.ShiftStart = d["shift_start"] is string a ? TimeSpan.Parse(a) : null;
        m.ShiftEnd = d["shift_end"] is string b ? TimeSpan.Parse(b) : null;
    }

    
    private void KeepWarehouseKeeper(Models.Staff staff, string title, string status, string field = "staff_status")
    {
        var kept = _db.Warehouses.Where(w => w.KeeperId == staff.StaffId).Select(w => w.WarehouseName).ToList();
        if (kept.Count > 0 && (title != "Warehouse Keeper" || status != "Active"))
        {
            var key = title != "Warehouse Keeper" && field == "staff_status" ? "job_title" : field;
            throw ValidationException.WithMessages(key, staff.FullName + " keeps " + JoinAnd(kept) + ". Give the warehouse another keeper first.");
        }
    }

    private static string JoinAnd(List<string> items) =>   
        items.Count <= 1 ? string.Join("", items) : string.Join(", ", items.Take(items.Count - 1)) + " and " + items[^1];

    private static string? ReturnedNote(List<Booking> returned)
    {
        if (returned.Count == 0) return null;
        return JoinAnd(returned.Select(b => b.Reference).ToList()) + " " + (returned.Count == 1 ? "was" : "were") + " returned to the queue for a new driver.";
    }

    
    private Dictionary<string, object?> DeleteGuard(Models.Staff staff)
    {
        var jobs = _db.Bookings.Count(b => b.StaffId == staff.StaffId);
        var warehouses = _db.Warehouses.Count(w => w.KeeperId == staff.StaffId);
        var messages = _db.Messages.Count(m => m.StaffId == staff.StaffId);
        var notices = _db.Announcements.Count(a => a.StaffId == staff.StaffId);
        var uses = new[]
        {
            jobs > 0 ? "is the driver of <b>" + jobs + " " + Helpers.Plural("booking", jobs) + "</b>" : null,
            warehouses > 0 ? "keeps <b>" + warehouses + " " + Helpers.Plural("warehouse", warehouses) + "</b>" : null,
            messages > 0 ? "appears in <b>" + messages + " chat " + Helpers.Plural("message", messages) + "</b>" : null,
            notices > 0 ? "has received <b>" + notices + " " + Helpers.Plural("announcement", notices) + "</b>" : null,
        }.Where(x => x != null).Select(x => x!).ToList();
        return new() { ["uses"] = uses };
    }

    private static IQueryable<Models.Staff> Search(IQueryable<Models.Staff> q, string term)
    {
        var digits = new string(term.Where(char.IsDigit).ToArray());
        var local = digits.TrimStart('0');
        return q.Where(s => EF.Functions.Like(s.FirstName + " " + s.LastName, "%" + term + "%")
            || EF.Functions.Like(s.Email, "%" + term + "%") || EF.Functions.Like(s.LicenseNumber, "%" + term + "%")
            || EF.Functions.Like(s.JobTitle, "%" + term + "%")
            || (digits.Length >= 4 && EF.Functions.Like(s.Phone, "%" + local + "%")));
    }
}
