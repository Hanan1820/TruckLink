
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TruckLink.Data;
using TruckLink.Models;
using TruckLink.Support.Middleware;
using TruckLink.Support.Policies;

namespace TruckLink.Controllers.Customer;


[Route("customer"), TypeFilter(typeof(Authenticate), Arguments = new object[] { "customer" })]
public class ReviewController : Controller
{
    private readonly AppDbContext _db;
    private readonly BookingPolicy _policy;

    public ReviewController(AppDbContext db, BookingPolicy policy) { _db = db; _policy = policy; }

    [HttpGet("bookings/{booking:int}/review", Name = "customer.bookings.review")]
    public IActionResult Create(int booking)
    {
        var model = _db.Bookings.Find(booking);
        if (model == null) return NotFound();
        var denied = Authorize(_policy.View(AuthManager.Customer()!, model));
        if (denied != null) return denied;
        return RedirectToRoute("customer.bookings.show", new { booking = model.BookingId, review = 1 });
    }

    [HttpPost("bookings/{booking:int}/review", Name = "customer.bookings.review.store"), ValidateAntiForgeryToken]
    public IActionResult Store(int booking, [FromForm(Name = "rating")] int? rating, [FromForm(Name = "comment")] string? comment)
    {
        var model = _db.Bookings.Include(b => b.Review).First(b => b.BookingId == booking);
        var denied = Authorize(_policy.Review(AuthManager.Customer()!, model));
        if (denied != null) return denied;
        var data = Validated(rating, comment);
        if (data == null) return BackWithErrors("review");

        _db.Reviews.Add(new Review { Rating = data.Value.Rating, Comment = data.Value.Comment, BookingId = model.BookingId, CustomerId = model.CustomerId });
        _db.SaveChanges();

        With("status", "Thank you - your review of " + model.Reference + " is published.");
        return RedirectToRoute("customer.bookings.show", new { booking = model.BookingId });
    }

    [HttpPut("bookings/{booking:int}/review", Name = "customer.bookings.review.update"), ValidateAntiForgeryToken]
    public IActionResult Update(int booking, [FromForm(Name = "rating")] int? rating, [FromForm(Name = "comment")] string? comment)
    {
        var (model, review, denied) = OwnReview(booking);
        if (denied != null) return denied;
        var data = Validated(rating, comment);
        if (data == null) return BackWithErrors("review");

        
        review!.Rating = data.Value.Rating; review.Comment = data.Value.Comment; review.AdminId = null;
        _db.SaveChanges();

        With("status", "Your review of " + model!.Reference + " is updated.");
        return RedirectToRoute("customer.bookings.show", new { booking = model.BookingId });
    }

    [HttpDelete("bookings/{booking:int}/review", Name = "customer.bookings.review.destroy"), ValidateAntiForgeryToken]
    public IActionResult Destroy(int booking)
    {
        var (model, review, denied) = OwnReview(booking);
        if (denied != null) return denied;
        _db.Reviews.Remove(review!);
        _db.SaveChanges();

        With("status", "Your review of " + model!.Reference + " is deleted.");
        return RedirectToRoute("customer.bookings.show", new { booking = model.BookingId });
    }

    private (Booking? Booking, Review? Review, IActionResult? Denied) OwnReview(int booking)
    {
        var model = _db.Bookings.Find(booking);
        if (model == null) return (null, null, NotFound());
        var denied = Authorize(_policy.View(AuthManager.Customer()!, model));
        if (denied != null) return (null, null, denied);
        var review = _db.Reviews.FirstOrDefault(r => r.BookingId == model.BookingId);   
        return review == null ? (null, null, NotFound()) : (model, review, null);
    }

    private (int Rating, string? Comment)? Validated(int? rating, string? comment)
    {
        
        
        if (rating == null) ModelState.AddModelError("rating", "Choose from 1 to 5 stars.");
        else if (rating < 1 || rating > 5) ModelState.AddModelError("rating", "The Rating must be between 1 and 5.");
        if (comment?.Length > 500) ModelState.AddModelError("comment", "The Review must not be greater than 500 characters.");
        if (!ModelState.IsValid) return null;
        return (rating!.Value, !string.IsNullOrWhiteSpace(comment) ? comment.Trim() : null);
    }
}
