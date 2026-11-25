
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TruckLink.Data;
using TruckLink.Models;
using TruckLink.Support.Helpers;
using TruckLink.Support.Middleware;

namespace TruckLink.Controllers.Admin;


[Route("admin"), TypeFilter(typeof(Authenticate), Arguments = new object[] { "admin" })]
public class WarehouseController : Controller
{
    private readonly AppDbContext _db;
    public WarehouseController(AppDbContext db) => _db = db;

    
    [HttpGet("warehouse", Name = "admin.warehouse")]
    public IActionResult Invoke(string? status, int? warehouse, string? q, int page = 1)
    {
        var warehouses = _db.Warehouses.Include(w => w.Keeper).OrderBy(w => w.WarehouseId).ToList();
        var activeCounts = warehouses.ToDictionary(w => w.WarehouseId, w => _db.StorageRequests.Count(s => s.WarehouseId == w.WarehouseId && s.StorageStatus != "Released"));   
        status = StorageRequest.STATUSES.Contains(status) ? status : null;
        var warehouseId = warehouses.FirstOrDefault(w => w.WarehouseId == warehouse)?.WarehouseId;
        q = (q ?? "").Trim();

        var query = _db.StorageRequests.Include(s => s.Booking).ThenInclude(b => b.Customer).Include(s => s.Warehouse).AsQueryable();
        if (status != null) query = query.Where(s => s.StorageStatus == status);
        if (warehouseId != null) query = query.Where(s => s.WarehouseId == warehouseId);
        if (q != "")
        {
            var digits = Regex.Replace(Regex.Replace(q, @"^BK-\d{4}-", "", RegexOptions.IgnoreCase), @"\D", "");
            var id = digits == "" ? 0 : int.Parse(digits);
            query = query.Where(s => EF.Functions.Like(s.StorageLocation, "%" + q + "%")
                || EF.Functions.Like(s.Booking.Customer.FirstName + " " + s.Booking.Customer.LastName, "%" + q + "%") || s.BookingId == id);
        }
        query = query.OrderBy(s => s.StorageStatus == "Released" ? 1 : 0).ThenBy(s => s.FreePeriodEndDate);

        return View("~/Views/Admin/Warehouse.cshtml", new
        {
            warehouses,
            activeCounts,
            requests = Paginator<StorageRequest>.Paginate(query, page, 8),
            byStatus = _db.StorageRequests.GroupBy(s => s.StorageStatus).Select(g => new { g.Key, N = g.Count() }).ToDictionary(x => x.Key, x => x.N),
            fees = (double)(_db.StorageRequests.Sum(s => (decimal?)s.StorageFee) ?? 0m),
            endingSoon = _db.StorageRequests.Count(s => (s.StorageStatus == "Free Period" || s.StorageStatus == "Extended")
                && s.FreePeriodEndDate >= DateTime.Today && s.FreePeriodEndDate <= DateTime.Today.AddDays(2)),
        });
    }
}
