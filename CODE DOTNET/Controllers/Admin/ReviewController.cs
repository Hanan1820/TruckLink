
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TruckLink.Controllers.Concerns;
using TruckLink.Data;
using TruckLink.Models;
using TruckLink.Support.Helpers;
using TruckLink.Support.Middleware;

namespace TruckLink.Controllers.Admin;


[Route("admin"), TypeFilter(typeof(Authenticate), Arguments = new object[] { "admin" })]
public class ReviewController : ConfirmsDeletion
{
    
    public static readonly List<(string Key, string Label, int[] Ratings)> CHIPS = new()
    {
        ("5", "5 stars", new[] { 5 }), ("4", "4 stars", new[] { 4 }), ("3", "3 stars", new[] { 3 }), ("low", "1–2 stars", new[] { 1, 2 }),
    };

    public static readonly Dictionary<string, string> SORTS = new() { ["newest"] = "Newest first", ["lowest"] = "Lowest rating", ["highest"] = "Highest rating" };

    private readonly AppDbContext _db;
    public ReviewController(AppDbContext db) => _db = db;

    [HttpGet("reviews", Name = "admin.reviews.index")]
    public IActionResult Index(string? rating, string? sort, string? moderated, string? q, int? delete, int page = 1)
    {
        rating = CHIPS.Any(c => c.Key == rating) ? rating : null;
        sort = sort != null && SORTS.ContainsKey(sort) ? sort : "newest";
        moderated = moderated is "yes" or "no" ? moderated : null;
        q = (q ?? "").Trim();

        var byRating = _db.Reviews.GroupBy(r => r.Rating).Select(g => new { g.Key, N = g.Count() }).ToDictionary(x => x.Key, x => x.N);
        var query = _db.Reviews.Include(r => r.Booking).ThenInclude(b => b.Driver).Include(r => r.Customer).Include(r => r.Moderator).AsQueryable();
        if (rating != null) { var ratings = CHIPS.First(c => c.Key == rating).Ratings; query = query.Where(r => ratings.Contains(r.Rating)); }
        if (moderated != null) query = moderated == "yes" ? query.Where(r => r.AdminId != null) : query.Where(r => r.AdminId == null);
        if (q != "")
        {
            var digits = Regex.Replace(Regex.Replace(q, @"^BK-\d{4}-", "", RegexOptions.IgnoreCase), @"\D", "");
            var id = digits == "" ? 0 : int.Parse(digits);
            query = query.Where(r => EF.Functions.Like(r.Comment, "%" + q + "%")
                || EF.Functions.Like(r.Customer.FirstName + " " + r.Customer.LastName, "%" + q + "%") || r.BookingId == id);
        }
        query = sort switch
        {
            "lowest" => query.OrderBy(r => r.Rating).ThenByDescending(r => r.CreatedAt),
            "highest" => query.OrderByDescending(r => r.Rating).ThenByDescending(r => r.CreatedAt),
            _ => query.OrderByDescending(r => r.CreatedAt).ThenByDescending(r => r.ReviewId),
        };
        var total = byRating.Values.Sum();

        return View("~/Views/Admin/Reviews.cshtml", new
        {
            reviews = Paginator<Review>.Paginate(query, page, 8),
            byRating,
            total,
            average = total > 0 ? byRating.Sum(kv => kv.Value * kv.Key) / (double)total : (double?)null,
            unmoderated = _db.Reviews.Count(r => r.AdminId == null),
            selected = delete != null ? _db.Reviews.Include(r => r.Booking).Include(r => r.Customer).FirstOrDefault(r => r.ReviewId == delete) : null,
        });
    }

    
    [HttpPost("reviews/{review:int}/moderate", Name = "admin.reviews.moderate"), ValidateAntiForgeryToken]
    public IActionResult Moderate(int review, string? rating, string? sort, string? q, string? moderated, int? page)
    {
        var model = _db.Reviews.Include(r => r.Booking).FirstOrDefault(r => r.ReviewId == review);
        if (model == null) return NotFound();
        model.AdminId = AuthManager.Admin()!.AdminId;
        _db.SaveChanges();

        With("status", "The review of " + model.Booking.Reference + " is kept and marked as moderated.");
        return RedirectToRoute("admin.reviews.index", new { rating, sort, q, moderated, page });   
    }

    [HttpDelete("reviews/{review:int}", Name = "admin.reviews.destroy"), ValidateAntiForgeryToken]
    public IActionResult Destroy(int review, [FromForm(Name = "confirm")] string? confirm)
    {
        var model = _db.Reviews.Include(r => r.Booking).FirstOrDefault(r => r.ReviewId == review);
        if (model == null) return NotFound();
        var reference = model.Booking.Reference;

        ConfirmDeletion(confirm, reference, "booking reference");
        _db.Reviews.Remove(model);
        _db.SaveChanges();

        With("status", "The review of " + reference + " was deleted.");
        return RedirectToRoute("admin.reviews.index");
    }
}
