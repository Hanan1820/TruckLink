
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TruckLink.Data;
using TruckLink.Models;
using TruckLink.Support.Helpers;
using TruckLink.Support.Middleware;
using TruckLink.Support.Services;

namespace TruckLink.Controllers.Customer;


[Route("customer"), TypeFilter(typeof(Authenticate), Arguments = new object[] { "customer" })]
public class InvoiceController : Controller
{
    private readonly PaymentLedger _ledger;
    private readonly AppDbContext _db;

    public InvoiceController(PaymentLedger ledger, AppDbContext db) { _ledger = ledger; _db = db; }

    [HttpGet("invoices", Name = "customer.invoices.index")]
    public IActionResult Index(string? status, int page = 1)
    {
        var customer = AuthManager.Customer()!;
        status = Payment.STATUSES.Contains(status) ? status : null;

        var mine = _db.Payments.Where(p => p.Booking.CustomerId == customer.CustomerId);   
        var query = mine.Include(p => p.Booking).ThenInclude(b => b.BookingServices).ThenInclude(l => l.Service)
            .Include(p => p.Booking).ThenInclude(b => b.Payments).AsQueryable();
        if (status != null) query = query.Where(p => p.PaymentStatus == status);
        query = query.OrderByDescending(p => p.TransactionDate).ThenByDescending(p => p.PaymentId);
        var payments = Paginator<Payment>.Paginate(query, page, 8);

        return View("~/Views/Customer/Invoices/Index.cshtml", new
        {
            payments,
            services = payments.Items.ToDictionary(p => p.PaymentId, p => _ledger.ServiceOf(p)),
            counts = mine.GroupBy(p => p.PaymentStatus).Select(g => new { g.Key, N = g.Count() }).ToDictionary(x => x.Key, x => x.N),
            paid = (double)mine.Where(p => p.PaymentStatus == "Paid").Sum(p => (decimal?)p.Amount ?? 0m),
            status,
        });
    }

    
    [HttpGet("invoices/{payment:int}/download", Name = "customer.invoices.download")]
    public async Task<IActionResult> Download(int payment, [FromServices] Microsoft.AspNetCore.Mvc.ViewEngines.ICompositeViewEngine engine)
    {
        var model = _db.Payments.Include(p => p.Booking).ThenInclude(b => b.Customer)
            .Include(p => p.Booking).ThenInclude(b => b.BookingServices).ThenInclude(l => l.Service)
            .Include(p => p.Booking).ThenInclude(b => b.Payments).FirstOrDefault(p => p.PaymentId == payment);
        if (model == null || model.Booking.CustomerId != AuthManager.Customer()!.CustomerId) return NotFound();   

        ViewData["payment"] = model;
        ViewData["service"] = _ledger.ServiceOf(model);
        
        return View("~/Views/Customer/Invoices/Document.cshtml");   
    }
}
