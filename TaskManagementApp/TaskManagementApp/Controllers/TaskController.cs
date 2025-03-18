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

        [HttpGet("task")]
        public async Task<IActionResult> GetTasks()
        {
            
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); 
            var userRole = Convert.ToInt32(User.FindFirstValue(ClaimTypes.Role));

            if (userId == null)
            {
                return Unauthorized("User is not authenticated.");
            }

            

            if (userRole == 1)
            {
                var tasks = await _context.Tasks
                    .Include(t => t.AssignedUser)
                    .Select(t => new
                    {
                        t.TaskId,
                        t.Title,
                        t.Status,
                        AssignedUser = t.AssignedUser != null ? t.AssignedUser.FullName : "Unassigned",
                        RoleId = userRole
                    })
                    .ToListAsync();

                return Ok(tasks);

            }
            else
            {
                var tasks = await _context.Tasks
                .Where(t => t.AssignedUserId == Convert.ToInt32(userId))
                .Include(t => t.AssignedUser)
                .Select(t => new
                {
                    t.TaskId,
                    t.Title,
                    t.Status,
                    AssignedUser = t.AssignedUser != null ? t.AssignedUser.FullName : "Unassigned",
                    RoleId = userRole
                })
                .ToListAsync();
                return Ok(tasks);
            }
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

        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] TaskEntity newTask)
        {
            if (newTask == null)
            {
                return BadRequest("Invalid task data.");
            }

            newTask.CreatedDate = DateTime.Now;
            newTask.UpdatedDate = DateTime.Now;

            _context.Tasks.Add(newTask);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetTaskById), new { id = newTask.TaskId }, newTask);
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
