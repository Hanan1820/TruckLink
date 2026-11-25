
using System.Dynamic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TruckLink.Data;
using TruckLink.Models;
using TruckLink.Support.Helpers;
using TruckLink.Support.Middleware;
using TruckLink.Support.Services;
using TruckLink.ViewModels.Admin;

namespace TruckLink.Controllers.Admin;






[Route("admin"), TypeFilter(typeof(Authenticate), Arguments = new object[] { "admin" })]
public class AnnouncementController : Controller
{
    public static readonly Dictionary<string, string> SOURCES = new() { ["admins"] = "Sent by admins", ["system"] = "System notices" };

    private readonly AppDbContext _db;
    public AnnouncementController(AppDbContext db) => _db = db;

    [HttpGet("announcements", Name = "admin.announcements.index")]
    public IActionResult Index(string? source, string? q, int page = 1)
    {
        source = source != null && SOURCES.ContainsKey(source) ? source : "admins";
        q = (q ?? "").Trim();

        var rows = _db.Announcements.AsQueryable();
        rows = source == "admins" ? rows.Where(a => a.AdminId != null) : rows.Where(a => a.AdminId == null);
        if (q != "") rows = rows.Where(a => EF.Functions.Like(a.Title, "%" + q + "%") || EF.Functions.Like(a.Content, "%" + q + "%"));

        
        var grouped = rows.GroupBy(a => new { a.AdminId, a.AnnouncementType, a.Title, a.Content, a.SentAt })
            .Select(g => new SendRow
            {
                AdminId = g.Key.AdminId, AnnouncementType = g.Key.AnnouncementType, Title = g.Key.Title, Content = g.Key.Content, SentAt = g.Key.SentAt,
                Recipients = g.Count(), ReadCount = g.Count(x => x.IsRead),
                Customers = g.Count(x => x.CustomerId != null), Staff = g.Count(x => x.StaffId != null),
                CustomerId = g.Min(x => x.CustomerId), StaffId = g.Min(x => x.StaffId),
            }).OrderByDescending(r => r.SentAt);
        var sends = Paginator<SendRow>.Paginate(grouped, page, 8);

        return View("~/Views/Admin/Announcements.cshtml", new
        {
            sends,
            names = Names(sends.Items),
            counts = new
            {
                admins = _db.Announcements.Where(a => a.AdminId != null).Select(a => a.Title + "|" + a.SentAt).Distinct().Count(),   
                system = _db.Announcements.Count(a => a.AdminId == null),
            },
            unread = Announcement.ScopeUnread(_db.Announcements).Count(),
            total = _db.Announcements.Count(),
            customers = _db.Customers.Where(c => c.AccountStatus == "Active").OrderBy(c => c.FirstName).ThenBy(c => c.LastName).ToList(),
            staff = _db.Staff.Where(s => s.StaffStatus == "Active").OrderBy(s => s.FirstName).ThenBy(s => s.LastName).ToList(),
            source,
        });
    }

    [HttpPost("announcements", Name = "admin.announcements.store"), ValidateAntiForgeryToken]
    public IActionResult Store(AnnouncementRequest request, [FromServices] Notifier notifier)
    {
        if (!ModelState.IsValid) return BackWithErrors();

        var audience = request.Audience;
        var customerRecipients = audience switch
        {
            "All customers" => _db.Customers.Where(c => c.AccountStatus == "Active").ToList(),
            "One customer" => _db.Customers.Where(c => c.CustomerId == request.CustomerId).ToList(),
            _ => new List<Models.Customer>(),
        };
        var staffRecipients = audience switch
        {
            "All staff" => _db.Staff.Where(s => s.StaffStatus == "Active").ToList(),
            "One staff member" => _db.Staff.Where(s => s.StaffId == request.StaffId).ToList(),
            _ => new List<Models.Staff>(),
        };
        var adminId = AuthManager.Admin()!.AdminId;
        var sentAt = DateTime.Now; sentAt = new DateTime(sentAt.Ticks - sentAt.Ticks % TimeSpan.TicksPerSecond);   
        var title = request.Title.Trim();
        var content = request.Content.Trim();

        using (var tx = _db.Database.BeginTransaction())   
        {
            foreach (var c in customerRecipients) notifier.Customer(c, request.AnnouncementType, title, content, adminId, sentAt);
            foreach (var s in staffRecipients) notifier.Staff(s, request.AnnouncementType, title, content, adminId, sentAt);
            tx.Commit();
        }

        var count = customerRecipients.Count + staffRecipients.Count;
        var who = count == 1
            ? (customerRecipients.FirstOrDefault()?.FullName ?? staffRecipients.First().FullName)
            : count + " " + (audience.Contains("customer") ? "customers" : "staff members");
        With("status", "“" + title + "” was sent to " + who + ".");
        return RedirectToRoute("admin.announcements.index");
    }

    
    private dynamic Names(List<SendRow> sends)
    {
        var one = sends.Where(s => s.Recipients == 1).ToList();
        var cIds = one.Where(s => s.CustomerId != null).Select(s => s.CustomerId!.Value).ToList();
        var sIds = one.Where(s => s.StaffId != null).Select(s => s.StaffId!.Value).ToList();
        var aIds = sends.Where(s => s.AdminId != null).Select(s => s.AdminId!.Value).ToList();
        IDictionary<string, object?> o = new ExpandoObject();
        o["Customers"] = _db.Customers.Where(c => cIds.Contains(c.CustomerId)).ToDictionary(c => c.CustomerId);
        o["Staff"] = _db.Staff.Where(s => sIds.Contains(s.StaffId)).ToDictionary(s => s.StaffId);
        o["Admins"] = _db.Admins.Where(a => aIds.Contains(a.AdminId)).ToDictionary(a => a.AdminId);
        return o;
    }

    
    public class SendRow
    {
        public int? AdminId { get; set; }
        public string AnnouncementType { get; set; } = "";
        public string Title { get; set; } = "";
        public string Content { get; set; } = "";
        public DateTime SentAt { get; set; }
        public int Recipients { get; set; }
        public int ReadCount { get; set; }
        public int Customers { get; set; }
        public int Staff { get; set; }
        public int? CustomerId { get; set; }
        public int? StaffId { get; set; }
    }
}
