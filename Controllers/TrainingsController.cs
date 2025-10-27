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
    public class TrainingsController : Controller
    {
        private readonly EmsContext _context;

        public TrainingsController(EmsContext context)
        {
            _context = context;
        }

        // GET: Trainings
        public async Task<IActionResult> Index()
        {
            try
            {
                var trainings = await _context.Trainings
                    .Include(t => t.Employee)
                    .OrderByDescending(t => t.StartDate)
                    .ThenBy(t => t.Employee.LastName)
                    .ToListAsync();

                Console.WriteLine($"Loaded {trainings.Count} training records");
                return View(trainings);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading trainings: {ex.Message}");
                TempData["Error"] = "Error loading training records. Please try again.";
                return View(new List<Training>());
            }
        }

        // GET: Trainings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                TempData["Error"] = "Training ID not provided.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var training = await _context.Trainings
                    .Include(t => t.Employee)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (training == null)
                {
                    TempData["Error"] = "Training record not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(training);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading training details: {ex.Message}");
                TempData["Error"] = "Error loading training details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Trainings/Create
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
                var training = new Training
                {
                    StartDate = DateTime.Today,
                    EndDate = DateTime.Today.AddDays(1),
                    Status = "Scheduled"
                };

                return View(training);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading create form: {ex.Message}");
                TempData["Error"] = "Error loading create form.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Trainings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Training training)
        {
            Console.WriteLine("=== CREATE TRAINING START ===");
            Console.WriteLine($"ModelState IsValid: {ModelState.IsValid}");

            // Validate dates
            if (training.EndDate < training.StartDate)
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
                ViewData["EmployeeId"] = new SelectList(employees, "Id", "FullName", training.EmployeeId);

                TempData["Error"] = "Please fix the validation errors below.";
                return View(training);
            }

            try
            {
                // Calculate duration
                var duration = (training.EndDate - training.StartDate).TotalDays + 1;
                Console.WriteLine($"Training duration: {duration} days");

                Console.WriteLine("Attempting to save training record...");
                _context.Add(training);
                await _context.SaveChangesAsync();

                Console.WriteLine("=== CREATE TRAINING SUCCESS ===");
                TempData["Success"] = "Training record created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating training: {ex.Message}");

                // Reload employees for dropdown
                var employees = _context.Employees
                    .OrderBy(e => e.LastName)
                    .ThenBy(e => e.FirstName)
                    .Select(e => new {
                        e.Id,
                        FullName = $"{e.FirstName} {e.LastName} ({e.Department})"
                    });
                ViewData["EmployeeId"] = new SelectList(employees, "Id", "FullName", training.EmployeeId);

                TempData["Error"] = "Error creating training record. Please try again.";
                return View(training);
            }
        }

        // GET: Trainings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                TempData["Error"] = "Training ID not provided.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var training = await _context.Trainings.FindAsync(id);
                if (training == null)
                {
                    TempData["Error"] = "Training record not found.";
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

                ViewData["EmployeeId"] = new SelectList(employees, "Id", "FullName", training.EmployeeId);
                return View(training);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading training for edit: {ex.Message}");
                TempData["Error"] = "Error loading training for editing.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Trainings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Training training)
        {
            if (id != training.Id)
            {
                TempData["Error"] = "Training ID mismatch.";
                return RedirectToAction(nameof(Index));
            }

            Console.WriteLine("=== EDIT TRAINING START ===");

            // Validate dates
            if (training.EndDate < training.StartDate)
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
                ViewData["EmployeeId"] = new SelectList(employees, "Id", "FullName", training.EmployeeId);

                TempData["Error"] = "Please fix the validation errors below.";
                return View(training);
            }

            try
            {
                // Calculate duration
                var duration = (training.EndDate - training.StartDate).TotalDays + 1;
                Console.WriteLine($"Training duration: {duration} days");

                Console.WriteLine("Attempting to update training...");
                _context.Update(training);
                await _context.SaveChangesAsync();

                Console.WriteLine("=== EDIT TRAINING SUCCESS ===");
                TempData["Success"] = "Training record updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TrainingExists(training.Id))
                {
                    TempData["Error"] = "Training record not found.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating training: {ex.Message}");

                // Reload employees for dropdown
                var employees = _context.Employees
                    .OrderBy(e => e.LastName)
                    .ThenBy(e => e.FirstName)
                    .Select(e => new {
                        e.Id,
                        FullName = $"{e.FirstName} {e.LastName} ({e.Department})"
                    });
                ViewData["EmployeeId"] = new SelectList(employees, "Id", "FullName", training.EmployeeId);

                TempData["Error"] = "Error updating training record. Please try again.";
                return View(training);
            }
        }

        // GET: Trainings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                TempData["Error"] = "Training ID not provided.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var training = await _context.Trainings
                    .Include(t => t.Employee)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (training == null)
                {
                    TempData["Error"] = "Training record not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(training);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading training for deletion: {ex.Message}");
                TempData["Error"] = "Error loading training for deletion.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Trainings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                Console.WriteLine("=== DELETE TRAINING START ===");
                var training = await _context.Trainings.FindAsync(id);
                if (training == null)
                {
                    TempData["Error"] = "Training record not found.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Trainings.Remove(training);
                await _context.SaveChangesAsync();

                Console.WriteLine("=== DELETE TRAINING SUCCESS ===");
                TempData["Success"] = "Training record deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting training: {ex.Message}");
                TempData["Error"] = "Error deleting training record. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        private bool TrainingExists(int id)
        {
            return _context.Trainings.Any(e => e.Id == id);
        }
    }
}