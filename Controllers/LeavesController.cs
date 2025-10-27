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
    public class LeavesController : Controller
    {
        private readonly EmsContext _context;

        public LeavesController(EmsContext context)
        {
            _context = context;
        }

        // GET: Leaves
        public async Task<IActionResult> Index()
        {
            try
            {
                var leaves = await _context.Leaves
                    .Include(l => l.Employee)
                    .OrderByDescending(l => l.StartDate)
                    .ThenBy(l => l.Employee.LastName)
                    .ToListAsync();

                Console.WriteLine($"Loaded {leaves.Count} leave records");
                return View(leaves);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading leaves: {ex.Message}");
                TempData["Error"] = "Error loading leave records. Please try again.";
                return View(new List<Leave>());
            }
        }

        // GET: Leaves/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                TempData["Error"] = "Leave ID not provided.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var leave = await _context.Leaves
                    .Include(l => l.Employee)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (leave == null)
                {
                    TempData["Error"] = "Leave record not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(leave);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading leave details: {ex.Message}");
                TempData["Error"] = "Error loading leave details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Leaves/Create
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

                // Set default dates
                var model = new Leave
                {
                    StartDate = DateTime.Today,
                    EndDate = DateTime.Today.AddDays(1),
                    Status = "Pending" // Default status
                };

                return View(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading create form: {ex.Message}");
                TempData["Error"] = "Error loading create form.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Leaves/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Leave leave)
        {
            Console.WriteLine("=== CREATE LEAVE START ===");
            Console.WriteLine($"EmployeeId: {leave.EmployeeId}");
            Console.WriteLine($"LeaveType: {leave.LeaveType}");
            Console.WriteLine($"StartDate: {leave.StartDate}");
            Console.WriteLine($"EndDate: {leave.EndDate}");
            Console.WriteLine($"Status: {leave.Status}");

            // Validate dates
            if (leave.EndDate < leave.StartDate)
            {
                ModelState.AddModelError("EndDate", "End date must be after start date.");
            }

            // Validate required fields
            if (leave.EmployeeId == 0)
            {
                ModelState.AddModelError("EmployeeId", "Please select an employee.");
            }

            if (string.IsNullOrEmpty(leave.LeaveType))
            {
                ModelState.AddModelError("LeaveType", "Please select a leave type.");
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
                ViewData["EmployeeId"] = new SelectList(employees, "Id", "FullName", leave.EmployeeId);

                TempData["Error"] = "Please fix the validation errors below.";
                return View(leave);
            }

            try
            {
                // Calculate total days
                var totalDays = (leave.EndDate - leave.StartDate).TotalDays + 1;
                Console.WriteLine($"Leave duration: {totalDays} days");

                // Ensure the entity is being added as new
                leave.Id = 0; // Ensure it's treated as new entity

                Console.WriteLine("Attempting to create leave...");
                _context.Leaves.Add(leave);
                await _context.SaveChangesAsync();

                Console.WriteLine("=== CREATE LEAVE SUCCESS ===");
                Console.WriteLine($"New Leave ID: {leave.Id}");

                TempData["Success"] = "Leave request created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating leave: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");

                // Reload employees for dropdown
                var employees = _context.Employees
                    .OrderBy(e => e.LastName)
                    .ThenBy(e => e.FirstName)
                    .Select(e => new {
                        e.Id,
                        FullName = $"{e.FirstName} {e.LastName} ({e.Department})"
                    });
                ViewData["EmployeeId"] = new SelectList(employees, "Id", "FullName", leave.EmployeeId);

                TempData["Error"] = $"Error creating leave request: {ex.Message}";
                return View(leave);
            }
        }


        // GET: Leaves/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                TempData["Error"] = "Leave ID not provided.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var leave = await _context.Leaves.FindAsync(id);
                if (leave == null)
                {
                    TempData["Error"] = "Leave record not found.";
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

                ViewData["EmployeeId"] = new SelectList(employees, "Id", "FullName", leave.EmployeeId);
                return View(leave);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading leave for edit: {ex.Message}");
                TempData["Error"] = "Error loading leave for editing.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Leaves/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Leave leave)
        {
            if (id != leave.Id)
            {
                TempData["Error"] = "Leave ID mismatch.";
                return RedirectToAction(nameof(Index));
            }

            Console.WriteLine("=== EDIT LEAVE START ===");

            // Validate dates
            if (leave.EndDate < leave.StartDate)
            {
                ModelState.AddModelError("EndDate", "End date must be after start date.");
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
                ViewData["EmployeeId"] = new SelectList(employees, "Id", "FullName", leave.EmployeeId);

                TempData["Error"] = "Please fix the validation errors below.";
                return View(leave);
            }

            try
            {
                // Calculate total days
                var totalDays = (leave.EndDate - leave.StartDate).TotalDays + 1;
                Console.WriteLine($"Leave duration: {totalDays} days");

                Console.WriteLine("Attempting to update leave...");
                _context.Update(leave);
                await _context.SaveChangesAsync();

                Console.WriteLine("=== EDIT LEAVE SUCCESS ===");
                TempData["Success"] = "Leave request updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LeaveExists(leave.Id))
                {
                    TempData["Error"] = "Leave record not found.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating leave: {ex.Message}");

                // Reload employees for dropdown
                var employees = _context.Employees
                    .OrderBy(e => e.LastName)
                    .ThenBy(e => e.FirstName)
                    .Select(e => new {
                        e.Id,
                        FullName = $"{e.FirstName} {e.LastName} ({e.Department})"
                    });
                ViewData["EmployeeId"] = new SelectList(employees, "Id", "FullName", leave.EmployeeId);

                TempData["Error"] = "Error updating leave request. Please try again.";
                return View(leave);
            }
        }
        // GET: Leaves/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                TempData["Error"] = "Leave ID not provided.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var leave = await _context.Leaves
                    .Include(l => l.Employee)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (leave == null)
                {
                    TempData["Error"] = "Leave record not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(leave);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading leave for deletion: {ex.Message}");
                TempData["Error"] = "Error loading leave for deletion.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Leaves/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                Console.WriteLine("=== DELETE LEAVE START ===");
                var leave = await _context.Leaves.FindAsync(id);
                if (leave == null)
                {
                    TempData["Error"] = "Leave record not found.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Leaves.Remove(leave);
                await _context.SaveChangesAsync();

                Console.WriteLine("=== DELETE LEAVE SUCCESS ===");
                TempData["Success"] = "Leave request deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting leave: {ex.Message}");
                TempData["Error"] = "Error deleting leave request. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        private bool LeaveExists(int id)
        {
            return _context.Leaves.Any(e => e.Id == id);
        }
    }
}