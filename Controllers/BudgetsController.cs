using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrackerWebApp.Data;
using TrackerWebApp.Models;

namespace TrackerWebApp.Controllers
{
    [Authorize]
    public class BudgetsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BudgetsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Budgets
        public async Task<IActionResult> Index()
        {
            var budgets = await _context.Budgets.ToListAsync();
            return View(budgets);
        }

        // GET: /Budgets/Create
        public IActionResult Create()
            => View();

        // POST: /Budgets/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormCollection form)
        {
            // Grab raw form values
            var name = form["Name"].ToString();
            var limitRaw = form["Limit"].ToString();
            decimal limit = 0;
            var ok = decimal.TryParse(limitRaw, out limit);

            // Simple validation
            if (string.IsNullOrWhiteSpace(name))
                ModelState.AddModelError("Name", "Name is required.");
            if (!ok)
                ModelState.AddModelError("Limit", "Limit must be a number.");

            if (!ModelState.IsValid)
            {
                // Render form again with errors
                return View();
            }

            // Build & save
            var budget = new Budget
            {
                Name = name,
                Limit = limit
            };

            _context.Budgets.Add(budget);
            await _context.SaveChangesAsync();

            // Verify it got an ID
            // Debug.WriteLine($"Saved budget id: {budget.BudgetId}");

            return RedirectToAction(nameof(Index));
        }

        // GET: /Budgets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var budget = await _context.Budgets.FindAsync(id);
            if (budget == null) return NotFound();

            return View(budget);
        }

        // POST: /Budgets/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BudgetId,Name,Limit")] Budget budget)
        {
            if (id != budget.BudgetId) return BadRequest();
            if (!ModelState.IsValid) return View(budget);

            try
            {
                _context.Update(budget);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Budgets.AnyAsync(e => e.BudgetId == budget.BudgetId))
                    return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Budgets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var budget = await _context.Budgets
                .FirstOrDefaultAsync(b => b.BudgetId == id);
            if (budget == null) return NotFound();

            return View(budget);
        }

        // POST: /Budgets/Delete/5
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var budget = await _context.Budgets.FindAsync(id);
            if (budget != null)
            {
                _context.Budgets.Remove(budget);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}