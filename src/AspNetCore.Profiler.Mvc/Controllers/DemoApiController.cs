using AspNetCore.Profiler.Dal.Models;
using AspNetCore.Profiler.Mvc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Trace;

namespace AspNetCore.Profiler.Mvc.Controllers;

[Route("api/[controller]")]
public class DemoApiController : ControllerBase
{
    private readonly ILogger logger;

    public DemoApiController(ILogger<DemoApiController> logger)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet]
    [Route("[action]/{id?}")]
    public async Task<IActionResult> TestOpenTelemetry([FromRoute] Guid? id, [FromServices] IHttpClientFactory httpClientFactory)
    {
        var traceId = Tracer.CurrentSpan.Context.TraceId;
        var spanId = Tracer.CurrentSpan.Context.SpanId;
        this.logger.LogDebug($"{HttpContext.Request.Path} - TraceId: {traceId}, SpanId: {spanId}");

        if (id.HasValue)
        {
            // Call the other API to see the Trace Id and Span Id
            var httpClient = httpClientFactory.CreateClient(Consts.HttpClientDemo);
            var payment = await httpClient.GetFromJsonAsync<Payment>($"api/PaymentApi/{id}");
            return Ok(payment);
        }
        else return Ok("Tested okay!");
    }
}
