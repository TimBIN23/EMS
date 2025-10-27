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
    public class UsersController : Controller
    {
        private readonly EmsContext _context;

        public UsersController(EmsContext context)
        {
            _context = context;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            try
            {
                var users = await _context.Users
                    .Include(u => u.Employee)
                    .OrderBy(u => u.Employee.LastName)
                    .ThenBy(u => u.Employee.FirstName)
                    .ToListAsync();

                Console.WriteLine($"Loaded {users.Count} user accounts");
                return View(users);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading users: {ex.Message}");
                TempData["Error"] = "Error loading user accounts. Please try again.";
                return View(new List<User>());
            }
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                TempData["Error"] = "User ID not provided.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var user = await _context.Users
                    .Include(u => u.Employee)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (user == null)
                {
                    TempData["Error"] = "User account not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(user);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading user details: {ex.Message}");
                TempData["Error"] = "Error loading user details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            try
            {
                // Get employees without existing user accounts for dropdown
                var employeesWithAccounts = _context.Users.Select(u => u.EmployeeId).ToList();
                var availableEmployees = _context.Employees
                    .Where(e => !employeesWithAccounts.Contains(e.Id))
                    .OrderBy(e => e.LastName)
                    .ThenBy(e => e.FirstName)
                    .Select(e => new {
                        e.Id,
                        FullName = $"{e.FirstName} {e.LastName} ({e.Department})"
                    });

                ViewData["EmployeeId"] = new SelectList(availableEmployees, "Id", "FullName");

                // Set default values
                var user = new User
                {
                    Role = "User" // Default role
                };

                return View(user);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading create form: {ex.Message}");
                TempData["Error"] = "Error loading create form.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user, string Password)
        {
            Console.WriteLine("=== CREATE USER START ===");

            // Remove navigation property from validation
            ModelState.Remove("Employee");
            ModelState.Remove("PasswordHash"); // We'll set this manually

            Console.WriteLine($"ModelState IsValid after removal: {ModelState.IsValid}");

            // Validate password
            if (string.IsNullOrEmpty(Password))
            {
                ModelState.AddModelError("Password", "Password is required.");
            }
            else if (Password.Length < 6)
            {
                ModelState.AddModelError("Password", "Password must be at least 6 characters long.");
            }

            // Check if username already exists
            if (await _context.Users.AnyAsync(u => u.Username == user.Username))
            {
                ModelState.AddModelError("Username", "Username already exists. Please choose a different username.");
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
                var employeesWithAccounts = _context.Users.Select(u => u.EmployeeId).ToList();
                var availableEmployees = _context.Employees
                    .Where(e => !employeesWithAccounts.Contains(e.Id))
                    .OrderBy(e => e.LastName)
                    .ThenBy(e => e.FirstName)
                    .Select(e => new {
                        e.Id,
                        FullName = $"{e.FirstName} {e.LastName} ({e.Department})"
                    });
                ViewData["EmployeeId"] = new SelectList(availableEmployees, "Id", "FullName", user.EmployeeId);

                TempData["Error"] = "Please fix the validation errors below.";
                return View(user);
            }

            try
            {
                // Hash the password
                user.PasswordHash = HashPassword(Password);

                Console.WriteLine("Attempting to save user account...");
                _context.Add(user);
                await _context.SaveChangesAsync();

                Console.WriteLine("=== CREATE USER SUCCESS ===");
                TempData["Success"] = "User account created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating user: {ex.Message}");

                // Reload employees for dropdown
                var employeesWithAccounts = _context.Users.Select(u => u.EmployeeId).ToList();
                var availableEmployees = _context.Employees
                    .Where(e => !employeesWithAccounts.Contains(e.Id))
                    .OrderBy(e => e.LastName)
                    .ThenBy(e => e.FirstName)
                    .Select(e => new {
                        e.Id,
                        FullName = $"{e.FirstName} {e.LastName} ({e.Department})"
                    });
                ViewData["EmployeeId"] = new SelectList(availableEmployees, "Id", "FullName", user.EmployeeId);

                TempData["Error"] = "Error creating user account. Please try again.";
                return View(user);
            }
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                TempData["Error"] = "User ID not provided.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    TempData["Error"] = "User account not found.";
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

                ViewData["EmployeeId"] = new SelectList(employees, "Id", "FullName", user.EmployeeId);
                return View(user);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading user for edit: {ex.Message}");
                TempData["Error"] = "Error loading user for editing.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, IFormCollection form)
        {
            Console.WriteLine("=== EDIT USER POST START ===");

            try
            {
                // Get the form values
                string username = form["Username"];
                int employeeId = int.Parse(form["EmployeeId"]);
                string role = form["Role"];
                string? newPassword = form["NewPassword"];

                Console.WriteLine($"Editing user {id}: {username}, Employee: {employeeId}, Role: {role}");
                Console.WriteLine($"NewPassword provided: {!string.IsNullOrEmpty(newPassword)}");

                // Check if username already exists (excluding current user)
                bool usernameExists = await _context.Users.AnyAsync(u => u.Username == username && u.Id != id);
                Console.WriteLine($"Username exists check: {usernameExists}");

                if (usernameExists)
                {
                    TempData["Error"] = "Username already exists. Please choose a different username.";
                    return RedirectToAction(nameof(Edit), new { id });
                }

                // Validate new password if provided
                if (!string.IsNullOrEmpty(newPassword) && newPassword.Length < 6)
                {
                    TempData["Error"] = "Password must be at least 6 characters long.";
                    return RedirectToAction(nameof(Edit), new { id });
                }

                var existingUser = await _context.Users.FindAsync(id);
                if (existingUser == null)
                {
                    Console.WriteLine("User not found in database");
                    TempData["Error"] = "User account not found.";
                    return RedirectToAction(nameof(Index));
                }

                Console.WriteLine($"Existing user found: {existingUser.Username}");

                // Update user properties
                existingUser.Username = username;
                existingUser.EmployeeId = employeeId;
                existingUser.Role = role;

                // Update password if new one provided
                if (!string.IsNullOrEmpty(newPassword))
                {
                    Console.WriteLine("Updating password...");
                    existingUser.PasswordHash = HashPassword(newPassword);
                }

                Console.WriteLine("Saving changes to database...");
                _context.Update(existingUser);
                int changes = await _context.SaveChangesAsync();
                Console.WriteLine($"Changes saved successfully. Rows affected: {changes}");

                TempData["Success"] = "User account updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating user: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");

                TempData["Error"] = "Error updating user account. Please try again.";
                return RedirectToAction(nameof(Edit), new { id });
            }
        }

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                TempData["Error"] = "User ID not provided.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var user = await _context.Users
                    .Include(u => u.Employee)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (user == null)
                {
                    TempData["Error"] = "User account not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(user);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading user for deletion: {ex.Message}");
                TempData["Error"] = "Error loading user for deletion.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                Console.WriteLine("=== DELETE USER START ===");
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    TempData["Error"] = "User account not found.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                Console.WriteLine("=== DELETE USER SUCCESS ===");
                TempData["Success"] = "User account deleted successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting user: {ex.Message}");
                TempData["Error"] = "Error deleting user account. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }

        // Simple password hashing (in production, use proper hashing like BCrypt)
        private string HashPassword(string password)
        {
            // This is a simple hash for demonstration purposes
            // In a real application, use: BCrypt.Net.BCrypt.HashPassword(password)
            using var sha = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}