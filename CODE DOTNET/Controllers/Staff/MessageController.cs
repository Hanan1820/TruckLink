
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TruckLink.Data;
using TruckLink.Models;
using TruckLink.Support.Middleware;
using TruckLink.Support.Services;

namespace TruckLink.Controllers.Staff;


[Route("staff"), TypeFilter(typeof(Authenticate), Arguments = new object[] { "staff" })]
public class MessageController : Controller
{
    private readonly Chat _chat;
    private readonly AppDbContext _db;

    public MessageController(Chat chat, AppDbContext db) { _chat = chat; _db = db; }

    [HttpGet("messages", Name = "staff.messages.index")]
    public IActionResult Index()
    {
        var threads = _chat.Threads(AuthManager.Staff()!);
        return threads.Count == 0
            ? View("~/Views/Staff/Messages.cshtml", new { threads, booking = (Booking?)null, messages = new List<Message>() })
            : RedirectToRoute("staff.messages.show", new { booking = threads.First().Booking.BookingId });
    }

    [HttpGet("messages/{booking:int}", Name = "staff.messages.show")]
    public IActionResult Show(int booking)
    {
        var model = Own(booking);
        if (model == null) return NotFound();
        _chat.MarkRead(model, "Staff");
        _db.Entry(model).Reference(b => b.Customer).Load();   
        return View("~/Views/Staff/Messages.cshtml", new
        {
            threads = _chat.Threads(AuthManager.Staff()!),
            booking = model,
            messages = _chat.Messages(model),
            open = _chat.IsOpen(model),
        });
    }

    [HttpPost("messages/{booking:int}", Name = "staff.messages.store"), ValidateAntiForgeryToken]
    public IActionResult Store(int booking, [FromForm(Name = "message_text")] string? messageText)
    {
        var model = Own(booking);
        if (model == null) return NotFound();
        if (!_chat.IsOpen(model)) return StatusCode(403, "This conversation is closed.");

        if (string.IsNullOrWhiteSpace(messageText)) ModelState.AddModelError("message_text", "The Message field is required.");
        else if (messageText.Length > 1000) ModelState.AddModelError("message_text", "The Message must not be greater than 1000 characters.");
        if (!ModelState.IsValid) return BackWithErrors();

        _chat.Send(model, "Staff", messageText!);
        return RedirectToRoute("staff.messages.show", new { booking = model.BookingId });
    }

    
    private Booking? Own(int booking)
    {
        var model = _db.Bookings.Find(booking);
        return model?.StaffId != null && model.StaffId == AuthManager.Staff()!.StaffId ? model : null;   
    }
}
