using AspNetCore.Profiler.Dal;
using AspNetCore.Profiler.Dal.Models;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Trace;
using StackExchange.Profiling;

namespace AspNetCore.Profiler.Mvc.Controllers;

[Route("api/[controller]")]
public class PaymentApiController : ControllerBase
{
    private readonly ILogger logger;
    private readonly DemoDbContext dbContext;

    public PaymentApiController(
            ILogger<PaymentApiController> logger,
            DemoDbContext dbContext)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
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

        var payment = await this.dbContext.Payments
            .FirstOrDefaultAsync(m => m.Id == id);
        return Ok(payment);
    }

    [HttpPost]
    [Route("Create")]
    public async Task<IActionResult> Create([FromBody][Bind("Item,Amount")] Payment payment)
    {
        if (ModelState.IsValid)
        {
            using CustomTiming timing = MiniProfiler.Current.CustomTiming("MyLongRun", "Test command", executeType: "Test", includeStackTrace: true);
            payment.Id = Guid.NewGuid();
            payment.CreateOn = DateTimeOffset.UtcNow;
            dbContext.Add(payment);
            await dbContext.SaveChangesAsync();

            timing.CommandString = $"Inserting {payment.Item} with amount {payment.Amount.ToString()}.";

            return Ok(payment);
        }
        return BadRequest(ModelState);
    }

    [HttpPost]
    [Route("Edit/{id}")]
    public async Task<IActionResult> Edit([FromRoute] Guid id, [FromBody][Bind("Id,Item,Amount,CreateOn")] Payment payment)
    {
        if (id != payment.Id)
        {
            return BadRequest("Payment ID mismatch");
        }

        if (ModelState.IsValid)
        {
            try
            {
                this.dbContext.Attach(payment);
                this.dbContext.Update(payment);
                await dbContext.SaveChangesAsync();
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
    [Route("Delete")]
    public async Task<IActionResult> Delete([FromBody] Guid id)
    {
        var payment = await dbContext.Payments.FindAsync(id);
        if (payment == null)
        {
            return NotFound();
        }

        dbContext.Payments.Remove(payment);
        await dbContext.SaveChangesAsync();
        return Ok();
    }

    private bool PaymentExists(Guid? id)
    {
        return id.HasValue ? dbContext.Payments.Any(e => e.Id == id) : false;
    }
}
