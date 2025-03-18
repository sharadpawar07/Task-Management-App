using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagementApp.Models;
using System.Linq;
using System.Threading.Tasks; 
using Microsoft.AspNetCore.Http;
using System;

namespace TaskManagementApp.Controllers
{
    public class TaskViewController : Controller
    {
        private readonly TaskManagementDBContext _context;

        public TaskViewController(TaskManagementDBContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var tasks = await _context.Tasks
                    .Include(t => t.AssignedUser)
                    .Select(t => new
                    {
                        t.TaskId,
                        t.Title,
                        t.Status,
                        AssignedUser = t.AssignedUser != null ? t.AssignedUser.FullName : "Unassigned"
                    })
                    .ToListAsync();
                return Json(tasks);
            }

            return View();
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

            return View(task);
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users.Select(u => new { u.UserId, u.FullName }).ToListAsync();
            return Json(users);
        }


        [HttpPut ]
        //[ValidateAntiForgeryToken]
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
