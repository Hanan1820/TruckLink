
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
public class CustomerController : ConfirmsDeletion
{
    public const int PER_PAGE = 8;

    public static readonly Dictionary<string, string> SORTS = new() { ["name"] = "Name A–Z", ["newest"] = "Newest first", ["bookings"] = "Most bookings" };

    
    public static readonly string[] OPEN = { "Pending", "Accepted", "Picked Up", "In Transit", "In Storage", "Out for Delivery" };

    private readonly AppDbContext _db;
    private readonly IConfiguration _config;
    private readonly Microsoft.AspNetCore.Identity.IPasswordHasher<object> _hasher;

    public CustomerController(AppDbContext db, IConfiguration config, Microsoft.AspNetCore.Identity.IPasswordHasher<object> hasher)
    {
        _db = db; _config = config; _hasher = hasher;
    }

    [HttpGet("customers", Name = "admin.customers.index")]
    public IActionResult Index(string? status, string? city, string? sort, string? q, int page = 1)
    {
        var cities = _config.GetSection("TruckLink:Cities").Get<string[]>() ?? Array.Empty<string>();
        var f = new
        {
            status = Models.Customer.STATUSES.Contains(status) ? status : null,
            city = cities.Contains(city) ? city : null,
            sort = sort != null && SORTS.ContainsKey(sort) ? sort : "name",
            q = (q ?? "").Trim(),
        };
        var byStatus = _db.Customers.GroupBy(c => c.AccountStatus).Select(g => new { g.Key, N = g.Count() }).ToDictionary(x => x.Key, x => x.N);

        var query = _db.Customers.AsQueryable();
        if (f.status != null) query = query.Where(c => c.AccountStatus == f.status);
        if (f.city != null) query = query.Where(c => c.City == f.city);
        if (f.q != "") query = Search(query, f.q);

        var ordered = f.sort switch
        {
            "newest" => query.OrderByDescending(c => c.CreatedAt).ThenByDescending(c => c.CustomerId),
            "bookings" => query.OrderByDescending(c => c.Bookings.Count).ThenBy(c => c.FirstName),
            _ => query.OrderBy(c => c.FirstName).ThenBy(c => c.LastName),
        };
        var customers = Paginator<Models.Customer>.Paginate(ordered, page, PER_PAGE);

        return View("~/Views/Admin/Customers/Index.cshtml", new
        {
            customers,
            bookingCounts = customers.Items.ToDictionary(c => c.CustomerId, c => _db.Bookings.Count(b => b.CustomerId == c.CustomerId)),   
            filters = f,
            byStatus,
            total = byStatus.Values.Sum(),
            withOpen = _db.Bookings.Where(b => OPEN.Contains(b.BookingStatus)).Select(b => b.CustomerId).Distinct().Count(),
            cities = _db.Customers.Select(c => c.City).Distinct().OrderBy(c => c).ToList(),
            highlight = TempData["saved.id"],
        });
    }

    [HttpGet("customers/create", Name = "admin.customers.create")]
    public IActionResult Create() =>
        View("~/Views/Admin/Customers/Create.cshtml", new { customer = new Models.Customer { AccountStatus = "Active" } });

    [HttpPost("customers", Name = "admin.customers.store"), ValidateAntiForgeryToken]
    public IActionResult Store(CustomerRequest request)
    {
        if (!ModelState.IsValid) return BackWithErrors();
        var d = request.Customer();
        var customer = new Models.Customer
        {
            FirstName = (string)d["first_name"]!, LastName = (string)d["last_name"]!, Email = (string)d["email"]!, Phone = (string)d["phone"]!,
            Password = _hasher.HashPassword(null!, (string)d["password"]!), City = (string)d["city"]!, Address = d["address"] as string,
            AccountStatus = (string)d["account_status"]!,
        };
        _db.Customers.Add(customer);   
        _db.SaveChanges();

        Saved("customer:" + customer.CustomerId, customer.CustomerId, created: true);
        return RedirectToRoute("admin.customers.show", new { customer = customer.CustomerId });
    }

    [HttpGet("customers/{customer:int}", Name = "admin.customers.show")]
    public IActionResult Show(int customer)
    {
        var model = _db.Customers.Find(customer);
        if (model == null) return NotFound();
        var bookings = _db.Bookings.Where(b => b.CustomerId == model.CustomerId).OrderByDescending(b => b.CreatedAt).ToList();
        var ids = bookings.Select(b => b.BookingId).ToList();

        return View("~/Views/Admin/Customers/Show.cshtml", new
        {
            customer = model,
            bookings,
            activity = new
            {
                completed = bookings.Count(b => b.BookingStatus == "Completed"),
                open = bookings.Count(b => OPEN.Contains(b.BookingStatus)),
                cancelled = bookings.Count(b => b.BookingStatus == "Cancelled"),
                paid = (double)_db.Payments.Where(p => ids.Contains(p.BookingId) && p.PaymentStatus == "Paid").Sum(p => (decimal?)p.Amount ?? 0m),
                rating = _db.Reviews.Where(r => r.CustomerId == model.CustomerId).Average(r => (double?)r.Rating),
            },
        }, DeleteGuard(model));
    }

