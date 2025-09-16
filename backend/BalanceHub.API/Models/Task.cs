using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BalanceHub.API.Models;

public class Task
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(500)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    // Eisenhower Matrix Properties
    [Range(1, 10)]
    public int Urgency { get; set; } = 5; // 1-10 scale

    [Range(1, 10)]
    public int Importance { get; set; } = 5; // 1-10 scale

    // Calculated Priority Matrix Type
    [MaxLength(20)]
    public string MatrixType { get; set; } = "do"; // "do", "schedule", "delegate", "delete"

    [Range(1, 10)]
    public int CalculatedPriority { get; set; } = 5; // 1-10 overall priority score

    // Time and Effort Estimation
    [Range(0.1, 100.0)]
    public double EstimatedHours { get; set; } = 1.0;

    [Range(0.1, 100.0)]
    public double ActualHours { get; set; } = 0.0;

    [MaxLength(20)]
    public string EffortLevel { get; set; } = "medium"; // "low", "medium", "high"

    // Deadlines and Scheduling
    public DateTimeOffset? Deadline { get; set; }

    public DateTimeOffset? StartDate { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    // Task Status
    [MaxLength(20)]
    public string Status { get; set; } = "todo"; // "todo", "in-progress", "completed", "cancelled"

    // User Relationship
    [Required]
    public Guid UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;

    // Categorization
    [MaxLength(100)]
    public string? Category { get; set; }

    public ICollection<string> Tags { get; set; } = new List<string>();

    // Dependencies and Relationships
    public ICollection<Task> Dependencies { get; set; } = new List<Task>();
    public ICollection<Task> DependentTasks { get; set; } = new List<Task>();

    // Audit Properties
    [Required]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    [Required]
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public bool IsDeleted { get; set; } = false;
    public DateTimeOffset? DeletedAt { get; set; }

    // Optimization Properties
    public double TimePressure { get; set; } = 0.0; // Calculated based on deadline proximity
    public double PriorityDecay { get; set; } = 0.0; // Priority reduction over time
    public int RescheduleCount { get; set; } = 0; // Number of times task was rescheduled

    // Computed Properties
    [NotMapped]
    public bool IsOverdue => Deadline < DateTimeOffset.UtcNow && Status != "completed";

    [NotMapped]
    public bool IsDueSoon => Deadline.HasValue &&
                            Deadline.Value > DateTimeOffset.UtcNow &&
                            Deadline.Value.Subtract(DateTimeOffset.UtcNow).TotalHours < 24 &&
                            Status != "completed";

    [NotMapped]
    public double CompletionPercentage => EstimatedHours > 0 ? (ActualHours / EstimatedHours) * 100 : 0;

    [NotMapped]
    public TimeSpan TimeRemaining => Deadline?.Subtract(DateTimeOffset.UtcNow) ?? TimeSpan.Zero;

    // Methods for Eisenhower Matrix Calculations
    public void RecalculatePriority()
    {
        // Eisenhower Matrix Calculation
        if (Urgency >= 7 && Importance >= 7)
        {
            MatrixType = "do";
            CalculatedPriority = Math.Min(10, (Urgency + Importance) / 2 + (int)(TimePressure * 2));
        }
        else if (Urgency < 7 && Importance >= 7)
        {
            MatrixType = "schedule";
            CalculatedPriority = Math.Max(3, (Urgency + Importance) / 2);
        }
        else if (Urgency >= 7 && Importance < 7)
        {
            MatrixType = "delegate";
            CalculatedPriority = Math.Min(7, (Urgency + Importance) / 2);
        }
        else
        {
            MatrixType = "delete";
            CalculatedPriority = Math.Max(1, (Urgency + Importance) / 2 - 2);
        }

        // Factor in deadline pressure
        if (IsDueSoon)
            CalculatedPriority += 2;
        if (IsOverdue)
            CalculatedPriority += 3;

        CalculatedPriority = Math.Clamp(CalculatedPriority, 1, 10);
    }

    public void UpdateTimePressure()
    {
        if (!Deadline.HasValue)
        {
            TimePressure = 0;
            return;
        }

        var timeToDeadline = Deadline.Value.Subtract(DateTimeOffset.UtcNow);
        var hoursToDeadline = timeToDeadline.TotalHours;

        if (hoursToDeadline <= 0)
            TimePressure = 5.0; // Maximum pressure for overdue tasks
        else if (hoursToDeadline <= 2)
            TimePressure = 4.5;
        else if (hoursToDeadline <= 6)
            TimePressure = 4.0;
        else if (hoursToDeadline <= 12)
            TimePressure = 3.5;
        else if (hoursToDeadline <= 24)
            TimePressure = 3.0;
        else if (hoursToDeadline <= 72)
            TimePressure = 2.0;
        else if (hoursToDeadline <= 168) // 1 week
            TimePressure = 1.0;
        else
            TimePressure = 0.5; // More than a week away

        // Increase pressure based on estimated effort
        TimePressure *= Math.Sqrt(EstimatedHours) / 2;
    }

    public void MarkCompleted(double actualHours)
    {
        Status = "completed";
        ActualHours = actualHours;
        CompletedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Start()
    {
        Status = "in-progress";
        StartDate = StartDate ?? DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Pause()
    {
        Status = "todo";
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}

public class TaskCreateDto
{
    [Required]
    [MaxLength(500)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    [Range(1, 10)]
    public int Urgency { get; set; } = 5;

    [Range(1, 10)]
    public int Importance { get; set; } = 5;

    [Range(0.1, 100.0)]
    public double EstimatedHours { get; set; } = 1.0;

    public DateTimeOffset? Deadline { get; set; }

    [MaxLength(100)]
    public string? Category { get; set; }

    public List<string> Tags { get; set; } = new List<string>();
}

public class TaskUpdateDto
{
    [MaxLength(500)]
    public string? Title { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    [Range(1, 10)]
    public int? Urgency { get; set; }

    [Range(1, 10)]
    public int? Importance { get; set; }

    [Range(0.1, 100.0)]
    public double? EstimatedHours { get; set; }

    public DateTimeOffset? Deadline { get; set; }

    [MaxLength(20)]
    public string? Status { get; set; }

    [MaxLength(100)]
    public string? Category { get; set; }
}

public class TaskDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Urgency { get; set; }
    public int Importance { get; set; }
    public string MatrixType { get; set; } = string.Empty;
    public int CalculatedPriority { get; set; }
    public double EstimatedHours { get; set; }
    public double ActualHours { get; set; }
    public string EffortLevel { get; set; } = string.Empty;
    public DateTimeOffset? Deadline { get; set; }
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Category { get; set; }
    public List<string> Tags { get; set; } = new List<string>();
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public double TimePressure { get; set; }
    public int RescheduleCount { get; set; }

    // Computed properties for API responses
    public bool IsOverdue { get; set; }
    public bool IsDueSoon { get; set; }
    public double CompletionPercentage { get; set; }
    public double TimeRemainingHours { get; set; }
}

public class TaskPriorityDto
{
    public int Urgency { get; set; }
    public int Importance { get; set; }
    public DateTimeOffset? Deadline { get; set; }
}

public class TaskAnalyticsDto
{
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int PendingTasks { get; set; }
    public int OverdueTasks { get; set; }
    public double AverageCompletionTime { get; set; }
    public Dictionary<string, int> TasksByPriority { get; set; } = new();
    public Dictionary<string, int> TasksByCategory { get; set; } = new();
    public Dictionary<string, int> TasksByStatus { get; set; } = new();
}
