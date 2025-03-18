using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using TaskManagementApp.Models;

namespace TaskManagementApp.Controllers
{
    public class DashboardController : Controller
    {
        private readonly TaskManagementDBContext _context;

        public DashboardController(TaskManagementDBContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            
            if (!HttpContext.Session.TryGetValue("UserRole", out byte[] roleBytes) || System.Text.Encoding.UTF8.GetString(roleBytes) != "1")
            {
                return RedirectToAction("Login", "AuthView");
            }

            
            var totalTasks = _context.Tasks.Count();
            var pendingTasks = _context.Tasks.Count(t => t.Status == "Pending");
            var inProgressTasks = _context.Tasks.Count(t => t.Status == "In Progress");
            var completedTasks = _context.Tasks.Count(t => t.Status == "Completed");

            var tasksByUser = _context.Users
                .Select(u => new
                {
                    UserName = u.FullName,
                    TaskCount = _context.Tasks.Count(t => t.AssignedUserId == u.UserId)
                })
                .ToList();

            ViewData["TotalTasks"] = totalTasks;
            ViewData["PendingTasks"] = pendingTasks;
            ViewData["InProgressTasks"] = inProgressTasks;
            ViewData["CompletedTasks"] = completedTasks;
            ViewData["TasksByUser"] = tasksByUser;

            return View();
        }
    }
}
