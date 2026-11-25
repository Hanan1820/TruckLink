
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TruckLink.Data;
using TruckLink.Models;
using TruckLink.Support.Helpers;
using TruckLink.Support.Middleware;
using TruckLink.Support.Policies;
using TruckLink.Support.Services;

namespace TruckLink.Controllers.Customer;


[Route("customer"), TypeFilter(typeof(Authenticate), Arguments = new object[] { "customer" })]   
public class BookingController : Controller
{
    
    public static readonly List<(string Key, string Label, string[]? Statuses)> FILTERS = new()
    {
        ("all", "All", null),
        ("pending", "Pending", new[] { "Pending" }),
        ("accepted", "Accepted", new[] { "Accepted" }),
        ("in-progress", "In progress", new[] { "Picked Up", "In Transit", "In Storage", "Out for Delivery" }),
        ("completed", "Completed", new[] { "Completed" }),
        ("rejected", "Rejected", new[] { "Rejected" }),
        ("cancelled", "Cancelled", new[] { "Cancelled" }),
    };

    public const int PER_PAGE = 6;   

    private readonly AppDbContext _db;
    private readonly BookingPolicy _policy;
    private readonly PaymentLedger _ledger;

    public BookingController(AppDbContext db, BookingPolicy policy, PaymentLedger ledger)
    {
        _db = db; _policy = policy; _ledger = ledger;
    }

    [HttpGet("bookings", Name = "customer.bookings.index")]
    public IActionResult Index(string? status, string? sort, int page = 1)
    {
        var customer = AuthManager.Customer()!;
        var filter = FILTERS.Any(f => f.Key == status) ? status! : "all";
        var oldestFirst = sort == "oldest";

        var statusCounts = _db.Bookings.Where(b => b.CustomerId == customer.CustomerId)
            .GroupBy(b => b.BookingStatus).Select(g => new { Status = g.Key, N = g.Count() }).ToDictionary(x => x.Status, x => x.N);

        var query = _db.Bookings.Include(b => b.BookingServices).ThenInclude(l => l.Service)
            .Where(b => b.CustomerId == customer.CustomerId);
        var statuses = FILTERS.First(f => f.Key == filter).Statuses;
        if (statuses != null) query = query.Where(b => statuses.Contains(b.BookingStatus));   
        query = oldestFirst ? query.OrderBy(b => b.CreatedAt).ThenBy(b => b.BookingId) : query.OrderByDescending(b => b.CreatedAt).ThenByDescending(b => b.BookingId);

        return View("~/Views/Customer/Bookings/Index.cshtml", new
        {
            bookings = Paginator<Booking>.Paginate(query, page, PER_PAGE),   
            filter,
            oldestFirst,
            counts = FILTERS.ToDictionary(f => f.Key, f => f.Statuses == null
                ? statusCounts.Values.Sum()
                : f.Statuses.Sum(s => statusCounts.GetValueOrDefault(s, 0))),
        });
    }

    [HttpGet("bookings/{booking:int}", Name = "customer.bookings.show")]
    public IActionResult Show(int booking)
    {
        var model = _db.Bookings.Find(booking);
        if (model == null) return NotFound();   
        var denied = Authorize(_policy.View(AuthManager.Customer()!, model));
        if (denied != null) return denied;

        
        _db.Entry(model).Collection(b => b.Items).Query().Include(i => i.Category).Include(i => i.Photos).Load();
        _db.Entry(model).Collection(b => b.BookingServices).Query().Include(l => l.Service).Load();
        _db.Entry(model).Collection(b => b.Payments).Load();
        _db.Entry(model).Reference(b => b.Vehicle).Load();
        _db.Entry(model).Reference(b => b.Driver).Load();
        _db.Entry(model).Reference(b => b.StorageRequest).Load();
        if (model.StorageRequest != null) _db.Entry(model.StorageRequest).Reference(s => s.Warehouse).Load();
        _db.Entry(model).Reference(b => b.Review).Load();

        return View("~/Views/Customer/Bookings/Show.cshtml", new
        {
            booking = model,
            ledger = _ledger.Lines(model),
            due = _ledger.Due(model),
            unread = model.StaffId != null
                ? _db.Messages.Count(m => m.BookingId == model.BookingId && m.SenderType == "Staff" && !m.IsRead)
                : 0,
        });
    }

    
    [HttpPost("bookings/{booking:int}/cancel", Name = "customer.bookings.cancel"), ValidateAntiForgeryToken]
    public IActionResult Cancel(int booking)
    {
        var model = _db.Bookings.Find(booking);
        if (model == null) return NotFound();
        var denied = Authorize(_policy.Cancel(AuthManager.Customer()!, model));
        if (denied != null) return denied;

        int refunded;
        using (var tx = _db.Database.BeginTransaction())   
        {
            model.BookingStatus = "Cancelled";
            _db.SaveChanges();
            
            _db.StorageRequests.Where(s => s.BookingId == model.BookingId).ExecuteUpdate(s => s.SetProperty(x => x.StorageStatus, "Released"));
            refunded = _db.Payments.Where(p => p.BookingId == model.BookingId && p.PaymentStatus == "Paid")
                .ExecuteUpdate(p => p.SetProperty(x => x.PaymentStatus, "Refunded"));
            tx.Commit();
        }

        With("status", model.Reference + " is cancelled. "
            + (refunded > 0 ? "The services you already paid for are refunded to your card." : "Nothing has been charged for it."));
        return RedirectToRoute("customer.bookings.show", new { booking = model.BookingId });
    }
}
