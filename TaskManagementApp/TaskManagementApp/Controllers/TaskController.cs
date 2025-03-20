using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagementApp.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TaskEntity = TaskManagementApp.Models.Task;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace TaskManagementApp.ApiControllers
{
    [Route("api")]
    [ApiController]
    [Authorize]
    public class TaskController : ControllerBase
    {
        private readonly TaskManagementDBContext _context;

        public TaskController(TaskManagementDBContext context)
        {
            _context = context;
        }



        [HttpPost("task")]
        public async Task<IActionResult> CreateTask([FromBody] TaskEntity newTask)
        {
            if (newTask == null || string.IsNullOrWhiteSpace(newTask.Title))
            {
                return BadRequest(new { message = "Invalid task data." });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized(new { message = "User is not authenticated." });
            }

            newTask.CreatedDate = DateTime.Now;
            newTask.UpdatedDate = DateTime.Now;
            newTask.CreatedBy = Convert.ToInt32(userId);
            newTask.AssignedUserId = newTask.AssignedUserId != 0 ? newTask.AssignedUserId : newTask.CreatedBy;

            _context.Tasks.Add(newTask);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTaskById), new { id = newTask.TaskId }, new { message = "Task created successfully!", task = newTask });
        }

        [HttpGet("task")]
        public async Task<IActionResult> GetTasks(string? searchTitle, string? statusFilter, DateTime? dueDateFilter, int? assignedUserFilter)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = Convert.ToInt32(User.FindFirstValue(ClaimTypes.Role));

            if (userId == null)
            {
                return Unauthorized("User is not authenticated.");
            }

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

            if (userRole == 1 && assignedUserFilter.HasValue)
            {
                tasksQuery = tasksQuery.Where(t => t.AssignedUserId == assignedUserFilter);
            }

            var tasks = await tasksQuery.Select(t => new
            {
                t.TaskId,
                t.Title,
                t.Status,
                DueDate = t.DueDate.HasValue ? t.DueDate.Value.ToString("yyyy-MM-dd") : null,
                AssignedUser = t.AssignedUser != null ? t.AssignedUser.FullName : "Unassigned",
                RoleId = userRole
            }).ToListAsync();

            return Ok(tasks);
        }



        [HttpGet("{id}")]
        public async Task<IActionResult> GetTaskById(int id)
        {
            var task = await _context.Tasks.Include(t => t.AssignedUser).FirstOrDefaultAsync(t => t.TaskId == id);
            if (task == null)
            {
                return NotFound();
            }
            return Ok(task);
        }

        
        [HttpPut("task/{id}")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] TaskEntity updatedTask)
        {
            if (id != updatedTask.TaskId)
            {
                return BadRequest("Task ID mismatch.");
            }

            var existingTask = await _context.Tasks.FindAsync(id);
            if (existingTask == null)
            {
                return NotFound();
            }

            existingTask.Title = updatedTask.Title;
            existingTask.Description = updatedTask.Description;
            existingTask.DueDate = updatedTask.DueDate;
            existingTask.Status = updatedTask.Status;
            existingTask.AssignedUserId = updatedTask.AssignedUserId;
            existingTask.UpdatedDate = DateTime.Now;

            _context.Tasks.Update(existingTask);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("task/{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            return NoContent();
        }


    }
}
