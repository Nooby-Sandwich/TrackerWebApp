using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
        public BudgetsController(ApplicationDbContext context) => _context = context;

        // GET: /Budgets
        public async Task<IActionResult> Index()
        {
            var budgets = await _context.Budgets.ToListAsync();
            return View(budgets);
        }

        // GET: /Budgets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var budget = await _context.Budgets.FirstOrDefaultAsync(b => b.BudgetId == id);
            if (budget == null) return NotFound();
            return View(budget);
        }

        // GET: /Budgets/Create
        public IActionResult Create() => View();

        // POST: /Budgets/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormCollection form)
        {
            var name = form["Name"].ToString();
            var limitRaw = form["Limit"].ToString();

            if (string.IsNullOrWhiteSpace(name))
                ModelState.AddModelError("Name", "Name is required.");
            if (!decimal.TryParse(limitRaw, out var limit))
                ModelState.AddModelError("Limit", "Limit must be a valid number.");

            if (!ModelState.IsValid)
                return View();

            var budget = new Budget { Name = name, Limit = limit };
            _context.Budgets.Add(budget);
            await _context.SaveChangesAsync();
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
        public async Task<IActionResult> Edit(int id, IFormCollection form)
        {
            var budget = await _context.Budgets.FindAsync(id);
            if (budget == null) return NotFound();

            var name = form["Name"].ToString();
            var limitRaw = form["Limit"].ToString();

            if (string.IsNullOrWhiteSpace(name))
                ModelState.AddModelError("Name", "Name is required.");
            if (!decimal.TryParse(limitRaw, out var limit))
                ModelState.AddModelError("Limit", "Limit must be a valid number.");

            if (!ModelState.IsValid)
                return View(budget);

            budget.Name = name;
            budget.Limit = limit;

            _context.Budgets.Update(budget);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: /Budgets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var budget = await _context.Budgets.FirstOrDefaultAsync(b => b.BudgetId == id);
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
