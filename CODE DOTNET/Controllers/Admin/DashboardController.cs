
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TruckLink.Data;
using TruckLink.Models;
using TruckLink.Support.Middleware;

namespace TruckLink.Controllers.Admin;


[Route("admin"), TypeFilter(typeof(Authenticate), Arguments = new object[] { "admin" })]
public class DashboardController : Controller
{
    private readonly AppDbContext _db;
    public DashboardController(AppDbContext db) => _db = db;

    
    [HttpGet("dashboard", Name = "admin.dashboard")]
    public IActionResult Invoke()
    {
        var onTheRoad = new[] { "Picked Up", "In Transit", "Out for Delivery" };
        var now = DateTime.Now;
        var monthStart = new DateTime(now.Year, now.Month, 1);
        var monthEnd = monthStart.AddMonths(1).AddTicks(-1);

        return View("~/Views/Admin/Dashboard.cshtml", new
        {
            kpis = new
            {
                pending = Booking.ScopePending(_db.Bookings).Count(),
                road = _db.Bookings.Count(b => onTheRoad.Contains(b.BookingStatus)),
                available = Vehicle.ScopeAvailable(_db.Vehicles).Count(),
                vehicles = _db.Vehicles.Count(),
                revenue = (double)_db.Payments.Where(p => p.PaymentStatus == "Paid" && p.TransactionDate >= monthStart && p.TransactionDate <= monthEnd)
                    .Sum(p => (decimal?)p.Amount ?? 0m),
            },
            pending = Booking.ScopePending(_db.Bookings).Include(b => b.Customer).OrderBy(b => b.CreatedAt).ThenBy(b => b.BookingId).Take(6).ToList(),
            chart = Chart(),
            today = _db.Bookings.Count(b => b.BookingStatus == "Accepted" && b.PickupDatetime.Date == DateTime.Today),
            warehouses = _db.Warehouses.OrderBy(w => w.WarehouseId).ToList(),
            endingSoon = _db.StorageRequests.Count(s => (s.StorageStatus == "Free Period" || s.StorageStatus == "Extended")
                && s.FreePeriodEndDate >= DateTime.Today && s.FreePeriodEndDate <= DateTime.Today.AddDays(2)),
            fleet = _db.Vehicles.GroupBy(v => v.VehicleStatus).Select(g => new { g.Key, N = g.Count() }).ToDictionary(x => x.Key, x => x.N),
        });
    }

    
    private object Chart()
    {
        var thisMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        var months = Enumerable.Range(0, 6).Select(i => thisMonth.AddMonths(-(5 - i))).ToList();   
        var from = months.First();
        var bookings = _db.Bookings.Where(b => b.PickupDatetime >= from).Select(b => new { b.PickupDatetime, b.BookingStatus }).ToList();
        var paid = _db.Payments.Where(p => p.PaymentStatus == "Paid" && p.TransactionDate >= from).Select(p => new { p.TransactionDate, p.Amount }).ToList();
        bool Same(DateTime a, DateTime m) => a.Year == m.Year && a.Month == m.Month;
        var refused = new[] { "Rejected", "Cancelled" };

        return new
        {
            labels = months.Select(m => m.ToString("MMM yyyy", System.Globalization.CultureInfo.InvariantCulture)).ToList(),
            bookings = months.Select(m => bookings.Count(b => Same(b.PickupDatetime, m) && !refused.Contains(b.BookingStatus))).ToList(),
            refused = months.Select(m => bookings.Count(b => Same(b.PickupDatetime, m) && refused.Contains(b.BookingStatus))).ToList(),
            revenue = months.Select(m => Math.Round(paid.Where(p => Same(p.TransactionDate, m)).Sum(p => (double)p.Amount), 2)).ToList(),
        };
    }
}