    [HttpGet("customers/{customer:int}/edit", Name = "admin.customers.edit")]
    public IActionResult Edit(int customer)
    {
        var model = _db.Customers.Find(customer);
        if (model == null) return NotFound();
        return View("~/Views/Admin/Customers/Edit.cshtml", new
        {
            customer = model,
            open = _db.Bookings.Count(b => b.CustomerId == model.CustomerId && OPEN.Contains(b.BookingStatus)),
        });
    }

    [HttpPut("customers/{customer:int}", Name = "admin.customers.update"), ValidateAntiForgeryToken]
    public IActionResult Update(int customer, CustomerRequest request)
    {
        var model = _db.Customers.Find(customer);
        if (model == null) return NotFound();
        request.Record = model;   
        if (!ModelState.IsValid) return BackWithErrors();

        var d = request.Customer();   
        model.FirstName = (string)d["first_name"]!; model.LastName = (string)d["last_name"]!; model.Email = (string)d["email"]!; model.Phone = (string)d["phone"]!;
        model.City = (string)d["city"]!; model.Address = d["address"] as string; model.AccountStatus = (string)d["account_status"]!;
        _db.SaveChanges();

        Saved("customer:" + model.CustomerId, model.CustomerId, created: false);
        return RedirectToRoute("admin.customers.show", new { customer = model.CustomerId });
    }

    
    [HttpPost("customers/{customer:int}/suspend", Name = "admin.customers.suspend"), ValidateAntiForgeryToken]
    public IActionResult Suspend(int customer)
    {
        var model = _db.Customers.Find(customer);
        if (model == null) return NotFound();
        model.AccountStatus = "Suspended";
        _db.SaveChanges();

        With("status", model.FullName + " is now Suspended and can no longer sign in or book.");
        return RedirectToRoute("admin.customers.show", new { customer = model.CustomerId });
    }

    [HttpDelete("customers/{customer:int}", Name = "admin.customers.destroy"), ValidateAntiForgeryToken]
    public IActionResult Destroy(int customer, [FromForm(Name = "confirm")] string? confirm)
    {
        var model = _db.Customers.Find(customer);
        if (model == null) return NotFound();

        ConfirmDeletion(confirm, model.Email, "email address");
        if (((List<string>)DeleteGuard(model)["uses"]!).Count > 0)
            RefuseDeletion(model.FullName + " is referenced by other records and can only be suspended.");

        var name = model.FullName;
        using (var tx = _db.Database.BeginTransaction()) { _db.Customers.Remove(model); _db.SaveChanges(); tx.Commit(); }   

        With("status", "Customer " + name + " was deleted.");
        return RedirectToRoute("admin.customers.index");
    }

    private Dictionary<string, object?> DeleteGuard(Models.Customer customer)
    {
        var bookings = _db.Bookings.Count(b => b.CustomerId == customer.CustomerId);
        var reviews = _db.Reviews.Count(r => r.CustomerId == customer.CustomerId);
        var messages = _db.Messages.Count(m => m.CustomerId == customer.CustomerId);
        var notices = _db.Announcements.Count(a => a.CustomerId == customer.CustomerId);
        var uses = new[]
        {
            bookings > 0 ? "has <b>" + bookings + " " + Helpers.Plural("booking", bookings) + "</b>" : null,
            reviews > 0 ? "wrote <b>" + reviews + " " + Helpers.Plural("review", reviews) + "</b>" : null,
            messages > 0 ? "appears in <b>" + messages + " chat " + Helpers.Plural("message", messages) + "</b>" : null,
            notices > 0 ? "has received <b>" + notices + " " + Helpers.Plural("announcement", notices) + "</b>" : null,
        }.Where(x => x != null).Select(x => x!).ToList();
        return new Dictionary<string, object?> { ["uses"] = uses };
    }

    private static IQueryable<Models.Customer> Search(IQueryable<Models.Customer> q, string term)
    {
        var digits = new string(term.Where(char.IsDigit).ToArray());
        var local = digits.TrimStart('0');
        return q.Where(c => EF.Functions.Like(c.FirstName + " " + c.LastName, "%" + term + "%")
            || EF.Functions.Like(c.Email, "%" + term + "%") || EF.Functions.Like(c.City, "%" + term + "%")
            || (digits.Length >= 4 && EF.Functions.Like(c.Phone, "%" + local + "%")));
    }
}
