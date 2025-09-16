using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BalanceHub.API.Data;
using BalanceHub.API.Models;

namespace BalanceHub.API.Controllers;

/// <summary>
/// TASK MANAGEMENT CONTROLLER - Eisenhower Matrix Intelligence API
///
/// This controller provides comprehensive task management capabilities powered by the
/// Eisenhower Matrix productivity method. It includes intelligent task prioritization,
/// time pressure analysis, work categorization, and productivity analytics.
///
/// AUTODISCOVERY ENDPOINTS:
/// • GET /api/tasks              - List all tasks with advanced filtering
/// • GET /api/tasks/{id}         - Get specific task details
/// • POST /api/tasks             - Create new AI-prioritized task
/// • PUT /api/tasks/{id}         - Update task with recalculation
/// • DELETE /api/tasks/{id}      - Soft delete task
/// • POST /api/tasks/{id}/complete - Mark task as completed
/// • PATCH /api/tasks/{id}/priority - Manual priority override
/// • GET /api/tasks/analytics    - Productivity insights dashboard
///
/// AUTHENTICATION REQUIRED:
/// All endpoints require Bearer token authentication obtained via /api/auth/login
/// </summary>
[Authorize]
[ApiController]
[Route("api/tasks")]
[Produces("application/json")]
[Consumes("application/json")]
[Tags("Task Management - Eisenhower Matrix API")]
public class TasksController : ApiControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TasksController> _logger;

    public TasksController(ApplicationDbContext context, ILogger<TasksController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// 🔍 LIST ALL TASKS - Eisenhower Matrix Filtered Search
    ///
    /// Retrieve tasks with advanced filtering capabilities powered by AI prioritization.
    /// Supports Eisenhower Matrix filtering, text search, status filtering, and pagination.
    /// Includes real-time time pressure calculations and completion tracking.
    /// </summary>
    ///
    /// <remarks>
    /// ## 🚀 AUTODISCOVERY: Advanced Task Filtering
    ///
    /// **Eisenhower Matrix Filters:**
    /// - `matrixType=do` - High priority tasks (Q1)
    /// - `matrixType=schedule` - Important but not urgent (Q2)
    /// - `matrixType=delegate` - Urgent but low importance (Q3)
    /// - `matrixType=delete` - Low priority items to consider removing (Q4)
    ///
    /// **Other Filters:**
    /// - `status=todo/in-progress/completed/cancelled` - Filter by completion status
    /// - `category=work/meetings/personal` - Custom category filtering
    /// - `search=meeting report` - Full-text search in title and description
    /// - `overdue=true` - Show tasks past their deadline
    ///
    /// **Sorting Options:**
    /// - `sortBy=calculatedPriority` ÷ Eisenhower Matrix score (default)
    /// - `sortBy=deadline` - Due date ordering
    /// - `sortBy=urgency/importance` - Raw urgency/importance values
    /// - `sortBy=createdAt/title` - Standard field sorting
    ///
    /// **Pagination:**
    /// - `page=1&pageSize=50` - Control result sets
    /// - Response headers include: `X-Total-Count, X-Page-Size, X-Current-Page`
    ///
    /// ## 🔐 AUTHENTICATION
    /// Requires valid Bearer token from `/api/auth/login`
    ///
    /// ## 📊 RESPONSE FORMAT
    /// Returns paginated list of task summaries with AI-powered fields
    /// </remarks>
    ///
    /// <param name="status">Filter by completion status (todo/in-progress/completed/cancelled)</param>
    /// <param name="matrixType">Eisenhower Matrix classification filter (do/schedule/delegate/delete)</param>
    /// <param name="category">Filter by custom task category or tag</param>
    /// <param name="search">Full-text search in task title and description</param>
    /// <param name="overdue">Show only overdue tasks (true/false)</param>
    /// <param name="page">Page number for pagination (default: 1)</param>
    /// <param name="pageSize">Tasks per page (default: 50, max: 100)</param>
    /// <param name="sortBy">Sort field (calculatedPriority, deadline, urgency, importance, createdAt, title)</param>
    /// <param name="descending">Sort order (true=descending, false=ascending)</param>
    ///
    /// <returns>Paginated list of tasks with AI prioritization metadata</returns>
    ///
    /// <response code="200">Tasks retrieved successfully with pagination metadata</response>
    /// <response code="401">Unauthorized - Invalid or missing JWT token</response>
    /// <response code="500">Internal server error</response>
    ///
    /// <example>
    /// **Example 1: High Priority Tasks Only**
    /// ```http
    /// GET /api/tasks?matrixType=do&status=todo&page=1&pageSize=20
    /// ```
    ///
    /// **Example 2: Overdue Tasks Search**
    /// ```http
    /// GET /api/tasks?overdue=true&sortBy=deadline&descending=false
    /// ```
    ///
    /// **Example 3: Meeting Tasks**
    /// ```http
    /// GET /api/tasks?search=meeting&category=work&sortBy=deadline
    /// ```
    /// </example>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TaskDto>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(500)]
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

    /// <summary>
    /// 🔎 GET SINGLE TASK - Detailed Task Information
    ///
    /// Retrieve complete task details including all AI-calculated fields,
    /// time pressure metrics, completion statistics, and Eisenhower Matrix scoring.
    /// </summary>
    ///
    /// <remarks>
    /// ## 🎯 AUTODISCOVERY: Detailed Task Analytics
    ///
    /// **Returns all task information including:**
    /// - Eisenhower Matrix classification and score
    /// - Time pressure and deadline analysis
    /// - Completion percentages and effort tracking
    /// - Creation/update timestamps with audit trail
    /// - Tag and category information
    /// - Reschedule count and productivity insights
    ///
    /// ## 🔐 AUTHENTICATION
    /// Requires valid Bearer token from `/api/auth/login`
    ///
    /// ## 📊 RESPONSE: Complete Task Object
    /// ```json
    /// {
    ///   "id": "task-uuid",
    ///   "title": "Prepare Quarterly Report",
    ///   "description": "...",
    ///   "matrixType": "do",
    ///   "calculatedPriority": 8.5,
    ///   "timePressure": 2.1,
    ///   "isOverdue": false,
    ///   "timeRemainingHours": 24.5,
    ///   "rescheduleCount": 2
    /// }
    /// ```
    /// </remarks>
    ///
    /// <param name="id">Task unique identifier (GUID format)</param>
    ///
    /// <returns>Complete task object with all AI-calculated fields</returns>
    ///
    /// <response code="200">Task details retrieved successfully</response>
    /// <response code="401">Unauthorized - Invalid JWT token</response>
    /// <response code="404">Task not found or belongs to different user</response>
    /// <response code="500">Internal server error</response>
    ///
    /// <example>
    /// ```http
    /// GET /api/tasks/123e4567-e89b-12d3-a456-426614174000
    /// Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
    /// ```
    /// </example>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TaskDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
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
