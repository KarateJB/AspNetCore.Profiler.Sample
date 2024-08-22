using System.Diagnostics;
using AspNetCore.Profiler.Mvc.Models;

namespace AspNetCore.Profiler.Mvc.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            // Update to get JWT from your Auth Server
            this.Response.Cookies.Append("access-token", "xxxxxxx");

            // DEMO
            //var traceId = Tracer.CurrentSpan.Context.TraceId;
            //var spanId = Tracer.CurrentSpan.Context.SpanId;
            //_logger.LogDebug($"TraceId: {traceId}, SpanId: {spanId}");

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
