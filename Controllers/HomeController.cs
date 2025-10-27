using System.Diagnostics;
using EM.Models;
using Microsoft.AspNetCore.Mvc;
using EM.Data; // Add this
using Microsoft.EntityFrameworkCore; // Add this

namespace EM.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly EmsContext _context; // Add this

        // Update constructor to inject DbContext
        public HomeController(ILogger<HomeController> logger, EmsContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // You can add dashboard statistics here
            ViewBag.TotalEmployees = await _context.Employees.CountAsync();
            ViewBag.TotalAttendance = await _context.Attendances.CountAsync();
            ViewBag.TotalLeaves = await _context.Leaves.CountAsync();

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}