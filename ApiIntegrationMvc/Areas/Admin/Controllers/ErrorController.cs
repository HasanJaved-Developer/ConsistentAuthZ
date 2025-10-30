using CentralizedLogging.Sdk.Abstractions;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary.Cache;

namespace ApiIntegrationMvc.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ErrorController : Controller
    {
        private readonly ICentralizedLoggingClient _centralizedlogs;
        private readonly ICacheAccessProvider _cache;
        private readonly IHttpContextAccessor _http;
        public ErrorController(ICentralizedLoggingClient centralizedlogs, ICacheAccessProvider cache, IHttpContextAccessor http) => (_centralizedlogs, _cache, _http) = (centralizedlogs, cache, http);

        public async Task<IActionResult> Index(CancellationToken ct)
        {
            string token = await _cache.GetAccessTokenAsync(ct);

            var result = await _centralizedlogs.GetAllErrorAsync(ct);

  
            return View(result.OrderByDescending(v => v.Id));
        }
    }
}
