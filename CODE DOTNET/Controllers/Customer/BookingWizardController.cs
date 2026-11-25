
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TruckLink.Data;
using TruckLink.Models;
using TruckLink.Support.Middleware;
using TruckLink.Support.Services;
using TruckLink.ViewModels.Customer;

namespace TruckLink.Controllers.Customer;


[Route("customer"), TypeFilter(typeof(Authenticate), Arguments = new object[] { "customer" })]
public class BookingWizardController : Controller
{
    private readonly BookingDraft _draft;
    private readonly AppDbContext _db;

    public BookingWizardController(BookingDraft draft, AppDbContext db) { _draft = draft; _db = db; }

    [HttpGet("bookings/create", Name = "customer.bookings.create")]
    public IActionResult Start() => ToStep(_draft.FirstIncomplete());

    
    [HttpGet("bookings/create/service", Name = "customer.bookings.create.service")]
    public IActionResult Service() =>
        View("~/Views/Customer/Booking/Step1Service.cshtml", new
        {
            draft = _draft,
            services = Models.Service.ScopeOfType(Models.Service.ScopeActive(_db.Services), "Transport").OrderBy(s => s.ServiceId).ToList(),
        });

    [HttpPost("bookings/create/service", Name = "customer.bookings.create.service.save"), ValidateAntiForgeryToken]
    public IActionResult SaveService(ServiceStepRequest request)
    {
        if (!ModelState.IsValid) return BackWithErrors();
        _draft.Put(new() { ["service_id"] = request.ServiceId });
        return ToStep("locations");
    }

    
    [HttpPost("bookings/create/cancel", Name = "customer.bookings.create.cancel"), ValidateAntiForgeryToken]
    public IActionResult Cancel()
    {
        _draft.Discard();
        return Redirect("/customer/bookings");
    }

    
    [HttpGet("bookings/create/locations", Name = "customer.bookings.create.locations")]
    public IActionResult Locations() =>
        Guard("locations") ?? View("~/Views/Customer/Booking/Step2Locations.cshtml", new
        {
            draft = _draft,
            cities = HttpContext.RequestServices.GetRequiredService<IConfiguration>().GetSection("TruckLink:Cities").Get<string[]>(),
        });

    [HttpPost("bookings/create/locations", Name = "customer.bookings.create.locations.save"), ValidateAntiForgeryToken]
    public IActionResult SaveLocations(LocationsStepRequest request)
    {
        if (!ModelState.IsValid) return BackWithErrors();
        
        _draft.Put(new()
        {
            ["pickup_address"] = request.PickupAddress, ["pickup_city"] = request.PickupCity,
            ["dropoff_address"] = request.DropoffAddress, ["dropoff_city"] = request.DropoffCity,
            ["pickup_date"] = request.PickupDate, ["pickup_time"] = request.PickupTime, ["customer_notes"] = request.CustomerNotes,
            ["pickup_lat"] = request.PickupLat, ["pickup_lng"] = request.PickupLng,
            ["dropoff_lat"] = request.DropoffLat, ["dropoff_lng"] = request.DropoffLng,
            ["locations_done"] = true,
        });
        return ToStep("items");
    }

    
    [HttpGet("bookings/create/items", Name = "customer.bookings.create.items")]
    public IActionResult Items() =>
        Guard("items") ?? View("~/Views/Customer/Booking/Step3Items.cshtml", new
        {
            draft = _draft,
            items = _draft.ItemModels(),
            categories = _db.ItemCategories.OrderBy(c => c.CategoryId).ToList(),
        });

    
    [HttpPost("bookings/create/items", Name = "customer.bookings.create.items.add"), ValidateAntiForgeryToken]
    public IActionResult AddItem(AddItemRequest request)
    {
        if (!ModelState.IsValid) return BackWithErrors();
        var redirect = Guard("items");
        if (redirect != null) return redirect;

        var uid = _draft.AddItem(request.ItemName, request.CategoryId!.Value, request.Quantity!.Value, request.Photos);
        var expectsJson = Request.Headers.Accept.ToString().Contains("json");
        return expectsJson ? StatusCode(201, new { uid }) : ToStep("items");
    }

    [HttpDelete("bookings/create/items/{uid}", Name = "customer.bookings.create.items.remove"), ValidateAntiForgeryToken]
    public IActionResult RemoveItem(string uid)
    {
        _draft.RemoveItem(uid);
        return ToStep("items");
    }

    
    [HttpGet("bookings/create/measurement", Name = "customer.bookings.create.measurement")]
    public IActionResult Measurement([FromServices] VehicleRecommender recommender)
    {
        var redirect = Guard("measurement");
        if (redirect != null) return redirect;

        var recommendation = _draft.Recommendation();
        var type = _draft.VehicleType();
        return View("~/Views/Customer/Booking/Step4Measurement.cshtml", new
        {
            draft = _draft,
            items = _draft.ItemModels(),
            recommendation,
            type,
            capacity = type != null && recommender.Capacities().TryGetValue(type, out var c) ? (double?)c.Capacity : null,
            load = recommender.LoadPercent(type!, recommendation.Volume),
            choices = new[] { recommendation.Type }.Concat(recommender.LargerTypes(recommendation.Type)).ToArray(),
            capacities = recommender.Capacities(),
        });
    }

