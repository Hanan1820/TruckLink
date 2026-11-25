
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TruckLink.Data;
using TruckLink.Models;
using TruckLink.Support.Middleware;
using TruckLink.Support.Policies;
using TruckLink.Support.Services;
using TruckLink.ViewModels.Customer;

namespace TruckLink.Controllers.Customer;


[Route("customer"), TypeFilter(typeof(Authenticate), Arguments = new object[] { "customer" })]
public class BookingEditController : Controller
{
    private readonly AppDbContext _db;
    private readonly BookingPolicy _policy;
    private readonly IConfiguration _config;

    public BookingEditController(AppDbContext db, BookingPolicy policy, IConfiguration config)
    {
        _db = db; _policy = policy; _config = config;
    }

    [HttpGet("bookings/{booking:int}/edit", Name = "customer.bookings.edit")]
    public IActionResult Edit(int booking)
    {
        var model = _db.Bookings.Find(booking);
        if (model == null) return NotFound();
        var denied = Authorize(_policy.Update(AuthManager.Customer()!, model));
        if (denied != null) return denied;

        _db.Entry(model).Collection(b => b.BookingServices).Query().Include(l => l.Service).Load();
        _db.Entry(model).Reference(b => b.StorageRequest).Load();

        return View("~/Views/Customer/Bookings/Edit.cshtml", new
        {
            booking = model,
            cities = _config.GetSection("TruckLink:Cities").Get<string[]>(),
            valueAdded = model.IsPending()
                ? Service.ScopeOfType(Service.ScopeActive(_db.Services), "Value-Added").OrderBy(s => s.ServiceId).ToList()
                : new List<Service>(),
        });
    }

    [HttpPut("bookings/{booking:int}", Name = "customer.bookings.update"), ValidateAntiForgeryToken]
    public IActionResult Update(int booking, UpdateBookingRequest request, [FromServices] BookingUpdate updater)
    {
        var model = _db.Bookings.Find(booking);
        if (model == null) return NotFound();
        request.Booking = model;   
        if (!ModelState.IsValid) return BackWithErrors();
        var denied = Authorize(_policy.Update(AuthManager.Customer()!, model));
        if (denied != null) return denied;

        var result = updater.Update(model, request, model.IsPending() ? request.Extras() : null);
        With("status", result.Requeued
            ? model.Reference + " is updated. Because the route or time changed, TruckLink reviews it again and assigns a vehicle for the new slot."
            : result.Changed ? model.Reference + " is updated." : "Nothing was changed.");
        return RedirectToRoute("customer.bookings.show", new { booking = model.BookingId });
    }
}
