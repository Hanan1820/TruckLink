
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TruckLink.Data;
using TruckLink.Models;
using TruckLink.Support.Middleware;
using TruckLink.Support.Services;

namespace TruckLink.Controllers.Customer;


[Route("customer"), TypeFilter(typeof(Authenticate), Arguments = new object[] { "customer" })]
public class StorageController : Controller
{
    private readonly StorageChange _change;
    private readonly PaymentLedger _ledger;
    private readonly AppDbContext _db;

    public StorageController(StorageChange change, PaymentLedger ledger, AppDbContext db)
    {
        _change = change; _ledger = ledger; _db = db;
    }

    [HttpGet("storage", Name = "customer.storage")]
    public IActionResult Index()
    {
        var customerId = AuthManager.Customer()!.CustomerId;
        var storages = _db.StorageRequests.Include(s => s.Warehouse)
            .Include(s => s.Booking).ThenInclude(b => b.BookingServices).ThenInclude(l => l.Service)
            .Include(s => s.Booking).ThenInclude(b => b.Payments)
            .Where(s => s.Booking.CustomerId == customerId)
            .OrderBy(s => s.StorageStatus == "Released" ? 1 : 0).ThenByDescending(s => s.StartDate).ToList();

        return View("~/Views/Customer/Storage.cshtml", new
        {
            storages,
            changeable = storages.ToDictionary(s => s.StorageId, s => _change.CanChange(s)),
            due = storages.ToDictionary(s => s.StorageId, s => _ledger.Due(s.Booking)),
            earliest = storages.ToDictionary(s => s.StorageId, s => _change.Earliest(s)),
        });
    }

    [HttpPut("storage/{storage:int}", Name = "customer.storage.update"), ValidateAntiForgeryToken]
    public IActionResult Update(int storage, [FromForm(Name = "end_date")] string? endDate)
    {
        var model = _db.StorageRequests.Include(s => s.Booking).FirstOrDefault(s => s.StorageId == storage);
        if (model == null || model.Booking.CustomerId != AuthManager.Customer()!.CustomerId) return NotFound();   
        if (!_change.CanChange(model)) return StatusCode(403, "This storage can no longer be changed.");

        var earliest = _change.Earliest(model);
        
        if (string.IsNullOrWhiteSpace(endDate)) ModelState.AddModelError("end_date", "The Planned release field is required.");
        else if (!DateTime.TryParse(endDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)) ModelState.AddModelError("end_date", "The Planned release is not a valid date.");
        else if (parsed.Date < earliest.Date) ModelState.AddModelError("end_date", "The release date cannot be before " + earliest.ToString("dd MMM yyyy", CultureInfo.InvariantCulture) + ".");
        else if (parsed.Date > model.StartDate.AddYears(1).Date) ModelState.AddModelError("end_date", "Storage can be planned up to one year ahead.");
        if (!ModelState.IsValid) return BackWithErrors();

        var before = model.EndDate;
        var quote = _change.Change(model, DateTime.Parse(endDate!, CultureInfo.InvariantCulture).Date);
        var end = DateTime.Parse(endDate!, CultureInfo.InvariantCulture);
        var verb = before == null || end > before ? "extended to" : "brought forward to";

        With("status", model.Booking.Reference + ": storage " + verb + " " + end.ToString("dd MMM yyyy", CultureInfo.InvariantCulture) + ". "
            + (quote.Fee > 0 ? "Extra days cost " + Support.Helpers.Helpers.Sar(quote.Fee) + " SAR in total." : "It stays within the free week."));
        return RedirectToRoute("customer.storage");
    }
}
