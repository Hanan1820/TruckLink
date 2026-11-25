
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TruckLink.Controllers.Concerns;
using TruckLink.Data;
using TruckLink.Models;
using TruckLink.Support.Exceptions;
using TruckLink.Support.Helpers;
using TruckLink.Support.Middleware;
using TruckLink.ViewModels.Admin;

namespace TruckLink.Controllers.Admin;





[Route("admin"), TypeFilter(typeof(Authenticate), Arguments = new object[] { "admin" })]   
public class AdminAccountController : ConfirmsDeletion
{
    public static readonly Dictionary<string, string[]> ROLE_ICONS = new()
    {
        ["Super Admin"] = new[] { "bi-shield-lock", "--p" },
        ["Operations Admin"] = new[] { "bi-person-badge", "--in" },
    };

    private readonly AppDbContext _db;
    public AdminAccountController(AppDbContext db) => _db = db;

    [HttpGet("admins", Name = "admin.admins.index")]
    public IActionResult Index(string? role, string? q, int page = 1)
    {
        role = Models.Admin.ROLES.Contains(role) ? role : null;
        q = (q ?? "").Trim();
        var byRole = _db.Admins.GroupBy(a => a.AdminRole).Select(g => new { g.Key, N = g.Count() }).ToDictionary(x => x.Key, x => x.N);

        var query = _db.Admins.AsQueryable();
        if (role != null) query = query.Where(a => a.AdminRole == role);
        if (q != "") query = query.Where(a => EF.Functions.Like(a.FirstName + " " + a.LastName, "%" + q + "%") || EF.Functions.Like(a.Email, "%" + q + "%"));

        
        var paged = Paginator<Models.Admin>.Paginate(query.OrderBy(a => a.AdminId), page, 8);
        var counts = paged.Items.ToDictionary(a => a.AdminId, a => new Dictionary<string, int>
        {
            ["vehicles_count"] = _db.Vehicles.Count(v => v.AdminId == a.AdminId),
            ["staff_count"] = _db.Staff.Count(s => s.AdminId == a.AdminId),
            ["services_count"] = _db.Services.Count(s => s.AdminId == a.AdminId),
            ["item_categories_count"] = _db.ItemCategories.Count(c => c.AdminId == a.AdminId),
        });

        return View("~/Views/Admin/Admins/Index.cshtml", new
        {
            admins = paged,
            withCounts = counts,
            byRole,
            total = byRole.Values.Sum(),
            sent = _db.Announcements.Count(a => a.AdminId != null),
            canManage = AuthManager.Admin()!.IsSuperAdmin(),
            highlight = TempData["saved.id"],   
        });
    }

    [HttpGet("admins/create", Name = "admin.admins.create")]
    public IActionResult Create()
    {
        if (!SuperOnly()) return StatusCode(403, "Only a Super Admin can manage admin accounts.");
        return View("~/Views/Admin/Admins/Create.cshtml", new { account = new Models.Admin { AdminRole = "Operations Admin" } });
    }

    [HttpPost("admins", Name = "admin.admins.store"), ValidateAntiForgeryToken]
    public IActionResult Store(AdminAccountRequest request, [FromServices] Microsoft.AspNetCore.Identity.IPasswordHasher<object> hasher)
    {
        if (!AdminAccountRequest.Authorize(AuthManager.Admin())) return Forbid();   
        if (!ModelState.IsValid) return BackWithErrors();

        var data = request.Account();
        var account = new Models.Admin
        {
            FirstName = (string)data["first_name"]!, LastName = (string)data["last_name"]!, Email = (string)data["email"]!, Phone = (string)data["phone"]!,
            Password = hasher.HashPassword(null!, (string)data["password"]!), AdminRole = request.AdminRole,
        };
        _db.Admins.Add(account);   
        _db.SaveChanges();

        Saved("admin:" + account.AdminId, account.AdminId, created: true);
        return RedirectToRoute("admin.admins.show", new { admin = account.AdminId });
    }

    [HttpGet("admins/{admin:int}", Name = "admin.admins.show")]
    public IActionResult Show(int admin)
    {
        var model = _db.Admins.Find(admin);
        if (model == null) return NotFound();
        var counts = Counts(model);   

        return View("~/Views/Admin/Admins/Show.cshtml", new
        {
            account = model,
            counts,
            recent = _db.Announcements.Include(a => a.Customer).Include(a => a.Staff).Where(a => a.AdminId == model.AdminId)
                .OrderByDescending(a => a.SentAt).Take(6).ToList(),
            canManage = AuthManager.Admin()!.IsSuperAdmin(),
        }, DeleteGuard(model));
    }

