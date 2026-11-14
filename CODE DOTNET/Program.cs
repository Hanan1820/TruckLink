
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TruckLink.Controllers.Auth;
using TruckLink.Data;
using TruckLink.Support.Exceptions;
using TruckLink.Support.Middleware;
using TruckLink.Support.Providers;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;
var services = builder.Services;


TimeZoneInfo.TryFindSystemTimeZoneById(config["App:Timezone"] ?? "Asia/Riyadh", out var tz);   
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;


services.AddDbContext<AppDbContext>(o => o.UseSqlServer(config.GetConnectionString("DefaultConnection")));


services.AddControllersWithViews(o =>
{
    o.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());          
    o.Filters.Add<Handler>();                                            
    o.ModelBinderProviders.Insert(0, new TrimStringsBinderProvider());   
    o.MaxModelValidationErrors = 200;
}).AddViewOptions(v => v.HtmlHelperOptions.ClientValidationEnabled = false);


services.Configure<Microsoft.AspNetCore.Builder.ForwardedHeadersOptions>(_ => { });


services.AddDistributedMemoryCache();
services.AddSession(o =>
{
    o.IdleTimeout = TimeSpan.FromMinutes(config.GetValue("Session:Lifetime", 15));
    o.Cookie.Name = "trucklink_session";
    o.Cookie.HttpOnly = true;
    o.Cookie.SameSite = SameSiteMode.Lax;       
    o.Cookie.IsEssential = true;
});

services.AddLocalization(o => o.ResourcesPath = "Resources");   
services.AddCors();                                                


AppServiceProvider.Register(services);
AppServiceProvider.Boot(services);
AuthServiceProvider.Boot(services);       
RouteServiceProvider.Boot(services);      
EventServiceProvider.Boot(services);
BroadcastServiceProvider.Boot(services);

var app = builder.Build();

TruckLink.Support.Services.StorageChange.SupportPhone = config["TruckLink:SupportPhone"] ?? "+966 11 000 0000";   


if (!app.Environment.IsDevelopment()) app.UseExceptionHandler("/error");
app.UseStaticFiles();                                    
Kernel.UseGlobalMiddleware(app);                         
app.UseRouting();
app.UseCors(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());   
Kernel.UseWebGroup(app);                                 
app.UseAuthentication();
app.UseMiddleware<EnsureAccountIsActive>();              
app.UseAuthorization();
app.UseRateLimiter();








app.MapControllers();
app.Map("/error", () => Results.Problem());   




app.Run();
