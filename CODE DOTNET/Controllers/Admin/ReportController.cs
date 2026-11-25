
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using TruckLink.Support.Middleware;
using TruckLink.Support.Services;

namespace TruckLink.Controllers.Admin;


[Route("admin"), TypeFilter(typeof(Authenticate), Arguments = new object[] { "admin" })]
public class ReportController : Controller
{
    
    public static readonly Dictionary<string, string> PERIODS = new() { ["month"] = "This month", ["quarter"] = "Last 3 months", ["year"] = "This year" };

    [HttpGet("reports", Name = "admin.reports.index")]
    public IActionResult Index() => View("~/Views/Admin/Reports/Index.cshtml", new { types = ReportBuilder.TYPES });

    
    [HttpGet("reports/{type:regex(^(warehouse|revenue|satisfaction)$)}", Name = "admin.reports.show")]
    public IActionResult Show(string type, string? period, string? from, string? to, [FromServices] ReportBuilder reports)
    {
        var (p, f, t) = Period(period, from, to);
        return View("~/Views/Admin/Reports/Show.cshtml", new
        {
            type,
            meta = ReportBuilder.TYPES[type],
            period = p,
            from = f,
            to = t,
            report = reports.Build(type, f, t),
            generatedAt = DateTime.Now,
            generatedBy = AuthManager.Admin(),
        });
    }

    
    private static (string Period, DateTime From, DateTime To) Period(string? periodQuery, string? fromQuery, string? toQuery)
    {
        var today = DateTime.Today;
        var from = Date(fromQuery);
        var to = Date(toQuery);
        if (from != null || to != null)
        {
            var toD = new[] { to ?? today, today }.Min();
            var fromD = new[] { from ?? new DateTime(toD.Year, toD.Month, 1), toD }.Min();
            return ("custom", fromD, toD);
        }
        var period = periodQuery != null && PERIODS.ContainsKey(periodQuery) ? periodQuery : "year";
        return (period, period switch
        {
            "month" => new DateTime(today.Year, today.Month, 1),
            "quarter" => today.AddMonths(-3).AddDays(1),   
            _ => new DateTime(today.Year, 1, 1),
        }, today);
    }

    private static DateTime? Date(string? value) =>
        value != null && Regex.IsMatch(value, @"^\d{4}-\d{2}-\d{2}$") && DateTime.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)
            ? d.Date : null;
}
