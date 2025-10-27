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
    public class PayrollsController : Controller
    {
        private readonly EmsContext _context;

        public PayrollsController(EmsContext context)
        {
            _context = context;
        }

        // GET: Payrolls
        public async Task<IActionResult> Index()
        {
            try
            {
                var payrolls = await _context.Payrolls
                    .Include(p => p.Employee)
                    .OrderByDescending(p => p.Year)
                    .ThenByDescending(p => p.Month)
                    .ThenBy(p => p.Employee.LastName)
                    .ToListAsync();

                Console.WriteLine($"Loaded {payrolls.Count} payroll records");
                return View(payrolls);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading payrolls: {ex.Message}");
                TempData["Error"] = "Error loading payroll records. Please try again.";
                return View(new List<Payroll>());
            }
        }

        // GET: Payrolls/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                TempData["Error"] = "Payroll ID not provided.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var payroll = await _context.Payrolls
                    .Include(p => p.Employee)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (payroll == null)
                {
                    TempData["Error"] = "Payroll record not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(payroll);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading payroll details: {ex.Message}");
                TempData["Error"] = "Error loading payroll details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Payrolls/Create
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
                        FullName = $"{e.FirstName} {e.LastName} (Base: ${e.Salary:N2})"
                    });

                ViewData["EmployeeId"] = new SelectList(employees, "Id", "FullName");

                // Set default values
                var currentDate = DateTime.Today;
                var payroll = new Payroll
                {
                    Month = currentDate.Month,
                    Year = currentDate.Year,
                    Salary = 0,
                    Bonus = 0,
                    Deductions = 0
                };

                return View(payroll);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading create form: {ex.Message}");
                TempData["Error"] = "Error loading create form.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Payrolls/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Payroll payroll)
        {
            Console.WriteLine("=== CREATE PAYROLL START ===");
            Console.WriteLine($"ModelState IsValid: {ModelState.IsValid}");

            // Validate month and year
            if (payroll.Month < 1 || payroll.Month > 12)
            {
                ModelState.AddModelError("Month", "Month must be between 1 and 12.");
            }

            if (payroll.Year < 2000 || payroll.Year > 2100)
            {
                ModelState.AddModelError("Year", "Year must be between 2000 and 2100.");
            }

            // Validate financial values
            if (payroll.Salary < 0)
            {
                ModelState.AddModelError("Salary", "Salary cannot be negative.");
            }

            if (payroll.Bonus < 0)
            {
                ModelState.AddModelError("Bonus", "Bonus cannot be negative.");
            }

            if (payroll.Deductions < 0)
            {
                ModelState.AddModelError("Deductions", "Deductions cannot be negative.");
            }

            // Check for duplicate payroll record
            if (await _context.Payrolls.AnyAsync(p =>
                p.EmployeeId == payroll.EmployeeId &&
                p.Month == payroll.Month &&
                p.Year == payroll.Year))
            {
                ModelState.AddModelError("", "A payroll record already exists for this employee for the selected month and year.");
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
                        FullName = $"{e.FirstName} {e.LastName} (Base: ${e.Salary:N2})"
                    });
                ViewData["EmployeeId"] = new SelectList(employees, "Id", "FullName", payroll.EmployeeId);

                TempData["Error"] = "Please fix the validation errors below.";
                return View(payroll);
            }

            try
            {
                // Calculate net pay
                payroll.NetPay = payroll.Salary + payroll.Bonus - payroll.Deductions;

                Console.WriteLine($"Calculated Net Pay: {payroll.NetPay}");

                Console.WriteLine("Attempting to save payroll record...");
                _context.Add(payroll);
                await _context.SaveChangesAsync();

                Console.WriteLine("=== CREATE PAYROLL SUCCESS ===");
                TempData["Success"] = "Payroll record created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating payroll: {ex.Message}");

                // Reload employees for dropdown
                var employees = _context.Employees
                    .OrderBy(e => e.LastName)
                    .ThenBy(e => e.FirstName)
                    .Select(e => new {
                        e.Id,
                        FullName = $"{e.FirstName} {e.LastName} (Base: ${e.Salary:N2})"
                    });
                ViewData["EmployeeId"] = new SelectList(employees, "Id", "FullName", payroll.EmployeeId);

                TempData["Error"] = "Error creating payroll record. Please try again.";
                return View(payroll);
            }
        }

        // GET: Payrolls/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                TempData["Error"] = "Payroll ID not provided.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var payroll = await _context.Payrolls.FindAsync(id);
                if (payroll == null)
                {
                    TempData["Error"] = "Payroll record not found.";
                    return RedirectToAction(nameof(Index));
                }

                // Get employees for dropdown with full name
                var employees = _context.Employees
                    .OrderBy(e => e.LastName)
                    .ThenBy(e => e.FirstName)
                    .Select(e => new {
                        e.Id,
                        FullName = $"{e.FirstName} {e.LastName} (Base: ${e.Salary:N2})"
                    });

                ViewData["EmployeeId"] = new SelectList(employees, "Id", "FullName", payroll.EmployeeId);
                return View(payroll);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading payroll for edit: {ex.Message}");
                TempData["Error"] = "Error loading payroll for editing.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Payrolls/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Payroll payroll)
        {
            if (id != payroll.Id)
            {
                TempData["Error"] = "Payroll ID mismatch.";
                return RedirectToAction(nameof(Index));
            }

            Console.WriteLine("=== EDIT PAYROLL START ===");

            // Validate month and year
            if (payroll.Month < 1 || payroll.Month > 12)
            {
                ModelState.AddModelError("Month", "Month must be between 1 and 12.");
            }

            if (payroll.Year < 2000 || payroll.Year > 2100)
            {
                ModelState.AddModelError("Year", "Year must be between 2000 and 2100.");
            }

            // Validate financial values
            if (payroll.Salary < 0)
            {
                ModelState.AddModelError("Salary", "Salary cannot be negative.");
            }

            if (payroll.Bonus < 0)
            {
                ModelState.AddModelError("Bonus", "Bonus cannot be negative.");
            }

            if (payroll.Deductions < 0)
            {
                ModelState.AddModelError("Deductions", "Deductions cannot be negative.");
            }

            // Check for duplicate payroll record (excluding current record)
            if (await _context.Payrolls.AnyAsync(p =>
                p.EmployeeId == payroll.EmployeeId &&
                p.Month == payroll.Month &&
                p.Year == payroll.Year &&
                p.Id != payroll.Id))
            {
                ModelState.AddModelError("", "A payroll record already exists for this employee for the selected month and year.");
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
                        FullName = $"{e.FirstName} {e.LastName} (Base: ${e.Salary:N2})"
                    });
                ViewData["EmployeeId"] = new SelectList(employees, "Id", "FullName", payroll.EmployeeId);

                TempData["Error"] = "Please fix the validation errors below.";
                return View(payroll);
            }

            try
            {
                // Calculate net pay
                payroll.NetPay = payroll.Salary + payroll.Bonus - payroll.Deductions;
                Console.WriteLine($"Calculated Net Pay: {payroll.NetPay}");

                Console.WriteLine("Attempting to update payroll...");
                _context.Update(payroll);
                await _context.SaveChangesAsync();

                Console.WriteLine("=== EDIT PAYROLL SUCCESS ===");
                TempData["Success"] = "Payroll record updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PayrollExists(payroll.Id))
                {
                    TempData["Error"] = "Payroll record not found.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating payroll: {ex.Message}");

                // Reload employees for dropdown
                var employees = _context.Employees
                    .OrderBy(e => e.LastName)
                    .ThenBy(e => e.FirstName)
                    .Select(e => new {
                        e.Id,
                        FullName = $"{e.FirstName} {e.LastName} (Base: ${e.Salary:N2})"
                    });
                ViewData["EmployeeId"] = new SelectList(employees, "Id", "FullName", payroll.EmployeeId);

                TempData["Error"] = "Error updating payroll record. Please try again.";
                return View(payroll);
            }
        }

        // GET: Payrolls/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                TempData["Error"] = "Payroll ID not provided.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var payroll = await _context.Payrolls
                    .Include(p => p.Employee)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (payroll == null)
                {
                    TempData["Error"] = "Payroll record not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(payroll);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading payroll for deletion: {ex.Message}");
                TempData["Error"] = "Error loading payroll for deletion.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Payrolls/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                Console.WriteLine("=== DELETE PAYROLL START ===");
                var payroll = await _context.Payrolls.FindAsync(id);
                if (payroll == null)
                {
                    TempData["Error"] = "Payroll record not found.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Payrolls.Remove(payroll);
                await _context.SaveChangesAsync();

                Console.WriteLine("=== DELETE PAYROLL SUCCESS ===");
                TempData["Success"] = "Payroll record deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting payroll: {ex.Message}");
                TempData["Error"] = "Error deleting payroll record. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        private bool PayrollExists(int id)
        {
            return _context.Payrolls.Any(e => e.Id == id);
        }
    }
}