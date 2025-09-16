using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BalanceHub.API.Data;
using BalanceHub.API.Models;

namespace BalanceHub.API.Controllers;

[Authorize]
[ApiController]
[Route("api/tasks")]
public class TasksController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TasksController> _logger;

    public TasksController(ApplicationDbContext context, ILogger<TasksController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: api/tasks
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskDto>>> GetTasks(
        [FromQuery] string? status = null,
        [FromQuery] string? matrixType = null,
        [FromQuery] string? category = null,
        [FromQuery] string? search = null,
        [FromQuery] bool? overdue = null,
        [FromQuery] int? page = 1,
        [FromQuery] int? pageSize = 50,
        [FromQuery] string? sortBy = "calculatedPriority",
        [FromQuery] bool? descending = true)
    {
        try
        {
            // Get current user ID from JWT token
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized();

            var query = _context.Tasks
                .Where(t => t.UserId == userId.Value && !t.IsDeleted)
                .AsNoTracking();

            // Apply filters
            if (!string.IsNullOrEmpty(status))
                query = query.Where(t => t.Status == status);

            if (!string.IsNullOrEmpty(matrixType))
                query = query.Where(t => t.MatrixType == matrixType);

            if (!string.IsNullOrEmpty(category))
                query = query.Where(t => t.Category == category);

            if (!string.IsNullOrEmpty(search))
                query = query.Where(t =>
                    EF.Functions.Like(t.Title, $"%{search}%") ||
                    (t.Description != null && EF.Functions.Like(t.Description, $"%{search}%")));

            if (overdue == true)
                query = query.Where(t => t.Deadline < DateTimeOffset.UtcNow && t.Status != "completed");

            // Apply sorting
            query = sortBy?.ToLower() switch
            {
                "deadline" => descending == true
                    ? query.OrderByDescending(t => t.Deadline ?? DateTimeOffset.MaxValue)
                    : query.OrderBy(t => t.Deadline ?? DateTimeOffset.MaxValue),
                "createdat" => descending == true
                    ? query.OrderByDescending(t => t.CreatedAt)
                    : query.OrderBy(t => t.CreatedAt),
                "title" => descending == true
                    ? query.OrderByDescending(t => t.Title)
                    : query.OrderBy(t => t.Title),
                "urgency" => descending == true
                    ? query.OrderByDescending(t => t.Urgency)
                    : query.OrderBy(t => t.Urgency),
                "importance" => descending == true
                    ? query.OrderByDescending(t => t.Importance)
                    : query.OrderBy(t => t.Importance),
                _ => descending == true
                    ? query.OrderByDescending(t => t.CalculatedPriority)
                    : query.OrderBy(t => t.CalculatedPriority)
            };

            // Apply pagination
            var totalCount = await query.CountAsync();
            var tasks = await query
                .Skip((page.Value - 1) * pageSize.Value)
                .Take(pageSize.Value)
                .ToListAsync();

            var taskDtos = tasks.Select(t => MapToDto(t)).ToList();

            // Add pagination metadata
            Response.Headers["X-Total-Count"] = totalCount.ToString();
            Response.Headers["X-Page-Size"] = pageSize.Value.ToString();
            Response.Headers["X-Current-Page"] = page.Value.ToString();

            _logger.LogInformation("Retrieved {Count} tasks for user {UserId}", tasks.Count, userId);
            return Ok(taskDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tasks for user");
            return StatusCode(500, new { message = "Failed to retrieve tasks" });
        }
    }

    // GET: api/tasks/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<TaskDto>> GetTask(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized();

            var task = await _context.Tasks
                .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId.Value && !t.IsDeleted);

            if (task == null)
            {
                _logger.LogWarning("Task {Id} not found for user {UserId}", id, userId);
                return NotFound();
            }

            return Ok(MapToDto(task));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving task {Id}", id);
            return StatusCode(500, new { message = "Failed to retrieve task" });
        }
    }

    // POST: api/tasks
    [HttpPost]
    public async Task<ActionResult<TaskDto>> CreateTask([FromBody] TaskCreateDto createDto)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized();

            var task = new Models.Task
            {
                Title = createDto.Title,
                Description = createDto.Description,
                Urgency = createDto.Urgency,
                Importance = createDto.Importance,
                EstimatedHours = createDto.EstimatedHours,
                Deadline = createDto.Deadline,
                Category = createDto.Category,
                Tags = createDto.Tags ?? new List<string>(),
                UserId = userId.Value
            };

            // Auto-calculate priority using Eisenhower Matrix
            task.UpdateTimePressure();
            task.RecalculatePriority();
            task.EffortLevel = CalculateEffortLevel(task.EstimatedHours);

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created task {Id} for user {UserId}", task.Id, userId);
            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, MapToDto(task));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating task");
            return StatusCode(500, new { message = "Failed to create task" });
        }
    }

    // PUT: api/tasks/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTask(Guid id, [FromBody] TaskUpdateDto updateDto)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized();

            var task = await _context.Tasks.FindAsync(id);
            if (task == null || task.UserId != userId.Value)
                return NotFound();

            // Apply updates
            if (!string.IsNullOrWhiteSpace(updateDto.Title))
                task.Title = updateDto.Title;

            if (updateDto.Description != null)
                task.Description = updateDto.Description;

            if (updateDto.Urgency.HasValue)
                task.Urgency = updateDto.Urgency.Value;

            if (updateDto.Importance.HasValue)
                task.Importance = updateDto.Importance.Value;

            if (updateDto.EstimatedHours.HasValue)
            {
                task.EstimatedHours = updateDto.EstimatedHours.Value;
                task.EffortLevel = CalculateEffortLevel(task.EstimatedHours);
            }

            if (updateDto.Deadline.HasValue)
                task.Deadline = updateDto.Deadline.Value;

            if (!string.IsNullOrWhiteSpace(updateDto.Status))
            {
                if (ValidStatusValues.Contains(updateDto.Status))
                {
                    if (updateDto.Status == "in-progress" && task.Status != "in-progress")
                        task.Start();
                    else if (updateDto.Status == "todo" && task.Status == "in-progress")
                        task.Pause();
                    task.Status = updateDto.Status;
                }
            }

            if (updateDto.Category != null)
                task.Category = updateDto.Category;

            // Recalculate priority
            task.UpdateTimePressure();
            task.RecalculatePriority();
            task.UpdatedAt = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated task {Id}", id);
            return Ok(MapToDto(task));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task {Id}", id);
            return StatusCode(500, new { message = "Failed to update task" });
        }
    }

    // DELETE: api/tasks/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask(Guid id, [FromQuery] bool hardDelete = false)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized();

            var task = await _context.Tasks.FindAsync(id);
            if (task == null || task.UserId != userId.Value)
                return NotFound();

            if (hardDelete)
            {
                _context.Tasks.Remove(task);
                _logger.LogInformation("Hard deleted task {Id}", id);
            }
            else
            {
                task.IsDeleted = true;
                task.DeletedAt = DateTimeOffset.UtcNow;
                task.UpdatedAt = DateTimeOffset.UtcNow;
                _logger.LogInformation("Soft deleted task {Id}", id);
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting task {Id}", id);
            return StatusCode(500, new { message = "Failed to delete task" });
        }
    }

    // PATCH: api/tasks/{id}/complete
    [HttpPost("{id}/complete")]
    public async Task<IActionResult> CompleteTask(Guid id, [FromBody] CompleteTaskDto completeDto)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized();

            var task = await _context.Tasks.FindAsync(id);
            if (task == null || task.UserId != userId.Value)
                return NotFound();

            task.MarkCompleted(completeDto?.ActualHours ?? task.ActualHours);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Completed task {Id}", id);
            return Ok(MapToDto(task));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing task {Id}", id);
            return StatusCode(500, new { message = "Failed to complete task" });
        }
    }

    // PATCH: api/tasks/{id}/priority
    [HttpPatch("{id}/priority")]
    public async Task<IActionResult> UpdateTaskPriority(Guid id, [FromBody] TaskPriorityDto priorityDto)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized();

            var task = await _context.Tasks.FindAsync(id);
            if (task == null || task.UserId != userId.Value)
                return NotFound();

            task.Urgency = priorityDto.Urgency;
            task.Importance = priorityDto.Importance;
            task.Deadline = priorityDto.Deadline ?? task.Deadline;

            task.UpdateTimePressure();
            task.RecalculatePriority();
            task.UpdatedAt = DateTimeOffset.UtcNow;
            task.RescheduleCount++;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated priority for task {Id}: U={Urgency}, I={Importance}",
                id, task.Urgency, task.Importance);

            return Ok(new
            {
                taskId = task.Id,
                matrixType = task.MatrixType,
                calculatedPriority = task.CalculatedPriority,
                timePressure = task.TimePressure
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task priority for {Id}", id);
            return StatusCode(500, new { message = "Failed to update task priority" });
        }
    }

    // GET: api/tasks/analytics
    [HttpGet("analytics")]
    public async Task<ActionResult<TaskAnalyticsDto>> GetTaskAnalytics()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized();

            var tasks = await _context.Tasks
                .Where(t => t.UserId == userId.Value && !t.IsDeleted)
                .AsNoTracking()
                .ToListAsync();

            var analytics = new TaskAnalyticsDto
            {
                TotalTasks = tasks.Count,
                CompletedTasks = tasks.Count(t => t.Status == "completed"),
                PendingTasks = tasks.Count(t => t.Status != "completed"),
                OverdueTasks = tasks.Count(t => t.IsOverdue),
                TasksByPriority = tasks.GroupBy(t => t.MatrixType)
                    .ToDictionary(g => g.Key, g => g.Count()),
                TasksByCategory = tasks.Where(t => t.Category != null)
                    .GroupBy(t => t.Category!)
                    .ToDictionary(g => g.Key, g => g.Count()),
                TasksByStatus = tasks.GroupBy(t => t.Status)
                    .ToDictionary(g => g.Key, g => g.Count())
            };

            // Calculate average completion time
            var completedTasksWithData = tasks.Where(t =>
                t.Status == "completed" &&
                t.StartDate.HasValue &&
                t.CompletedAt.HasValue);

            if (completedTasksWithData.Any())
            {
                analytics.AverageCompletionTime = completedTasksWithData
                    .Average(t => (t.CompletedAt!.Value - t.StartDate!.Value).TotalHours);
            }

            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating task analytics");
            return StatusCode(500, new { message = "Failed to generate analytics" });
        }
    }

    // Helper methods
    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                         User.FindFirst("sub")?.Value;

        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    private static TaskDto MapToDto(BalanceHub.API.Models.Task task)
    {
        return new TaskDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Urgency = task.Urgency,
            Importance = task.Importance,
            MatrixType = task.MatrixType,
            CalculatedPriority = task.CalculatedPriority,
            EstimatedHours = task.EstimatedHours,
            ActualHours = task.ActualHours,
            EffortLevel = task.EffortLevel,
            Deadline = task.Deadline,
            StartDate = task.StartDate,
            CompletedAt = task.CompletedAt,
            Status = task.Status,
            Category = task.Category,
            Tags = task.Tags.ToList(),
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt,
            TimePressure = task.TimePressure,
            RescheduleCount = task.RescheduleCount,
            IsOverdue = task.IsOverdue,
            IsDueSoon = task.IsDueSoon,
            CompletionPercentage = task.CompletionPercentage,
            TimeRemainingHours = task.TimeRemaining.TotalHours
        };
    }

    private static string CalculateEffortLevel(double hours)
    {
        return hours switch
        {
            <= 1 => "low",
            <= 4 => "medium",
            _ => "high"
        };
    }

    private static readonly HashSet<string> ValidStatusValues = new()
    {
        "todo", "in-progress", "completed", "cancelled"
    };
}

public class CompleteTaskDto
{
    [Range(0.1, 100.0)]
    public double? ActualHours { get; set; }
}
