using AspNetCore.Profiler.Dal;
using AspNetCore.Profiler.Dal.Models;
using AspNetCore.Profiler.Mvc.Models;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Trace;
using StackExchange.Profiling;

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

    [HttpPost]
    [Route("Create")]
    public async Task<IActionResult> Create([FromBody] Payment payment)
    {
        if (ModelState.IsValid)
        {
            using CustomTiming timing = MiniProfiler.Current.CustomTiming("MyLongRun", "Test command", executeType: "Test", includeStackTrace: true);
            payment.Id = Guid.NewGuid();
            payment.CreateOn = DateTimeOffset.UtcNow;
            dbcontext.Add(payment);
            await dbcontext.SaveChangesAsync();

            timing.CommandString = $"Inserting {payment.Item} with amount {payment.Amount.ToString()}.";

            return Ok(payment);
        }
        return BadRequest(ModelState);
    }

    [HttpPost]
    [Route("Edit/{id}")]
    public async Task<IActionResult> Edit(Guid id, [FromBody] Payment payment)
    {
        if (id != payment.Id)
        {
            return BadRequest("Payment ID mismatch");
        }

        if (ModelState.IsValid)
        {
            try
            {
                this.dbcontext.Attach(payment);
                this.dbcontext.Update(payment);
                await dbcontext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PaymentExists(payment.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return Ok(payment);
        }
        return BadRequest(ModelState);
    }

    [HttpPost]
    [Route("Delete/{id}")]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var payment = await dbcontext.Payments.FindAsync(id);
        if (payment == null)
        {
            return NotFound();
        }

        dbcontext.Payments.Remove(payment);
        await dbcontext.SaveChangesAsync();
        return Ok();
    }

    private bool PaymentExists(Guid id)
    {
        return dbcontext.Payments.Any(e => e.Id == id);
    }
}
