using AspNetCore.Profiler.Dal;
using AspNetCore.Profiler.Dal.Models;
using Microsoft.EntityFrameworkCore;

namespace AspNetCore.Profiler.Mvc.Controllers
{
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
            var payment = await this.dbcontext.Payments
                .FirstOrDefaultAsync(m => m.Id == id);
            return Ok(payment);
        }
    }
}
