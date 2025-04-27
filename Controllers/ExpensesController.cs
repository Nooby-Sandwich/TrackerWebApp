using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TrackerWebApp.Data;
using TrackerWebApp.Models;

namespace TrackerWebApp.Controllers
{
    [Authorize]
    public class ExpensesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ExpensesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Expenses
        public async Task<IActionResult> Index()
        {
            var expenses = await _context.Expenses
                                         .Include(e => e.Budget)
                                         .ToListAsync();
            return View(expenses);
        }


        // GET: /Expenses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var expense = await _context.Expenses
                .Include(e => e.Budget)
                .FirstOrDefaultAsync(e => e.ExpenseId == id);
            if (expense == null) return NotFound();

            return View(expense);
        }

        // GET: /Expenses/Create
        public IActionResult Create()
        {
            PopulateBudgetsDropDown();
            return View();
        }

        // POST: /Expenses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Expense expense)
        {
            if (ModelState.IsValid)
            {
                await _context.Expenses.AddAsync(expense);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            PopulateBudgetsDropDown(expense.BudgetId);
            return View(expense);
        }

        // GET: /Expenses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var expense = await _context.Expenses.FindAsync(id);
            if (expense == null) return NotFound();

            PopulateBudgetsDropDown(expense.BudgetId);
            return View(expense);
        }

        // POST: /Expenses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Expense expense)
        {
            if (id != expense.ExpenseId) return BadRequest();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(expense);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await ExpenseExists(expense.ExpenseId)) return NotFound();
                    throw;
                }
            }

            PopulateBudgetsDropDown(expense.BudgetId);
            return View(expense);
        }

        // GET: /Expenses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var expense = await _context.Expenses
                .Include(e => e.Budget)
                .FirstOrDefaultAsync(e => e.ExpenseId == id);
            if (expense == null) return NotFound();

            return View(expense);
        }

        // POST: /Expenses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var expense = await _context.Expenses.FindAsync(id);
            if (expense == null) return NotFound();

            _context.Expenses.Remove(expense);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> ExpenseExists(int id)
            => await _context.Expenses.AnyAsync(e => e.ExpenseId == id);

        private void PopulateBudgetsDropDown(object? selectedBudget = null)
        {
            ViewData["BudgetId"] = new SelectList(
                _context.Budgets,
                nameof(Budget.BudgetId),
                nameof(Budget.Name),
                selectedBudget
            );
        }
    }
}
