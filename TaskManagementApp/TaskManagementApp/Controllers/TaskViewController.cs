using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagementApp.Models;
using System.Linq;
using System.Threading.Tasks; 
using Microsoft.AspNetCore.Http;
using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace TaskManagementApp.Controllers
{
    
    public class TaskViewController : Controller
    {
        private readonly TaskManagementDBContext _context;

        public TaskViewController(TaskManagementDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Models.Task newTask)
        {
            if (newTask == null || string.IsNullOrWhiteSpace(newTask.Title))
            {
                return BadRequest(new { message = "Invalid task data." });
            }

      
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "User not authenticated." });
            }

            
            newTask.CreatedBy = userId; 
            newTask.CreatedDate = DateTime.Now;
            newTask.UpdatedDate = DateTime.Now;

            _context.Tasks.Add(newTask);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Task created successfully!" });
        }

        public async Task<IActionResult> Index(string searchTitle, string statusFilter, DateTime? dueDateFilter, int? assignedUserFilter)
        {
            var tasksQuery = _context.Tasks.Include(t => t.AssignedUser).AsQueryable();

            
            if (!string.IsNullOrEmpty(searchTitle))
            {
                tasksQuery = tasksQuery.Where(t => t.Title.Contains(searchTitle));
            }

            
            if (!string.IsNullOrEmpty(statusFilter))
            {
                tasksQuery = tasksQuery.Where(t => t.Status == statusFilter);
            }

           
            if (dueDateFilter.HasValue)
            {
                tasksQuery = tasksQuery.Where(t => t.DueDate.HasValue && t.DueDate.Value.Date == dueDateFilter.Value.Date);
            }

            var userRole ="";
            if (!HttpContext.Session.TryGetValue("UserRole", out byte[] roleBytes) || System.Text.Encoding.UTF8.GetString(roleBytes) != "1")
            {
                userRole = "2";
            }
            else
            {
                userRole = "1";
            }
                
            if ((userRole == "1") && assignedUserFilter.HasValue)
            {
                tasksQuery = tasksQuery.Where(t => t.AssignedUserId == assignedUserFilter);
            }

            var tasks = await tasksQuery.Select(t => new
            {
                t.TaskId,
                t.Title,
                t.Status,
                DueDate = t.DueDate.HasValue ? t.DueDate.Value.ToString("yyyy-MM-dd") : null,
                AssignedUser = t.AssignedUser != null ? t.AssignedUser.FullName : "Unassigned"
            }).ToListAsync();

            if (HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(tasks);
            }

            return View(tasks);
        }



        public async Task<IActionResult> Details(int id)
        {
            var task = await _context.Tasks.Include(t => t.AssignedUser)
                                           .FirstOrDefaultAsync(t => t.TaskId == id);

            if (task == null)
            {
                return NotFound();
            }

            return View(task);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var task = await _context.Tasks.Include(t => t.AssignedUser)
                                           .FirstOrDefaultAsync(t => t.TaskId == id);

            if (task == null)
            {
                return NotFound();
            }

            return View(task);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task != null)
            {
                _context.Tasks.Remove(task);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var task = await _context.Tasks.Include(t => t.AssignedUser)
                                           .FirstOrDefaultAsync(t => t.TaskId == id);

            if (task == null)
            {
                return NotFound();
            }

            if (HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new
                {
                    task.TaskId,
                    task.Title,
                    task.Description,
                    DueDate = task.DueDate.HasValue ? task.DueDate.Value.ToString("yyyy-MM-dd") : null,
                    task.Status,
                    task.AssignedUserId
                });
            }
            
            var userRole = "";
            if (!HttpContext.Session.TryGetValue("UserRole", out byte[] roleBytes) || System.Text.Encoding.UTF8.GetString(roleBytes) != "1")
            {
                userRole = "2";
            }
            else
            {
                userRole = "1";
            }
            ViewData["UserRole"] = userRole;

            return View(task);
        }

        
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users
                .Select(u => new { userId = u.UserId, fullName = u.FullName }) // Convert to camelCase
                .ToListAsync();

            if (users == null || !users.Any())
            {
                return NotFound(new { message = "No users found." });
            }

            return new JsonResult(users, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
            });
        }



        [HttpPut ]
        public async Task<IActionResult> Edit(int id, [FromBody] Models.Task updatedTask)
        {
            if (id != updatedTask.TaskId)
            {
                return BadRequest();
            }

          

            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            task.Title = updatedTask.Title;
            task.Description = updatedTask.Description;
            task.DueDate = updatedTask.DueDate;
            task.Status = updatedTask.Status;
            task.AssignedUserId = updatedTask.AssignedUserId;
            task.UpdatedDate = DateTime.Now;

            try
            {
                _context.Tasks.Update(task);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                return View(updatedTask);
            }
        }
    }
}
