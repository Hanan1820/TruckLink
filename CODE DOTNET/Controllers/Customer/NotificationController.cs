
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TruckLink.Data;
using TruckLink.Models;
using TruckLink.Support.Helpers;
using TruckLink.Support.Middleware;

namespace TruckLink.Controllers.Customer;


[Route("customer"), TypeFilter(typeof(Authenticate), Arguments = new object[] { "customer" })]
public class NotificationController : Controller
{
    private readonly AppDbContext _db;
    public NotificationController(AppDbContext db) => _db = db;

    [HttpGet("notifications", Name = "customer.notifications")]
    public IActionResult Index(int page = 1)
    {
        var customer = AuthManager.Customer()!;
        var query = _db.Announcements.Where(a => a.CustomerId == customer.CustomerId)
            .OrderByDescending(a => a.SentAt).ThenByDescending(a => a.AnnouncementId);
        var notices = Paginator<Announcement>.Paginate(query, page, 10);

        
        var @new = notices.Items.Where(a => !a.IsRead).Select(a => a.AnnouncementId).ToList();
        Announcement.ScopeUnread(_db.Announcements.Where(a => a.CustomerId == customer.CustomerId))
            .ExecuteUpdate(a => a.SetProperty(x => x.IsRead, true));

        return View("~/Views/Customer/Notifications.cshtml", new { notices, @new });
    }
}
