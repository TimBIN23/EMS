using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EM.Data;
using EM.Models;

namespace EM.Controllers
{
    public class PerformancesController : Controller
    {
        private readonly EmsContext _context;

        public PerformancesController(EmsContext context)
        {
            _context = context;
        }

        // GET: Performances
        public async Task<IActionResult> Index()
        {
            try
            {
                var performances = await _context.Performances
                    .Include(p => p.Employee)
                    .OrderByDescending(p => p.ReviewDate)
                    .ThenBy(p => p.Employee.LastName)
                    .ToListAsync();

                Console.WriteLine($"Loaded {performances.Count} performance records");
                return View(performances);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading performances: {ex.Message}");
                TempData["Error"] = "Error loading performance records. Please try again.";
                return View(new List<Performance>());
            }
        }

        // GET: Performances/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                TempData["Error"] = "Performance ID not provided.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var performance = await _context.Performances
                    .Include(p => p.Employee)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (performance == null)
                {
                    TempData["Error"] = "Performance record not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(performance);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading performance details: {ex.Message}");
                TempData["Error"] = "Error loading performance details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Performances/Create
        public IActionResult Create()
        {
            try
            {
                // Get employees for dropdown with full name
                var employees = _context.Employees
                    .OrderBy(e => e.LastName)
                    .ThenBy(e => e.FirstName)
                    .Select(e => new {
                        e.Id,
                        FullName = $"{e.FirstName} {e.LastName} ({e.Department})"
                    });

                ViewData["EmployeeId"] = new SelectList(employees, "Id", "FullName");

                // Set default values
                var performance = new Performance
                {
                    ReviewDate = DateTime.Today,
                    Score = 3.0m // Default average score
                };

                return View(performance);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading create form: {ex.Message}");
                TempData["Error"] = "Error loading create form.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Performances/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Performance performance)
        {
            Console.WriteLine("=== CREATE PERFORMANCE START ===");
            Console.WriteLine($"ModelState IsValid: {ModelState.IsValid}");

            // Validate score
            if (performance.Score < 1 || performance.Score > 5)
            {
                ModelState.AddModelError("Score", "Score must be between 1.0 and 5.0.");
            }

            // Validate review date
            if (performance.ReviewDate > DateTime.Today)
            {
                ModelState.AddModelError("ReviewDate", "Review date cannot be in the future.");
            }

            if (!ModelState.IsValid)
            {
                Console.WriteLine("=== VALIDATION ERRORS ===");
                foreach (var key in ModelState.Keys)
                {
                    var state = ModelState[key];
                    foreach (var error in state.Errors)
                    {
                        Console.WriteLine($"Field: {key}, Error: {error.ErrorMessage}");
                    }
                }

                // Reload employees for dropdown
                var employees = _context.Employees
                    .OrderBy(e => e.LastName)
                    .ThenBy(e => e.FirstName)
                    .Select(e => new {
                        e.Id,
                        FullName = $"{e.FirstName} {e.LastName} ({e.Department})"
                    });
                ViewData["EmployeeId"] = new SelectList(employees, "Id", "FullName", performance.EmployeeId);

                TempData["Error"] = "Please fix the validation errors below.";
                return View(performance);
            }

            try
            {
                Console.WriteLine("Attempting to save performance record...");
                _context.Add(performance);
                await _context.SaveChangesAsync();

                Console.WriteLine("=== CREATE PERFORMANCE SUCCESS ===");
                TempData["Success"] = "Performance record created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating performance: {ex.Message}");

                // Reload employees for dropdown
                var employees = _context.Employees
                    .OrderBy(e => e.LastName)
                    .ThenBy(e => e.FirstName)
                    .Select(e => new {
                        e.Id,
                        FullName = $"{e.FirstName} {e.LastName} ({e.Department})"
                    });
                ViewData["EmployeeId"] = new SelectList(employees, "Id", "FullName", performance.EmployeeId);

                TempData["Error"] = "Error creating performance record. Please try again.";
                return View(performance);
            }
        }

        // GET: Performances/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                TempData["Error"] = "Performance ID not provided.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var performance = await _context.Performances.FindAsync(id);
                if (performance == null)
                {
                    TempData["Error"] = "Performance record not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Get employees for dropdown with full name
                var employees = _context.Employees
                    .OrderBy(e => e.LastName)
                    .ThenBy(e => e.FirstName)
                    .Select(e => new {
                        e.Id,
                        FullName = $"{e.FirstName} {e.LastName} ({e.Department})"
                    });

                ViewData["EmployeeId"] = new SelectList(employees, "Id", "FullName", performance.EmployeeId);
                return View(performance);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading performance for edit: {ex.Message}");
                TempData["Error"] = "Error loading performance for editing.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Performances/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Performance performance)
        {
            if (id != performance.Id)
            {
                TempData["Error"] = "Performance ID mismatch.";
                return RedirectToAction(nameof(Index));
            }

            Console.WriteLine("=== EDIT PERFORMANCE START ===");

            // Validate score
            if (performance.Score < 1 || performance.Score > 5)
            {
                ModelState.AddModelError("Score", "Score must be between 1.0 and 5.0.");
            }

            // Validate review date
            if (performance.ReviewDate > DateTime.Today)
            {
                ModelState.AddModelError("ReviewDate", "Review date cannot be in the future.");
            }

            if (!ModelState.IsValid)
            {
                Console.WriteLine("=== VALIDATION ERRORS ===");
                foreach (var key in ModelState.Keys)
                {
                    var state = ModelState[key];
                    foreach (var error in state.Errors)
                    {
                        Console.WriteLine($"Field: {key}, Error: {error.ErrorMessage}");
                    }
                }

                // Reload employees for dropdown
                var employees = _context.Employees
                    .OrderBy(e => e.LastName)
                    .ThenBy(e => e.FirstName)
                    .Select(e => new {
                        e.Id,
                        FullName = $"{e.FirstName} {e.LastName} ({e.Department})"
                    });
                ViewData["EmployeeId"] = new SelectList(employees, "Id", "FullName", performance.EmployeeId);

                TempData["Error"] = "Please fix the validation errors below.";
                return View(performance);
            }

            try
            {
                Console.WriteLine("Attempting to update performance...");
                _context.Update(performance);
                await _context.SaveChangesAsync();

                Console.WriteLine("=== EDIT PERFORMANCE SUCCESS ===");
                TempData["Success"] = "Performance record updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PerformanceExists(performance.Id))
                {
                    TempData["Error"] = "Performance record not found.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating performance: {ex.Message}");

                // Reload employees for dropdown
                var employees = _context.Employees
                    .OrderBy(e => e.LastName)
                    .ThenBy(e => e.FirstName)
                    .Select(e => new {
                        e.Id,
                        FullName = $"{e.FirstName} {e.LastName} ({e.Department})"
                    });
                ViewData["EmployeeId"] = new SelectList(employees, "Id", "FullName", performance.EmployeeId);

                TempData["Error"] = "Error updating performance record. Please try again.";
                return View(performance);
            }
        }

        // GET: Performances/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                TempData["Error"] = "Performance ID not provided.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var performance = await _context.Performances
                    .Include(p => p.Employee)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (performance == null)
                {
                    TempData["Error"] = "Performance record not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(performance);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading performance for deletion: {ex.Message}");
                TempData["Error"] = "Error loading performance for deletion.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Performances/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                Console.WriteLine("=== DELETE PERFORMANCE START ===");
                var performance = await _context.Performances.FindAsync(id);
                if (performance == null)
                {
                    TempData["Error"] = "Performance record not found.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Performances.Remove(performance);
                await _context.SaveChangesAsync();

                Console.WriteLine("=== DELETE PERFORMANCE SUCCESS ===");
                TempData["Success"] = "Performance record deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting performance: {ex.Message}");
                TempData["Error"] = "Error deleting performance record. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        private bool PerformanceExists(int id)
        {
            return _context.Performances.Any(e => e.Id == id);
        }
    }
}