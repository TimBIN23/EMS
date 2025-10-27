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
    public class CompliancesController : Controller
    {
        private readonly EmsContext _context;

        public CompliancesController(EmsContext context)
        {
            _context = context;
        }

        // GET: Compliances
        public async Task<IActionResult> Index()
        {
            try
            {
                var compliances = await _context.Compliances
                    .Include(c => c.Employee)
                    .OrderByDescending(c => c.AcknowledgedOn)
                    .ThenBy(c => c.Employee.LastName)
                    .ToListAsync();

                Console.WriteLine($"Loaded {compliances.Count} compliance records");
                return View(compliances);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading compliances: {ex.Message}");
                TempData["Error"] = "Error loading compliance records. Please try again.";
                return View(new List<Compliance>());
            }
        }

        // GET: Compliances/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                TempData["Error"] = "Compliance ID not provided.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var compliance = await _context.Compliances
                    .Include(c => c.Employee)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (compliance == null)
                {
                    TempData["Error"] = "Compliance record not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(compliance);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading compliance details: {ex.Message}");
                TempData["Error"] = "Error loading compliance details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Compliances/Create
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
                var compliance = new Compliance
                {
                    AcknowledgedOn = DateTime.Today,
                    Status = "Pending"
                };

                return View(compliance);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading create form: {ex.Message}");
                TempData["Error"] = "Error loading create form.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Compliances/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EmployeeId,Policy,AcknowledgedOn,Status")] Compliance compliance)
        {
            Console.WriteLine("=== CREATE COMPLIANCE START ===");
            Console.WriteLine($"ModelState IsValid: {ModelState.IsValid}");

            // Remove navigation property from validation
            ModelState.Remove("Employee");

            // Validate future dates
            if (compliance.AcknowledgedOn > DateTime.Today)
            {
                ModelState.AddModelError("AcknowledgedOn", "Acknowledgment date cannot be in the future.");
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
                ViewData["EmployeeId"] = new SelectList(employees, "Id", "FullName", compliance.EmployeeId);

                TempData["Error"] = "Please fix the validation errors below.";
                return View(compliance);
            }

            try
            {
                Console.WriteLine("Attempting to save compliance record...");
                _context.Add(compliance);
                await _context.SaveChangesAsync();

                Console.WriteLine("=== CREATE COMPLIANCE SUCCESS ===");
                TempData["Success"] = "Compliance record created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating compliance: {ex.Message}");

                // Reload employees for dropdown
                var employees = _context.Employees
                    .OrderBy(e => e.LastName)
                    .ThenBy(e => e.FirstName)
                    .Select(e => new {
                        e.Id,
                        FullName = $"{e.FirstName} {e.LastName} ({e.Department})"
                    });
                ViewData["EmployeeId"] = new SelectList(employees, "Id", "FullName", compliance.EmployeeId);

                TempData["Error"] = "Error creating compliance record. Please try again.";
                return View(compliance);
            }
        }

        // GET: Compliances/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                TempData["Error"] = "Compliance ID not provided.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var compliance = await _context.Compliances.FindAsync(id);
                if (compliance == null)
                {
                    TempData["Error"] = "Compliance record not found.";
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

                ViewData["EmployeeId"] = new SelectList(employees, "Id", "FullName", compliance.EmployeeId);
                return View(compliance);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading compliance for edit: {ex.Message}");
                TempData["Error"] = "Error loading compliance for editing.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Compliances/Edit/5
        // POST: Compliances/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,EmployeeId,Policy,AcknowledgedOn,Status")] Compliance compliance)
        {
            if (id != compliance.Id)
            {
                TempData["Error"] = "Compliance ID mismatch.";
                return RedirectToAction(nameof(Index));
            }

            Console.WriteLine("=== EDIT COMPLIANCE START ===");

            // Remove navigation property from validation
            ModelState.Remove("Employee");

            // Validate future dates
            if (compliance.AcknowledgedOn > DateTime.Today)
            {
                ModelState.AddModelError("AcknowledgedOn", "Acknowledgment date cannot be in the future.");
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
                ViewData["EmployeeId"] = new SelectList(employees, "Id", "FullName", compliance.EmployeeId);

                TempData["Error"] = "Please fix the validation errors below.";
                return View(compliance);
            }

            try
            {
                Console.WriteLine("Attempting to update compliance...");
                _context.Update(compliance);
                await _context.SaveChangesAsync();

                Console.WriteLine("=== EDIT COMPLIANCE SUCCESS ===");
                TempData["Success"] = "Compliance record updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ComplianceExists(compliance.Id))
                {
                    TempData["Error"] = "Compliance record not found.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating compliance: {ex.Message}");

                // Reload employees for dropdown
                var employees = _context.Employees
                    .OrderBy(e => e.LastName)
                    .ThenBy(e => e.FirstName)
                    .Select(e => new {
                        e.Id,
                        FullName = $"{e.FirstName} {e.LastName} ({e.Department})"
                    });
                ViewData["EmployeeId"] = new SelectList(employees, "Id", "FullName", compliance.EmployeeId);

                TempData["Error"] = "Error updating compliance record. Please try again.";
                return View(compliance);
            }
        }

        // GET: Compliances/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                TempData["Error"] = "Compliance ID not provided.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var compliance = await _context.Compliances
                    .Include(c => c.Employee)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (compliance == null)
                {
                    TempData["Error"] = "Compliance record not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(compliance);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading compliance for deletion: {ex.Message}");
                TempData["Error"] = "Error loading compliance for deletion.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Compliances/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                Console.WriteLine("=== DELETE COMPLIANCE START ===");
                var compliance = await _context.Compliances.FindAsync(id);
                if (compliance == null)
                {
                    TempData["Error"] = "Compliance record not found.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Compliances.Remove(compliance);
                await _context.SaveChangesAsync();

                Console.WriteLine("=== DELETE COMPLIANCE SUCCESS ===");
                TempData["Success"] = "Compliance record deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting compliance: {ex.Message}");
                TempData["Error"] = "Error deleting compliance record. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        private bool ComplianceExists(int id)
        {
            return _context.Compliances.Any(e => e.Id == id);
        }
    }
}