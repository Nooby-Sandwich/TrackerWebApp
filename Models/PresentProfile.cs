// File: TrackerWebApp/Models/PresetProfile.cs
using System;
using System.Collections.Generic;
using System.Linq;

namespace TrackerWebApp.Models
{
    /// <summary>
    /// Defines preset allocations for budgets based on salary.
    /// </summary>
    public class PresetProfile
    {
        private static readonly Dictionary<string, PresetProfile> Presets = new()
        {
            ["Default"] = new(new List<BudgetAllocation>
            {
                new("Tax", 0.30m),
                new("Essentials", 0.40m),
                new("Leisure", 0.10m),
                new("Savings", 0.20m)
            }),
            ["High-Savings"] = new(new List<BudgetAllocation>
            {
                new("Tax", 0.30m),
                new("Essentials", 0.30m),
                new("Leisure", 0.05m),
                new("Savings", 0.35m)
            }),
            ["Travel"] = new(new List<BudgetAllocation>
            {
                new("Tax", 0.25m),
                new("Essentials", 0.30m),
                new("Travel", 0.20m),
                new("Savings", 0.25m)
            })
        };

        public List<BudgetAllocation> Allocations { get; }

        private PresetProfile(List<BudgetAllocation> allocations)
        {
            Allocations = allocations;
        }

        /// <summary>
        /// Retrieves a preset profile by key.
        /// </summary>
        public static PresetProfile GetPreset(string key)
        {
            if (!Presets.TryGetValue(key, out var preset))
                throw new ArgumentException($"Invalid preset key: {key}");
            return preset;
        }

        /// <summary>
        /// Calculates budgets based on the salary and preset allocations.
        /// </summary>
        public List<Budget> CalculateBudgets(decimal salary)
        {
            return Allocations
                .Select(a => new Budget
                {
                    Name = a.Category,
                    Limit = Math.Round(a.Percentage * salary, 2)
                })
                .ToList();
        }
    }

    /// <summary>
    /// Represents a single budget allocation category and percentage.
    /// </summary>
    public record BudgetAllocation(string Category, decimal Percentage);
}
