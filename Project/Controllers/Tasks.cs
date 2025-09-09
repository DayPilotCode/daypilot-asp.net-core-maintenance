using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project.Models;

namespace Project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Tasks : ControllerBase
    {
        private readonly MaintenanceDbContext _context;

        public Tasks(MaintenanceDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetTasks([FromQuery] DateTime? start, [FromQuery] DateTime? end)
        {
            if (_context.Tasks == null)
            {
                return NotFound();
            }

            IQueryable<MaintenanceTask> query = _context.Tasks
                .Include(t => t.MaintenanceType)
                .Include(t => t.TaskItems)
                .Include(t => t.Next);

            if (start.HasValue)
            {
                query = query.Where(t => t.DueDate >= start.Value);
            }

            if (end.HasValue)
            {
                query = query.Where(t => t.DueDate <= end.Value);
            }

            var tasks = await query.ToListAsync();

            // prevent the whole chain from loading in TaskTransformer.TransformTask
            foreach(var task in tasks)
            {
                if(task.Next != null) {
                    task.Next = new MaintenanceTask
                    {
                        Id = task.Next.Id,
                        DueDate = task.Next.DueDate,
                    };
                }
                else {
                    task.Next = null;
                }
            }

            var result = tasks.Select(TaskTransformer.TransformTask).ToList();

            return result;
        }



        // GET: api/Tasks/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MaintenanceTask>> GetMaintenanceTask(int id)
        {
          if (_context.Tasks == null)
          {
              return NotFound();
          }
            var maintenanceTask = await _context.Tasks.FindAsync(id);

            if (maintenanceTask == null)
            {
                return NotFound();
            }

            return maintenanceTask;
        }

        // PUT: api/Tasks/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMaintenanceTask(int id, MaintenanceTask maintenanceTask)
        {
            if (id != maintenanceTask.Id)
            {
                return BadRequest();
            }

            _context.Entry(maintenanceTask).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MaintenanceTaskExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Tasks
        [HttpPost]
        public async Task<ActionResult<MaintenanceTask>> PostMaintenanceTask(MaintenanceTaskDto dto)
        {
            var maintenanceType = await _context.Types.FindAsync(dto.Type);
            if (maintenanceType == null)
            {
                return NotFound();
            }

            var task = new MaintenanceTask 
            {
                DueDate = dto.Start,
                Text = dto.Text,
                MaintenanceType = maintenanceType,  // Link to MaintenanceType
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            // Re-fetch the task, including related entities
            var newTask = await _context.Tasks
                .Include(t => t.MaintenanceType)
                .Include(t => t.TaskItems)
                .FirstOrDefaultAsync(t => t.Id == task.Id);

            return CreatedAtAction("GetMaintenanceTask", new { id = newTask.Id }, TaskTransformer.TransformTask(newTask));
        }



        // DELETE: api/Tasks/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMaintenanceTask(int id)
        {
            var maintenanceTask = await _context.Tasks
                .Include(t => t.Next)
                .Include(t => t.TaskItems)  // Include TaskItems
                .FirstOrDefaultAsync(t => t.Id == id);
            if (maintenanceTask == null)
            {
                return NotFound();
            }

            // Don't allow deletion if the next task was scheduled
            if (maintenanceTask.Next != null)
            {
                return BadRequest("Can't delete a task that has a next task scheduled.");
            }

            // If this task is the "next" task of another task, clear that link
            var previousTask = await _context.Tasks.FirstOrDefaultAsync(t => t.Next == maintenanceTask);
            if (previousTask != null)
            {
                previousTask.Next = null;
                _context.Entry(previousTask).State = EntityState.Modified;
            }

            // Remove the TaskItems
            _context.TaskItems.RemoveRange(maintenanceTask.TaskItems);

            _context.Tasks.Remove(maintenanceTask);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        
        [HttpPost("{id}/update-checklist")]
        public async Task<IActionResult> UpdateChecklist(int id, Dictionary<int, bool> checklist)
        {
            var task = await _context.Tasks
                .Include(t => t.TaskItems)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
            {
                return NotFound();
            }

            // Check each item in the received checklist
            foreach (var item in checklist)
            {
                var taskItem = task.TaskItems.FirstOrDefault(ti => ti.MaintenanceTypeItemId == item.Key);

                if (item.Value)  // If item is checked
                {
                    if (taskItem == null)  // If item doesn't exist, create it
                    {
                        taskItem = new MaintenanceTaskItem
                        {
                            Checked = true,
                            MaintenanceTypeItemId = item.Key
                        };
                        task.TaskItems.Add(taskItem);
                    }
                    else  // If item exists, check it
                    {
                        taskItem.Checked = true;
                    }
                }
                else if (taskItem != null)  // If item is unchecked and it exists, delete it
                {
                    _context.TaskItems.Remove(taskItem);
                }
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }
        
        [HttpPost("{id}/schedule-next")]
        public async Task<ActionResult<MaintenanceTask>> ScheduleNextTask(int id, MaintenanceTaskDto dto)
        {
            var originalTask = await _context.Tasks.Include(t => t.Next).FirstOrDefaultAsync(t => t.Id == id);
            if (originalTask == null)
            {
                return NotFound();
            }

            if (originalTask.Next != null)
            {
                return BadRequest("A next task is already scheduled for this task.");
            }

            var maintenanceType = await _context.Types.FindAsync(dto.Type);
            if (maintenanceType == null)
            {
                return NotFound();
            }

            var nextTask = new MaintenanceTask 
            {
                DueDate = dto.Start,
                Text = dto.Text,
                MaintenanceType = maintenanceType,  // Link to MaintenanceType
            };

            _context.Tasks.Add(nextTask);
            await _context.SaveChangesAsync();

            // Linking the next task to the original one
            originalTask.Next = nextTask;
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMaintenanceTask", new { id = nextTask.Id }, TaskTransformer.TransformTask(nextTask));
        }

        // POST: api/Tasks/{id}/due-date
        [HttpPost("{id}/due-date")]
        public async Task<IActionResult> UpdateTaskDueDate(int id, DueDateUpdateDto dto)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }

            task.DueDate = dto.Date;
            _context.Entry(task).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MaintenanceTaskExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }



        private bool MaintenanceTaskExists(int id)
        {
            return (_context.Tasks?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
    
    
    public class DueDateUpdateDto
    {
        public DateTime Date { get; set; }
    }
    
    public class MaintenanceTaskDto
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string Text { get; set; }
        public int Type { get; set; }
        public int Resource { get; set; }  // Assuming this is an integer ID
    }
    
    public static class TaskTransformer {
        public static object TransformTask(MaintenanceTask task)
        {
            var result = new
            {
                id = task.Id,
                start = task.DueDate.ToString("yyyy-MM-dd"),
                end = task.DueDate.ToString("yyyy-MM-dd"),
                text = task.Text,
                type = task.MaintenanceType.Id,
                checklist = task.TaskItems.ToDictionary(ti => ti.MaintenanceTypeItemId.ToString(), ti => ti.Checked),
                next = task.Next?.Id,
                nextDate = task.Next?.DueDate,
            };

            return result;
        }
    }
}