    [HttpPost("bookings/create/measurement/correct", Name = "customer.bookings.create.measurement.correct"), ValidateAntiForgeryToken]
    public IActionResult CorrectItem(CorrectItemRequest request)
    {
        if (!ModelState.IsValid) return BackWithErrors();
        if (!_draft.Items().ContainsKey(request.Uid)) return NotFound();   
        _draft.CorrectItem(request.Uid, (double)request.EstimatedLengthCm!, (double)request.EstimatedWidthCm!,
            (double)request.EstimatedHeightCm!, (double)request.EstimatedWeightKg!);
        return ToStep("measurement");
    }

    
    [HttpPost("bookings/create/measurement/vehicle", Name = "customer.bookings.create.measurement.vehicle"), ValidateAntiForgeryToken]
    public IActionResult ChooseVehicle([FromForm(Name = "vehicle_type")] string? vehicleType, [FromServices] VehicleRecommender recommender)
    {
        var recommended = _draft.Recommendation().Type;
        var allowed = new[] { recommended }.Concat(recommender.LargerTypes(recommended));
        
        if (string.IsNullOrEmpty(vehicleType)) ModelState.AddModelError("vehicle_type", "The Vehicle type field is required.");
        else if (!allowed.Contains(vehicleType)) ModelState.AddModelError("vehicle_type", "The selected Vehicle type is invalid.");
        if (!ModelState.IsValid) return BackWithErrors();

        _draft.Put(new() { ["vehicle_type"] = vehicleType });
        return ToStep("measurement");
    }

    [HttpPost("bookings/create/measurement", Name = "customer.bookings.create.measurement.save"), ValidateAntiForgeryToken]
    public IActionResult SaveMeasurement()
    {
        var redirect = Guard("measurement");
        if (redirect != null) return redirect;
        _draft.Put(new() { ["measurement_done"] = true });
        return ToStep("extras");
    }

    
    [HttpGet("bookings/create/extras", Name = "customer.bookings.create.extras")]
    public IActionResult Extras() =>
        Guard("extras") ?? View("~/Views/Customer/Booking/Step5Extras.cshtml", new
        {
            draft = _draft,
            valueAdded = Models.Service.ScopeOfType(Models.Service.ScopeActive(_db.Services), "Value-Added").OrderBy(s => s.ServiceId).ToList(),
            extensionPrices = Models.Service.ScopeOfType(Models.Service.ScopeActive(_db.Services), "Storage").ToDictionary(s => s.PricingUnit, s => s.BasePrice),
            warehouseCity = WarehouseCity(),
        });

    [HttpPost("bookings/create/extras", Name = "customer.bookings.create.extras.save"), ValidateAntiForgeryToken]
    public IActionResult SaveExtras(ExtrasStepRequest request)
    {
        request.DraftPickupDate = _draft.Get<string>("pickup_date");   
        if (!ModelState.IsValid) return BackWithErrors();
        var errors = ((IValidatableObject)request).Validate(new ValidationContext(request)).ToList();
        if (errors.Count > 0)
        {
            foreach (var e in errors) ModelState.AddModelError(e.MemberNames.FirstOrDefault() ?? "", e.ErrorMessage!);
            return BackWithErrors();
        }

        var extras = request.Services
            .Select(kv => (kv.Key, Qty: kv.Value.Selected == true ? Math.Max(1, kv.Value.Quantity ?? 1) : 0))
            .Where(x => x.Qty != 0).ToDictionary(x => x.Key, x => x.Qty);
        _draft.Put(new()
        {
            ["extras"] = extras,
            ["storage"] = request.Storage == true ? new DraftStorage { end_date = request.EndDate! } : null,
            ["extras_done"] = true,
        });
        return ToStep("review");
    }

    
    [HttpGet("bookings/create/review", Name = "customer.bookings.create.review")]
    public IActionResult Review() =>
        Guard("review") ?? View("~/Views/Customer/Booking/Step6Review.cshtml", new { draft = _draft, items = _draft.ItemModels() });

    [HttpPost("bookings/create/review", Name = "customer.bookings.create.submit"), ValidateAntiForgeryToken]
    public IActionResult Submit([FromServices] BookingSubmission submission)
    {
        var redirect = Guard("review");
        if (redirect != null) return redirect;

        var booking = submission.Submit(AuthManager.Customer()!);
        return RedirectToRoute("customer.bookings.submitted", new { booking = booking.BookingId });
    }

    
    [HttpGet("bookings/{booking:int}/submitted", Name = "customer.bookings.submitted")]
    public IActionResult Submitted(int booking, [FromServices] CrewPlanner crew)
    {
        var model = _db.Bookings.Find(booking);
        if (model == null || model.CustomerId != AuthManager.Customer()!.CustomerId) return NotFound();   

        
        if (!model.IsPending()) return Redirect("/customer/bookings/" + model.BookingId);

        _db.Entry(model).Collection(b => b.Items).Load();
        _db.Entry(model).Collection(b => b.BookingServices).Query().Include(l => l.Service).Load();
        var lines = model.BookingServices;
        return View("~/Views/Customer/Booking/Submitted.cshtml", new
        {
            booking = model,
            transport = lines.FirstOrDefault(l => l.Service.ServiceType == "Transport")?.Service,
            crewSize = crew.RecommendedSize((double)(model.TotalVolumeM3 ?? 0), lines.Any(l => l.Service.ServiceName == "Assembly & Disassembly")),
            storageLine = lines.FirstOrDefault(l => l.Service.ServiceType == "Storage" && l.Service.PricingUnit != "first 7 days"),
            hasStorage = lines.Any(l => l.Service.ServiceType == "Storage"),
        });
    }

    private string? WarehouseCity()
    {
        var pickupCity = _draft.Get<string>("pickup_city");
        return _db.Warehouses.Where(w => w.City == pickupCity).Select(w => w.City).FirstOrDefault();   
    }

    private IActionResult? Guard(string step) => _draft.CanOpen(step) ? null : ToStep(_draft.FirstIncomplete());

    private IActionResult ToStep(string step) => RedirectToRoute("customer.bookings.create." + step);
}
