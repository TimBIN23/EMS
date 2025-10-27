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
    public class AttendancesController : Controller
    {
        private readonly EmsContext _context;

        public AttendancesController(EmsContext context)
        {
            _context = context;
        }

        // GET: Attendances
        public async Task<IActionResult> Index()
        {
            try
            {
                var attendances = await _context.Attendances
                    .Include(a => a.Employee)
                    .OrderByDescending(a => a.Date)
                    .ThenBy(a => a.Employee.LastName)
                    .ToListAsync();

                Console.WriteLine($"Loaded {attendances.Count} attendance records");
                return View(attendances);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading attendances: {ex.Message}");
                TempData["Error"] = "Error loading attendance records. Please try again.";
                return View(new List<Attendance>());
            }
        }

        // GET: Attendances/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                TempData["Error"] = "Attendance ID not provided.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var attendance = await _context.Attendances
                    .Include(a => a.Employee)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (attendance == null)
                {
                    TempData["Error"] = "Attendance record not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(attendance);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading attendance details: {ex.Message}");
                TempData["Error"] = "Error loading attendance details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Attendances/Create
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
                var attendance = new Attendance
                {
                    Date = DateTime.Today,
                    CheckInTime = new TimeSpan(9, 0, 0), // Default 9:00 AM
                    Status = "Present"
                };

                return View(attendance);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading create form: {ex.Message}");
                TempData["Error"] = "Error loading create form.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Attendances/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public async Task<IActionResult> Create([Bind("EmployeeId,Date,CheckInTime,CheckOutTime,Status")] Attendance attendance)
        {
            Console.WriteLine("=== CREATE ATTENDANCE START ===");
            Console.WriteLine($"ModelState IsValid: {ModelState.IsValid}");

            // Remove navigation property from validation
            ModelState.Remove("Employee");

            // Validate check-out time
            if (attendance.CheckOutTime.HasValue && attendance.CheckOutTime <= attendance.CheckInTime)
            {
                ModelState.AddModelError("CheckOutTime", "Check-out time must be after check-in time.");
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
                ViewData["EmployeeId"] = new SelectList(employees, "Id", "FullName", attendance.EmployeeId);

                TempData["Error"] = "Please fix the validation errors below.";
                return View(attendance);
            }

            try
            {
                Console.WriteLine("Attempting to save attendance record...");
                _context.Add(attendance);
                await _context.SaveChangesAsync();

                Console.WriteLine("=== CREATE ATTENDANCE SUCCESS ===");
                TempData["Success"] = "Attendance record created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating attendance: {ex.Message}");

                // Reload employees for dropdown
                var employees = _context.Employees
                    .OrderBy(e => e.LastName)
                    .ThenBy(e => e.FirstName)
                    .Select(e => new {
                        e.Id,
                        FullName = $"{e.FirstName} {e.LastName} ({e.Department})"
                    });
                ViewData["EmployeeId"] = new SelectList(employees, "Id", "FullName", attendance.EmployeeId);

                TempData["Error"] = "Error creating attendance record. Please try again.";
                return View(attendance);
            }
        }

        // GET: Attendances/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                TempData["Error"] = "Attendance ID not provided.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var attendance = await _context.Attendances.FindAsync(id);
                if (attendance == null)
                {
                    TempData["Error"] = "Attendance record not found.";
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

                ViewData["EmployeeId"] = new SelectList(employees, "Id", "FullName", attendance.EmployeeId);
                return View(attendance);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading attendance for edit: {ex.Message}");
                TempData["Error"] = "Error loading attendance for editing.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Attendances/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Attendance attendance)
        {
            if (id != attendance.Id)
            {
                TempData["Error"] = "Attendance ID mismatch.";
                return RedirectToAction(nameof(Index));
            }

            Console.WriteLine("=== EDIT ATTENDANCE START ===");

            // Validate check-out time
            if (attendance.CheckOutTime.HasValue && attendance.CheckOutTime <= attendance.CheckInTime)
            {
                ModelState.AddModelError("CheckOutTime", "Check-out time must be after check-in time.");
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
                ViewData["EmployeeId"] = new SelectList(employees, "Id", "FullName", attendance.EmployeeId);

                TempData["Error"] = "Please fix the validation errors below.";
                return View(attendance);
            }

            try
            {
                Console.WriteLine("Attempting to update attendance...");
                _context.Update(attendance);
                await _context.SaveChangesAsync();

                Console.WriteLine("=== EDIT ATTENDANCE SUCCESS ===");
                TempData["Success"] = "Attendance record updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AttendanceExists(attendance.Id))
                {
                    TempData["Error"] = "Attendance record not found.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating attendance: {ex.Message}");

                // Reload employees for dropdown
                var employees = _context.Employees
                    .OrderBy(e => e.LastName)
                    .ThenBy(e => e.FirstName)
                    .Select(e => new {
                        e.Id,
                        FullName = $"{e.FirstName} {e.LastName} ({e.Department})"
                    });
                ViewData["EmployeeId"] = new SelectList(employees, "Id", "FullName", attendance.EmployeeId);

                TempData["Error"] = "Error updating attendance record. Please try again.";
                return View(attendance);
            }
        }

        // GET: Attendances/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                TempData["Error"] = "Attendance ID not provided.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var attendance = await _context.Attendances
                    .Include(a => a.Employee)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (attendance == null)
                {
                    TempData["Error"] = "Attendance record not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(attendance);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading attendance for deletion: {ex.Message}");
                TempData["Error"] = "Error loading attendance for deletion.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Attendances/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                Console.WriteLine("=== DELETE ATTENDANCE START ===");
                var attendance = await _context.Attendances.FindAsync(id);
                if (attendance == null)
                {
                    TempData["Error"] = "Attendance record not found.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Attendances.Remove(attendance);
                await _context.SaveChangesAsync();

                Console.WriteLine("=== DELETE ATTENDANCE SUCCESS ===");
                TempData["Success"] = "Attendance record deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting attendance: {ex.Message}");
                TempData["Error"] = "Error deleting attendance record. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        private bool AttendanceExists(int id)
        {
            return _context.Attendances.Any(e => e.Id == id);
        }
    }
}