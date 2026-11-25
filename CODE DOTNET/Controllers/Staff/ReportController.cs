
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TruckLink.Data;
using TruckLink.Models;
using TruckLink.Support.Middleware;
using TruckLink.Support.Services;
using AdminReports = TruckLink.Controllers.Admin.ReportController;

namespace TruckLink.Controllers.Staff;






[Route("staff"), TypeFilter(typeof(Authenticate), Arguments = new object[] { "staff" })]
public class ReportController : Controller
{
    private readonly JobWorkflow _jobs;
    private readonly AppDbContext _db;

    public ReportController(JobWorkflow jobs, AppDbContext db) { _jobs = jobs; _db = db; }

    [HttpGet("reports", Name = "staff.reports")]
    public IActionResult Index(string? period)
    {
        var staff = AuthManager.Staff()!;
        period = period != null && AdminReports.PERIODS.ContainsKey(period) ? period : "year";
        var today = DateTime.Today;
        var from = period switch
        {
            "month" => new DateTime(today.Year, today.Month, 1),
            "quarter" => today.AddMonths(-3).AddDays(1),     
            _ => new DateTime(today.Year, 1, 1),
        };
        var to = today.AddDays(1).AddTicks(-1);              

        var completed = _jobs.JobsQuery(staff).Include(b => b.Customer)
            .Where(b => b.BookingStatus == "Completed" && b.PickupDatetime >= from && b.PickupDatetime <= to)
            .OrderByDescending(b => b.PickupDatetime).ToList();
        var ids = completed.Select(b => b.BookingId).ToList();
        var reviews = _db.Reviews.Where(r => ids.Contains(r.BookingId)).ToDictionary(r => r.BookingId);

        return View("~/Views/Staff/Reports.cshtml", new
        {
            period, from, to,
            jobs = completed,
            reviews,
            hours = completed.Count * BookingAcceptance.SLOT_HOURS,
            km = completed.Sum(b => b.DistanceKm ?? 0m),
            average = reviews.Count > 0 ? reviews.Values.Average(r => (double)r.Rating) : (double?)null,
            staff,
        });
    }
}
