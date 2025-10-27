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
    public class EmployeesController : Controller
    {
        private readonly EmsContext _context;

        public EmployeesController(EmsContext context)
        {
            _context = context;
        }

        // GET: Employees
        public async Task<IActionResult> Index()
        {
            try
            {
                var employees = await _context.Employees
                    .OrderBy(e => e.LastName)
                    .ThenBy(e => e.FirstName)
                    .ToListAsync();

                Console.WriteLine($"Loaded {employees.Count} employees");
                return View(employees);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading employees: {ex.Message}");
                TempData["Error"] = "Error loading employees. Please try again.";
                return View(new List<Employee>());
            }
        }

        // GET: Employees/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                TempData["Error"] = "Employee ID not provided.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var employee = await _context.Employees
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (employee == null)
                {
                    TempData["Error"] = "Employee not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(employee);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading employee details: {ex.Message}");
                TempData["Error"] = "Error loading employee details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Employees/Create
        public IActionResult Create()
        {
            // Initialize with default values for better UX
            var employee = new Employee
            {
                HireDate = DateTime.Today // Set default hire date to today
            };
            return View(employee);
        }

        // POST: Employees/Create
        // POST: Employees/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Employee employee)
        {
            Console.WriteLine("=== CREATE EMPLOYEE START ===");
            Console.WriteLine($"ModelState IsValid: {ModelState.IsValid}");

            // Remove validation for navigation properties
            ModelState.Remove("Attendances");
            ModelState.Remove("Leaves");
            ModelState.Remove("Performances");
            ModelState.Remove("Payrolls");
            ModelState.Remove("Trainings");
            ModelState.Remove("Compliances");
            ModelState.Remove("UserAccount");

            // Log all model state errors after removing navigation properties
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
                Console.WriteLine("=== END VALIDATION ERRORS ===");

                TempData["Error"] = "Please fix the validation errors below.";
                return View(employee);
            }

            try
            {
                Console.WriteLine("Attempting to save employee to database...");

                // Initialize navigation properties as empty collections
                employee.Attendances = new List<Attendance>();
                employee.Leaves = new List<Leave>();
                employee.Performances = new List<Performance>();
                employee.Payrolls = new List<Payroll>();
                employee.Trainings = new List<Training>();
                employee.Compliances = new List<Compliance>();

                // Add the employee to context
                _context.Employees.Add(employee);

                // Save changes to database
                int recordsAffected = await _context.SaveChangesAsync();

                Console.WriteLine($"Save successful! Records affected: {recordsAffected}");
                Console.WriteLine($"New Employee ID: {employee.Id}");
                Console.WriteLine("=== CREATE EMPLOYEE SUCCESS ===");

                TempData["Success"] = $"Employee {employee.FirstName} {employee.LastName} created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine($"Database Update Error: {dbEx.Message}");
                Console.WriteLine($"Inner Exception: {dbEx.InnerException?.Message}");

                TempData["Error"] = "Unable to save changes. Please try again. Database error occurred.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Error: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");

                TempData["Error"] = "An unexpected error occurred. Please try again.";
            }

            Console.WriteLine("=== CREATE EMPLOYEE FAILED ===");
            return View(employee);
        }
        // GET: Employees/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                TempData["Error"] = "Employee ID not provided.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var employee = await _context.Employees.FindAsync(id);
                if (employee == null)
                {
                    TempData["Error"] = "Employee not found.";
                    return RedirectToAction(nameof(Index));
                }
                return View(employee);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading employee for edit: {ex.Message}");
                TempData["Error"] = "Error loading employee for editing.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Employees/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Employee employee)
        {
            if (id != employee.Id)
            {
                TempData["Error"] = "Employee ID mismatch.";
                return RedirectToAction(nameof(Index));
            }

            Console.WriteLine("=== EDIT EMPLOYEE START ===");
            Console.WriteLine($"ModelState IsValid: {ModelState.IsValid}");

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

                TempData["Error"] = "Please fix the validation errors below.";
                return View(employee);
            }

            try
            {
                Console.WriteLine("Attempting to update employee...");
                _context.Update(employee);
                await _context.SaveChangesAsync();

                Console.WriteLine("=== EDIT EMPLOYEE SUCCESS ===");
                TempData["Success"] = $"Employee {employee.FirstName} {employee.LastName} updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmployeeExists(employee.Id))
                {
                    TempData["Error"] = "Employee not found.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating employee: {ex.Message}");
                TempData["Error"] = "Error updating employee. Please try again.";
                return View(employee);
            }
        }

        // GET: Employees/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                TempData["Error"] = "Employee ID not provided.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var employee = await _context.Employees
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (employee == null)
                {
                    TempData["Error"] = "Employee not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(employee);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading employee for deletion: {ex.Message}");
                TempData["Error"] = "Error loading employee for deletion.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                Console.WriteLine("=== DELETE EMPLOYEE START ===");
                var employee = await _context.Employees.FindAsync(id);
                if (employee == null)
                {
                    TempData["Error"] = "Employee not found.";
                    return RedirectToAction(nameof(Index));
                }

                string employeeName = $"{employee.FirstName} {employee.LastName}";
                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();

                Console.WriteLine("=== DELETE EMPLOYEE SUCCESS ===");
                TempData["Success"] = $"Employee {employeeName} deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting employee: {ex.Message}");
                TempData["Error"] = "Error deleting employee. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.Id == id);
        }
    }
}