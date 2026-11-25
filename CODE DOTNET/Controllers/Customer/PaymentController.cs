
using Microsoft.AspNetCore.Mvc;
using TruckLink.Data;
using TruckLink.Support;
using TruckLink.Support.Middleware;
using TruckLink.Support.Policies;
using TruckLink.Support.Services;
using TruckLink.ViewModels.Customer;

namespace TruckLink.Controllers.Customer;


[Route("customer"), TypeFilter(typeof(Authenticate), Arguments = new object[] { "customer" })]
public class PaymentController : Controller
{
    private readonly PaymentLedger _ledger;
    private readonly AppDbContext _db;
    private readonly BookingPolicy _policy;

    public PaymentController(PaymentLedger ledger, AppDbContext db, BookingPolicy policy) { _ledger = ledger; _db = db; _policy = policy; }

    
    [HttpGet("bookings/{booking:int}/pay", Name = "customer.bookings.pay")]
    public IActionResult Create(int booking)
    {
        var model = _db.Bookings.Find(booking);
        if (model == null) return NotFound();
        var denied = Authorize(_policy.View(AuthManager.Customer()!, model));
        if (denied != null) return denied;
        return RedirectToRoute("customer.bookings.show", new { booking = model.BookingId, pay = 1 });
    }

    [HttpPost("bookings/{booking:int}/pay", Name = "customer.bookings.pay.store"), ValidateAntiForgeryToken]
    public IActionResult Store(int booking, PayRequest request)
    {
        var customer = AuthManager.Customer()!;
        request.User = customer;
        var model = _db.Bookings.Find(booking);
        if (model == null) return NotFound();
        if (!ModelState.IsValid) return BackWithErrors(request.ErrorBag);   
        var denied = Authorize(_policy.View(customer, model));
        if (denied != null) return denied;

        if (!_ledger.CanPay(model))
        {
            With("status", "Nothing is due on " + model.Reference + ".");
            return RedirectToRoute("customer.bookings.show", new { booking = model.BookingId });
        }

        var method = request.PaymentMethod;
        var newCard = request.UsesNewCard();
        var declined = newCard && Card.Declined(request.CardNumber!);
        var ids = request.Lines.ToList();
        var payments = _ledger.Pay(model, ids, method, declined);

        if (payments.Count == 0)
        {
            ModelState.AddModelError("lines", "Those services are already paid.");
            return BackWithErrors("payment");   
        }

        if (declined)
        {
            ModelState.AddModelError("card_number", "Your bank declined this card. Nothing was charged - try another card or method.");
            return BackWithErrors("payment");   
        }

        if (newCard && request.SaveCard == true)
        {
            customer.CardHolderName = request.CardHolderName!.Trim();
            customer.CardNumber = Card.Mask(request.CardNumber!);
            customer.CardType = Card.Type(request.CardNumber!, method);
            customer.ExpiryDate = request.ExpiryDate!.Trim();
            _db.SaveChanges();
        }

        var amount = payments.Sum(p => p.Amount);
        With("status", "Paid " + Support.Helpers.Helpers.Sar(amount) + " SAR by " + method + ". "
            + (payments.Count == 1 ? "Invoice " + payments[0].InvoiceNumber + " is" : payments.Count + " invoices are") + " in Invoices.");
        return RedirectToRoute("customer.bookings.show", new { booking = model.BookingId });
    }
}