    [HttpGet("admins/{admin:int}/edit", Name = "admin.admins.edit")]
    public IActionResult Edit(int admin)
    {
        if (!SuperOnly()) return StatusCode(403, "Only a Super Admin can manage admin accounts.");
        var model = _db.Admins.Find(admin);
        return model == null ? NotFound() : View("~/Views/Admin/Admins/Edit.cshtml", new { account = model });
    }

    [HttpPut("admins/{admin:int}", Name = "admin.admins.update"), ValidateAntiForgeryToken]
    public IActionResult Update(int admin, AdminAccountRequest request)
    {
        if (!AdminAccountRequest.Authorize(AuthManager.Admin())) return Forbid();
        var model = _db.Admins.Find(admin);
        if (model == null) return NotFound();
        request.Record = model;   
        if (!ModelState.IsValid) return BackWithErrors();

        if (model.IsSuperAdmin() && request.AdminRole != "Super Admin" && SuperAdmins() == 1)
            throw ValidationException.WithMessages("admin_role", "TruckLink needs at least one Super Admin: make another admin Super Admin first.");

        var data = request.Account();   
        model.FirstName = (string)data["first_name"]!; model.LastName = (string)data["last_name"]!;
        model.Email = (string)data["email"]!; model.Phone = (string)data["phone"]!; model.AdminRole = request.AdminRole;
        _db.SaveChanges();

        Saved("admin:" + model.AdminId, model.AdminId, created: false);
        return RedirectToRoute("admin.admins.show", new { admin = model.AdminId });
    }

    [HttpDelete("admins/{admin:int}", Name = "admin.admins.destroy"), ValidateAntiForgeryToken]
    public IActionResult Destroy(int admin, [FromForm(Name = "confirm")] string? confirm)
    {
        if (!SuperOnly()) return StatusCode(403, "Only a Super Admin can manage admin accounts.");
        var model = _db.Admins.Find(admin);
        if (model == null) return NotFound();

        ConfirmDeletion(confirm, model.Email, "email address");
        var guard = DeleteGuard(model);
        var blocked = (string?)guard["blocked"];
        if (blocked != null || ((List<string>)guard["uses"]!).Count > 0)
            RefuseDeletion(blocked ?? model.FullName + " registered or sent records that still reference this account.");

        var name = model.FullName;
        _db.Admins.Remove(model);
        _db.SaveChanges();

        With("status", "Admin account " + name + " was deleted.");
        return RedirectToRoute("admin.admins.index");
    }

    
    private bool SuperOnly() => AuthManager.Admin()!.IsSuperAdmin();

    private int SuperAdmins() => _db.Admins.Count(a => a.AdminRole == "Super Admin");

    private Dictionary<string, int> Counts(Models.Admin a) => new()
    {
        ["vehicles_count"] = _db.Vehicles.Count(v => v.AdminId == a.AdminId),
        ["staff_count"] = _db.Staff.Count(s => s.AdminId == a.AdminId),
        ["services_count"] = _db.Services.Count(s => s.AdminId == a.AdminId),
        ["item_categories_count"] = _db.ItemCategories.Count(c => c.AdminId == a.AdminId),
        ["moderated_reviews_count"] = _db.Reviews.Count(r => r.AdminId == a.AdminId),
        ["announcements_count"] = _db.Announcements.Count(n => n.AdminId == a.AdminId),
    };

    private Dictionary<string, object?> DeleteGuard(Models.Admin admin)
    {
        var c = Counts(admin);
        string? Phrase(int n, string what, string verb) => n > 0 ? verb + " <b>" + n + " " + Helpers.Plural(what, n) + "</b>" : null;
        var uses = new[]
        {
            Phrase(c["vehicles_count"], "vehicle", "registered"),
            Phrase(c["staff_count"], "staff member", "registered"),
            Phrase(c["services_count"] + c["item_categories_count"], "catalogue entry", "created"),
            Phrase(c["moderated_reviews_count"], "review", "moderated"),
            Phrase(c["announcements_count"], "announcement", "sent"),
        }.Where(x => x != null).Select(x => x!).ToList();

        string? blocked =
            admin.AdminId == AuthManager.Admin()!.AdminId ? "You cannot delete the account you are signed in with."
            : admin.IsSuperAdmin() && SuperAdmins() == 1 ? "This is the only Super Admin; TruckLink always needs one."
            : null;
        return new Dictionary<string, object?> { ["uses"] = uses, ["blocked"] = blocked };
    }
}
