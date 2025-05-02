// Data/DataSeeder.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using TrackerWebApp.Models;

namespace TrackerWebApp.Data
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var ctx = services.GetRequiredService<ApplicationDbContext>();
            var users = services.GetRequiredService<UserManager<IdentityUser>>();

            // 1) apply migrations
            await ctx.Database.MigrateAsync();

            // 2) ensure our demo user
            var demoEmail = "demo@budgetapp.local";
            var user = await users.FindByEmailAsync(demoEmail);
            if (user == null)
            {
                user = new IdentityUser { UserName = demoEmail, Email = demoEmail };
                await users.CreateAsync(user, "Password123!");
            }

            // 3) bail if budgets already exist
            if (ctx.Budgets.Any(b => b.UserId == user.Id))
                return;

            // 4) create budgets
            var categories = new[] {
                ("Housing Rent",    20000m),
                ("Food",            10000m),
                ("Travel",          5000m),
                ("Groceries",       7000m),
                ("Other",           3000m),
                ("Savings",         8000m),
                ("Investments",     6000m),
            };

            var budList = categories
                .Select(c => new Budget
                {
                    Name = c.Item1,
                    Limit = c.Item2,
                    UserId = user.Id
                })
                .ToList();
            ctx.Budgets.AddRange(budList);
            await ctx.SaveChangesAsync();

            // 5) spread some expenses over the past 6 months
            var rand = new Random();
            var today = DateTime.Today;

            var expList = budList.SelectMany(b =>
            {
                // each category: one expense per month, random up to 90% of budget
                return Enumerable.Range(0, 6).Select(offset =>
                {
                    var dt = new DateTime(today.Year, today.Month, 1)
                             .AddMonths(-offset)
                             .AddDays(rand.Next(0, 25));
                    // spend between 50%–90% of limit/6
                    var amt = Math.Round((decimal)rand.NextDouble() * (b.Limit * 0.9m / 6m) + (b.Limit * 0.5m / 6m), 2);
                    return new Expense
                    {
                        BudgetId = b.BudgetId,
                        UserId = b.UserId,
                        Amount = amt,
                        Date = dt,
                        Description = $"{b.Name} on {dt:MMM yyyy}"
                    };
                });
            }).ToList();

            ctx.Expenses.AddRange(expList);
            await ctx.SaveChangesAsync();
        }
    }
}
