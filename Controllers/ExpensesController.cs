using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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

        // GET: /Expenses/Create
        public IActionResult Create()
        {
            ViewData["BudgetId"] = new SelectList(_context.Budgets, "BudgetId", "Name");
            return View();
        }

        // POST: /Expenses/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormCollection form)
        {
            // 1) Manually pull values
            var desc = form["Description"].ToString();
            var amtRaw = form["Amount"].ToString();
            var dateRaw = form["Date"].ToString();
            var budgetRaw = form["BudgetId"].ToString();

            // 2) Parse & validate
            if (string.IsNullOrWhiteSpace(desc))
                ModelState.AddModelError("Description", "Description is required.");

            if (!decimal.TryParse(amtRaw, out var amt))
                ModelState.AddModelError("Amount", "Amount must be a number.");

            if (!DateTime.TryParse(dateRaw, out var date))
                ModelState.AddModelError("Date", "Invalid date format.");

            if (!int.TryParse(budgetRaw, out var budgetId))
                ModelState.AddModelError("BudgetId", "You must select a Budget.");

            if (!ModelState.IsValid)
            {
                ViewData["BudgetId"] = new SelectList(_context.Budgets, "BudgetId", "Name", budgetId);
                return View();
            }

            // 3) Construct and save
            var expense = new Expense
            {
                Description = desc,
                Amount = amt,
                Date = date,
                BudgetId = budgetId
            };

            _context.Expenses.Add(expense);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: /Expenses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var expense = await _context.Expenses.FindAsync(id);
            if (expense == null) return NotFound();

            ViewData["BudgetId"] = new SelectList(_context.Budgets, "BudgetId", "Name", expense.BudgetId);
            return View(expense);
        }

        // POST: /Expenses/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, IFormCollection form)
        {
            // retrieve the existing entity
            var expense = await _context.Expenses.FindAsync(id);
            if (expense == null) return NotFound();

            // pull the new values
            var desc = form["Description"].ToString();
            var amtRaw = form["Amount"].ToString();
            var dateRaw = form["Date"].ToString();
            var budgetRaw = form["BudgetId"].ToString();

            // parse & validate
            if (string.IsNullOrWhiteSpace(desc))
                ModelState.AddModelError("Description", "Description is required.");

            if (!decimal.TryParse(amtRaw, out var amt))
                ModelState.AddModelError("Amount", "Amount must be a number.");

            if (!DateTime.TryParse(dateRaw, out var date))
                ModelState.AddModelError("Date", "Invalid date format.");

            if (!int.TryParse(budgetRaw, out var budgetId))
                ModelState.AddModelError("BudgetId", "You must select a Budget.");

            if (!ModelState.IsValid)
            {
                ViewData["BudgetId"] = new SelectList(_context.Budgets, "BudgetId", "Name", budgetId);
                return View(expense);
            }

            // assign & save
            expense.Description = desc;
            expense.Amount = amt;
            expense.Date = date;
            expense.BudgetId = budgetId;

            _context.Expenses.Update(expense);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
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
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var expense = await _context.Expenses.FindAsync(id);
            if (expense != null)
            {
                _context.Expenses.Remove(expense);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
