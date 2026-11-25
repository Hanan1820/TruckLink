
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TruckLink.Controllers.Concerns;
using TruckLink.Data;
using TruckLink.Models;
using TruckLink.Support.Helpers;
using TruckLink.Support.Middleware;
using TruckLink.ViewModels.Admin;

namespace TruckLink.Controllers.Admin;


[Route("admin"), TypeFilter(typeof(Authenticate), Arguments = new object[] { "admin" })]
public class ItemCategoryController : ConfirmsDeletion
{
    private readonly AppDbContext _db;
    public ItemCategoryController(AppDbContext db) => _db = db;

    [HttpGet("item-categories", Name = "admin.item-categories.index")]
    public IActionResult Index(string? vehicle, string? q, int page = 1)
    {
        vehicle = Models.Vehicle.TYPES.Contains(vehicle) ? vehicle : null;
        q = (q ?? "").Trim();
        var byVehicle = _db.ItemCategories.GroupBy(c => c.RecommendedVehicleType).Select(g => new { g.Key, N = g.Count() }).ToDictionary(x => x.Key, x => x.N);

        var query = _db.ItemCategories.AsQueryable();
        if (vehicle != null) query = query.Where(c => c.RecommendedVehicleType == vehicle);
        if (q != "") query = query.Where(c => EF.Functions.Like(c.CategoryName, "%" + q + "%") || EF.Functions.Like(c.Description, "%" + q + "%"));
        var categories = Paginator<ItemCategory>.Paginate(query.OrderBy(c => c.CategoryId), page, 10);

        return View("~/Views/Admin/Categories/Index.cshtml", new
        {
            categories,
            itemCounts = categories.Items.ToDictionary(c => c.CategoryId, c => _db.Items.Count(i => i.CategoryId == c.CategoryId)),   
            byVehicle,
            total = byVehicle.Values.Sum(),
            temperature = _db.ItemCategories.Count(c => c.RequiresTemperatureControl),
            items = _db.Items.Count(),
            bookings = _db.Items.Select(i => i.BookingId).Distinct().Count(),
            highlight = TempData["saved.id"],
        });
    }

    [HttpGet("item-categories/create", Name = "admin.item-categories.create")]
    public IActionResult Create() =>
        View("~/Views/Admin/Categories/Create.cshtml", new { category = new ItemCategory { RequiresTemperatureControl = false } });

    [HttpPost("item-categories", Name = "admin.item-categories.store"), ValidateAntiForgeryToken]
    public IActionResult Store(ItemCategoryRequest request)
    {
        if (!ModelState.IsValid) return BackWithErrors();
        var d = request.Category();
        var category = new ItemCategory
        {
            CategoryName = (string)d["category_name"]!, Description = d["description"] as string,
            RequiresTemperatureControl = (bool)d["requires_temperature_control"]!, RecommendedVehicleType = (string)d["recommended_vehicle_type"]!,
            AdminId = AuthManager.Admin()!.AdminId,
        };
        _db.ItemCategories.Add(category);
        _db.SaveChanges();

        Saved("category:" + category.CategoryId, category.CategoryId, created: true);
        return RedirectToRoute("admin.item-categories.show", new { item_category = category.CategoryId });
    }

    [HttpGet("item-categories/{item_category:int}", Name = "admin.item-categories.show")]
    public IActionResult Show(int item_category)
    {
        var model = _db.ItemCategories.Include(c => c.Admin).FirstOrDefault(c => c.CategoryId == item_category);
        if (model == null) return NotFound();
        var items = _db.Items.Where(i => i.CategoryId == model.CategoryId);

        return View("~/Views/Admin/Categories/Show.cshtml", new
        {
            category = model,
            recent = items.Include(i => i.Booking).OrderByDescending(i => i.ItemId).Take(8).ToList(),
            stats = new
            {
                items = items.Sum(i => (int?)i.Quantity) ?? 0,
                bookings = items.Select(i => i.BookingId).Distinct().Count(),
                volume = (double)(items.Sum(i => i.EstimatedVolumeM3) ?? 0m),   
            },
        }, DeleteGuard(model));
    }

    [HttpGet("item-categories/{item_category:int}/edit", Name = "admin.item-categories.edit")]
    public IActionResult Edit(int item_category)
    {
        var model = _db.ItemCategories.Find(item_category);
        return model == null ? NotFound() : View("~/Views/Admin/Categories/Edit.cshtml", new { category = model });
    }

    [HttpPut("item-categories/{item_category:int}", Name = "admin.item-categories.update"), ValidateAntiForgeryToken]
    public IActionResult Update(int item_category, ItemCategoryRequest request)
    {
        var model = _db.ItemCategories.Find(item_category);
        if (model == null) return NotFound();
        request.IgnoreCategoryId = model.CategoryId;   
        if (!ModelState.IsValid) return BackWithErrors();

        var d = request.Category();
        model.CategoryName = (string)d["category_name"]!; model.Description = d["description"] as string;
        model.RequiresTemperatureControl = (bool)d["requires_temperature_control"]!; model.RecommendedVehicleType = (string)d["recommended_vehicle_type"]!;
        _db.SaveChanges();

        Saved("category:" + model.CategoryId, model.CategoryId, created: false);
        return RedirectToRoute("admin.item-categories.show", new { item_category = model.CategoryId });
    }

    [HttpDelete("item-categories/{item_category:int}", Name = "admin.item-categories.destroy"), ValidateAntiForgeryToken]
    public IActionResult Destroy(int item_category, [FromForm(Name = "confirm")] string? confirm)
    {
        var model = _db.ItemCategories.Find(item_category);
        if (model == null) return NotFound();

        ConfirmDeletion(confirm, model.CategoryName, "category name");
        if (((List<string>)DeleteGuard(model)["uses"]!).Count > 0)
            RefuseDeletion(model.CategoryName + " is used by booked items and cannot be deleted.");

        var name = model.CategoryName;
        _db.ItemCategories.Remove(model);
        _db.SaveChanges();

        With("status", "Category " + name + " was deleted.");
        return RedirectToRoute("admin.item-categories.index");
    }

    private Dictionary<string, object?> DeleteGuard(ItemCategory category)
    {
        var items = _db.Items.Count(i => i.CategoryId == category.CategoryId);
        return new() { ["uses"] = items > 0 ? new List<string> { "classifies <b>" + items + " booked " + Helpers.Plural("item", items) + "</b>" } : new List<string>() };
    }
}
