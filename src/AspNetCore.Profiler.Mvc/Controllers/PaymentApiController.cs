using AspNetCore.Profiler.Dal;
using AspNetCore.Profiler.Dal.Models;
using AspNetCore.Profiler.Mvc.Models;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Trace;

namespace AspNetCore.Profiler.Mvc.Controllers;

[Route("api/[controller]")]
public class PaymentApiController : ControllerBase
{
    private readonly ILogger logger;
    private readonly DemoDbContext dbcontext;

    public PaymentApiController(
            ILogger<PaymentApiController> logger,
            DemoDbContext dbContext)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.dbcontext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    [HttpGet]
    [Route("{id}")]
    public async Task<ActionResult<Payment>> Get([FromRoute] Guid id)
    {
#if DEBUG
        var traceId = Tracer.CurrentSpan.Context.TraceId;
        var spanId = Tracer.CurrentSpan.Context.SpanId;
        this.logger.LogDebug($"{HttpContext.Request.Path} - TraceId: {traceId}, SpanId: {spanId}");
        this.logger.LogDebug($"OpenTelemetry header: {HttpContext.Request.Headers["traceparent"].FirstOrDefault()?.ToString()}");
#endif

        var payment = await this.dbcontext.Payments
            .FirstOrDefaultAsync(m => m.Id == id);
        return Ok(payment);
    }

    [HttpGet]
    [Route("[action]/{id}")]
    public async Task<IActionResult> TestOpenTelemetry([FromRoute] Guid id, [FromServices] IHttpClientFactory httpClientFactory)
    {
#if DEBUG
        var traceId = Tracer.CurrentSpan.Context.TraceId;
        var spanId = Tracer.CurrentSpan.Context.SpanId;
        this.logger.LogDebug($"{HttpContext.Request.Path} - TraceId: {traceId}, SpanId: {spanId}");
#endif

        // Call the other API to see the Trace Id and Span Id
        // var httpClient = httpClientFactory.CreateClient(Consts.HttpClientDemo);
        // var payment = await httpClient.GetFromJsonAsync<Payment>($"api/PaymentApi/{id}");
        // return Ok(payment);
        return Ok("Tested okay!");
    }

}
