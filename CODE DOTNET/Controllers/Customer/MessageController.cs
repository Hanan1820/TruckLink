
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TruckLink.Data;
using TruckLink.Models;
using TruckLink.Support.Middleware;
using TruckLink.Support.Policies;
using TruckLink.Support.Services;

namespace TruckLink.Controllers.Customer;


[Route("customer"), TypeFilter(typeof(Authenticate), Arguments = new object[] { "customer" })]
public class MessageController : Controller
{
    private readonly Chat _chat;
    private readonly AppDbContext _db;
    private readonly BookingPolicy _policy;

    public MessageController(Chat chat, AppDbContext db, BookingPolicy policy) { _chat = chat; _db = db; _policy = policy; }

    [HttpGet("messages", Name = "customer.messages.index")]
    public IActionResult Index()
    {
        var threads = _chat.Threads(AuthManager.Customer()!);
        return threads.Count == 0
            ? View("~/Views/Customer/Messages.cshtml", new { threads, booking = (Booking?)null, messages = new List<Message>() })
            : RedirectToRoute("customer.messages.show", new { booking = threads.First().Booking.BookingId });
    }

    [HttpGet("messages/{booking:int}", Name = "customer.messages.show")]
    public IActionResult Show(int booking)
    {
        var model = _db.Bookings.Find(booking);
        if (model == null) return NotFound();
        var denied = Authorize(_policy.View(AuthManager.Customer()!, model));
        if (denied != null) return denied;
        if (model.StaffId == null) return NotFound();   

        _chat.MarkRead(model, "Customer");
        _db.Entry(model).Reference(b => b.Driver).Load();   
        return View("~/Views/Customer/Messages.cshtml", new
        {
            threads = _chat.Threads(AuthManager.Customer()!),
            booking = model,
            messages = _chat.Messages(model),
            open = _chat.IsOpen(model),
        });
    }

    [HttpPost("messages/{booking:int}", Name = "customer.messages.store"), ValidateAntiForgeryToken]
    public IActionResult Store(int booking, [FromForm(Name = "message_text")] string? messageText)
    {
        var model = _db.Bookings.Find(booking);
        if (model == null) return NotFound();
        var denied = Authorize(_policy.View(AuthManager.Customer()!, model));
        if (denied != null) return denied;
        if (!_chat.IsOpen(model)) return StatusCode(403, "This conversation is closed.");   

        
        if (string.IsNullOrWhiteSpace(messageText)) ModelState.AddModelError("message_text", "The Message field is required.");
        else if (messageText.Length > 1000) ModelState.AddModelError("message_text", "The Message must not be greater than 1000 characters.");
        if (!ModelState.IsValid) return BackWithErrors();

        _chat.Send(model, "Customer", messageText!);
        return RedirectToRoute("customer.messages.show", new { booking = model.BookingId });
    }
}
