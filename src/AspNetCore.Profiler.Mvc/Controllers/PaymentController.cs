using Microsoft.EntityFrameworkCore;
using AspNetCore.Profiler.Dal;
using AspNetCore.Profiler.Dal.Models;
using StackExchange.Profiling;

namespace AspNetCore.Profiler.Mvc.Controllers;

public class PaymentController : Controller
{
    private readonly DemoDbContext dbContext;

    public PaymentController(DemoDbContext dbContext)
    {
        this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    // GET: Payments
    public async Task<IActionResult> Index()
    {
        return View(await dbContext.Payments.ToListAsync());
    }

    // GET: Payments/Details/5
    public async Task<IActionResult> Details(Guid? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var payment = await dbContext.Payments
            .FirstOrDefaultAsync(m => m.Id == id);
        if (payment == null)
        {
            return NotFound();
        }

        return View(payment);
    }

    // GET: Payments/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Payments/Create
    // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
    // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Item,Amount")] Payment payment)
    {
        if (ModelState.IsValid)
        {
            // Custom timing for MiniProfiler
            using (CustomTiming timing = MiniProfiler.Current.CustomTiming("MyLongRun", "Test command", executeType: "Test", includeStackTrace: true))
            {
                payment.Id = Guid.NewGuid();
                payment.CreateOn = DateTimeOffset.UtcNow;
                dbContext.Add(payment);
                await dbContext.SaveChangesAsync();

                timing.CommandString = $"Inserting {payment.Item} with amount {payment.Amount.ToString()}.";
            }

            return RedirectToAction(nameof(Index));
        }
        return View(payment);
    }

    // GET: Payments/Edit/5
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var payment = await dbContext.Payments.FindAsync(id);
        if (payment == null)
        {
            return NotFound();
        }
        return View(payment);
    }

    // POST: Payments/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, [Bind("Id,Item,Amount,CreateOn")] Payment payment)
    {
        if (id != payment.Id)
        {
            return NotFound();
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
            return RedirectToAction(nameof(Index));
        }
        return View(payment);
    }

    // GET: Payments/Delete/5
    public async Task<IActionResult> Delete(Guid? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var payment = await dbContext.Payments
            .FirstOrDefaultAsync(m => m.Id == id);
        if (payment == null)
        {
            return NotFound();
        }

        return View(payment);
    }

    // POST: Payments/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var payment = await dbContext.Payments.FindAsync(id);
        dbContext.Payments.Remove(payment);
        await dbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool PaymentExists(Guid? id)
    {
        return id.HasValue ? dbContext.Payments.Any(e => e.Id == id) : false;
    }
}
