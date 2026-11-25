
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using TruckLink.Support.Auth;
using TruckLink.Support.Exceptions;
using TruckLink.Support.Policies;

namespace TruckLink.Controllers;







public abstract class Controller : Microsoft.AspNetCore.Mvc.Controller
{
    protected AuthManager AuthManager => HttpContext.RequestServices.GetRequiredService<AuthManager>();

    
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        ViewData["errors"] = ReadTemp<Dictionary<string, string[]>>("errors") ?? new Dictionary<string, string[]>();
        ViewData["errorBag"] = TempData["errorBag"] as string ?? "default";
        ViewData["old"] = ReadTemp<Dictionary<string, string>>("old") ?? new Dictionary<string, string>();
        ViewData["status"] = TempData["status"];
        base.OnActionExecuting(context);
    }

    private T? ReadTemp<T>(string key) => TempData[key] is string json ? JsonSerializer.Deserialize<T>(json) : default;

    

    
    protected ViewResult View(string view, object data)
    {
        foreach (var p in data.GetType().GetProperties()) ViewData[p.Name] = p.GetValue(data);
        return base.View(view);
    }

    

    
    protected IActionResult Back() => Redirect(Request.Headers.Referer.ToString() is { Length: > 0 } r ? r : "/");

    
    protected void With(string key, string? message) => TempData[key] = message;

    
    protected void WithErrors(Dictionary<string, string> errors, string bag = "default")
    {
        TempData["errors"] = JsonSerializer.Serialize(errors.ToDictionary(kv => kv.Key, kv => new[] { kv.Value }));
        TempData["errorBag"] = bag;
    }

    
    protected IActionResult BackWithErrors(string bag = "default")
    {
        var errors = ModelState.Where(kv => kv.Value!.Errors.Count > 0)
            .ToDictionary(kv => kv.Key, kv => kv.Value!.Errors.Select(e => e.ErrorMessage).ToArray());
        TempData["errors"] = JsonSerializer.Serialize(errors);
        TempData["errorBag"] = bag;
        FlashInput();
        return Back();
    }

    
    protected void FlashInput()
    {
        if (!Request.HasFormContentType) return;
        var old = Request.Form.Keys.Where(k => !Handler.DontFlash.Contains(k) && k != "__RequestVerificationToken")
            .ToDictionary(k => k, k => Request.Form[k].ToString());
        TempData["old"] = JsonSerializer.Serialize(old);
    }

    
    protected IActionResult RedirectToRouteWithErrors(string route, Dictionary<string, string> errors)
    {
        WithErrors(errors);
        return RedirectToRoute(route);
    }

    
    protected void Saved(string key, int id, bool created)
    {
        TempData["saved.key"] = key;
        TempData["saved.id"] = id;
        TempData["saved.created"] = created;
    }

    
    protected ViewResult View(string view, object data, IDictionary<string, object?> extra)
    {
        foreach (var kv in extra) ViewData[kv.Key] = kv.Value;
        return View(view, data);
    }

    
    protected ViewResult View(string view, IDictionary<string, object?> data)
    {
        foreach (var kv in data) ViewData[kv.Key] = kv.Value;
        return base.View(view);
    }

    

    
    protected IActionResult Invalid(ValidationException e)
    {
        foreach (var (field, messages) in e.Errors)
            foreach (var m in messages) ModelState.AddModelError(field, m);
        return BackWithErrors();
    }

    
    protected IActionResult? Authorize(PolicyResponse response) => response switch
    {
        PolicyResponse.NotFound => NotFound(),
        PolicyResponse.Deny => Forbid(),
        _ => null,
    };

    
    protected IActionResult? Authorize(bool allowed) => allowed ? null : Forbid();
}
