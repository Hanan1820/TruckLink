
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TruckLink.Data;
using TruckLink.Models;
using TruckLink.Support.Helpers;
using TruckLink.Support.Middleware;

namespace TruckLink.Controllers.Staff;


[Route("staff"), TypeFilter(typeof(Authenticate), Arguments = new object[] { "staff" })]
public class NotificationController : Controller
{
    private readonly AppDbContext _db;
    public NotificationController(AppDbContext db) => _db = db;

    [HttpGet("notifications", Name = "staff.notifications")]
    public IActionResult Index(int page = 1)
    {
        var staff = AuthManager.Staff()!;
        var query = _db.Announcements.Where(a => a.StaffId == staff.StaffId).OrderByDescending(a => a.SentAt).ThenByDescending(a => a.AnnouncementId);
        var notices = Paginator<Announcement>.Paginate(query, page, 10);
        var @new = notices.Items.Where(a => !a.IsRead).Select(a => a.AnnouncementId).ToList();

        
        Announcement.ScopeUnread(_db.Announcements.Where(a => a.StaffId == staff.StaffId)).Where(a => a.AnnouncementType != "Job Assigned")
            .ExecuteUpdate(a => a.SetProperty(x => x.IsRead, true));

        return View("~/Views/Staff/Notifications.cshtml", new { notices, @new });
    }
}
