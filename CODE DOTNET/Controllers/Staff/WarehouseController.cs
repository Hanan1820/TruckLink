
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TruckLink.Data;
using TruckLink.Models;
using TruckLink.Support.Helpers;
using TruckLink.Support.Middleware;
using TruckLink.Support.Services;

namespace TruckLink.Controllers.Staff;


[Route("staff"), TypeFilter(typeof(Authenticate), Arguments = new object[] { "staff" })]
public class WarehouseController : Controller
{
    private readonly WarehouseKeeping _keeping;
    private readonly AppDbContext _db;

    public WarehouseController(WarehouseKeeping keeping, AppDbContext db) { _keeping = keeping; _db = db; }

    [HttpGet("warehouse", Name = "staff.warehouse")]
    public IActionResult Index(string? status, int page = 1)
    {
        var keeper = Keeper();
        if (keeper == null) return StatusCode(403, "The warehouse is for warehouse keepers.");

        var warehouses = _db.Warehouses.Where(w => w.KeeperId == keeper.StaffId).OrderBy(w => w.WarehouseId).ToList();
        status = StorageRequest.STATUSES.Contains(status) ? status : null;
        var ids = warehouses.Select(w => w.WarehouseId).ToList();
        var mine = _db.StorageRequests.Where(s => ids.Contains(s.WarehouseId));

        var q = mine.Include(s => s.Booking).ThenInclude(b => b.Customer).Include(s => s.Warehouse).AsQueryable();
        q = status != null ? q.Where(s => s.StorageStatus == status) : q.Where(s => s.StorageStatus != "Released");   
        q = q.OrderBy(s => s.StorageStatus == "Reserved" ? 0 : s.StorageStatus == "Released" ? 2 : 1).ThenBy(s => s.FreePeriodEndDate);

        return View("~/Views/Staff/Warehouse.cshtml", new
        {
            warehouses,
            requests = Paginator<StorageRequest>.Paginate(q, page, 10),
            byStatus = mine.GroupBy(s => s.StorageStatus).Select(g => new { g.Key, N = g.Count() }).ToDictionary(x => x.Key, x => x.N),
            status,
            keeping = _keeping,
        });
    }

    [HttpPost("warehouse/{storage:int}/intake", Name = "staff.warehouse.intake"), ValidateAntiForgeryToken]
    public IActionResult Intake(int storage, [FromForm(Name = "storage_location")] string? location)
    {
        var model = Own(storage);
        if (model == null) return NotFound();
        _keeping.Intake(model, (location ?? "").Trim().ToUpperInvariant());
        With("status", model.Booking.Reference + " is stored in bay " + model.StorageLocation + ". The free week runs until " + model.FreePeriodEndDate.ToString("dd MMM", CultureInfo.InvariantCulture) + ".");
        return Back();
    }

    [HttpPost("warehouse/{storage:int}/move", Name = "staff.warehouse.move"), ValidateAntiForgeryToken]
    public IActionResult Move(int storage, [FromForm(Name = "storage_location")] string? location)
    {
        var model = Own(storage);
        if (model == null) return NotFound();
        _keeping.Move(model, (location ?? "").Trim().ToUpperInvariant());
        With("status", model.Booking.Reference + " moved to bay " + model.StorageLocation + ".");
        return Back();
    }

    [HttpPost("warehouse/{storage:int}/release", Name = "staff.warehouse.release"), ValidateAntiForgeryToken]
    public IActionResult Release(int storage)
    {
        var model = Own(storage);
        if (model == null) return NotFound();
        _keeping.Release(model);
        With("status", model.Booking.Reference + " is released - the driver can take it out for delivery.");
        return Back();
    }

    
    private Models.Staff? Keeper()
    {
        var staff = AuthManager.Staff()!;
        return staff.JobTitle == "Warehouse Keeper" ? staff : null;
    }

    private StorageRequest? Own(int storage)
    {
        var model = _db.StorageRequests.Include(s => s.Booking).ThenInclude(b => b.Customer).Include(s => s.Warehouse).FirstOrDefault(s => s.StorageId == storage);
        return model != null && model.Warehouse.KeeperId == Keeper()?.StaffId ? model : null;
    }
}
