
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TruckLink.Data;
using TruckLink.Models;
using TruckLink.Support.Helpers;
using TruckLink.Support.Middleware;

namespace TruckLink.Controllers.Staff;


[Route("staff"), TypeFilter(typeof(Authenticate), Arguments = new object[] { "staff" })]
public class ReviewController : Controller
{
    private readonly AppDbContext _db;
    public ReviewController(AppDbContext db) => _db = db;

    [HttpGet("reviews", Name = "staff.reviews")]
    public IActionResult Index(int page = 1)
    {
        var staff = AuthManager.Staff()!;
        var mine = _db.Reviews.Where(r => r.Booking.StaffId == staff.StaffId);   
        var byRating = mine.GroupBy(r => r.Rating).Select(g => new { Rating = g.Key, N = g.Count() }).ToDictionary(x => x.Rating, x => x.N);
        var total = byRating.Values.Sum();

        return View("~/Views/Staff/Reviews.cshtml", new
        {
            reviews = Paginator<Review>.Paginate(mine.Include(r => r.Booking).Include(r => r.Customer).OrderByDescending(r => r.CreatedAt), page, 8),
            byRating,
            total,
            average = total > 0 ? byRating.Sum(kv => kv.Value * kv.Key) / (double)total : (double?)null,
            isDriver = staff.JobTitle == "Driver",
        });
    }
}
