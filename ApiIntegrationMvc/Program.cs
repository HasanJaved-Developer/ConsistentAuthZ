using ApiIntegrationMvc;
using CentralizedLogging.Sdk.Extensions;
using Microsoft.AspNetCore.Authentication.Cookies;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using SharedLibrary;
using UserManagement.Sdk.Extensions;


var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.WithProperty("Application", "IntegrationPortal")
    .Enrich.FromLogContext()
    .Enrich.With(new ActivityTraceEnricher())  // <-- custom enricher
    .CreateLogger();

builder.Host.UseSerilog();

// Add MemoryCache globally
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor(); // required for the above

builder.Services.AddUserManagementSdk();
builder.Services.AddCentralizedLoggingSdk();


// Add services to the container.
builder.Services.AddControllersWithViews();

// Cookie auth for the web app (UI)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath = "/Account/Login";
        o.LogoutPath = "/Account/Logout";
        o.AccessDeniedPath = "/Account/Denied";
        // optional:
        o.SlidingExpiration = true;
        o.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

builder.Services.AddAuthorization();

// OpenTelemetry + Jaeger
builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService(
        serviceName: "IntegrationPortal", // <- change per project
        serviceVersion: "1.0.0"))
    .WithTracing(t => t
        .AddAspNetCoreInstrumentation(o => { o.RecordException = true; })
        .AddHttpClientInstrumentation()
        .AddOtlpExporter(o =>
        {
            o.Endpoint = new Uri($"http://{builder.Configuration["JAEGER_HOST"]}:4317"); // host -> docker            
            o.Protocol = OtlpExportProtocol.Grpc;
        }));

var app = builder.Build();
app.UseSerilogRequestLogging(); // structured request logs

app.MapGet("/health", () => Results.Ok("OK"));

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Login}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "root_to_login",
    pattern: "",
    defaults: new { area = "Account", controller = "Login", action = "Index" });

app.Run();
