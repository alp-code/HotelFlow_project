using System;
using System.Collections.Generic;
using System.Text;

using HotelFlow.Domain.Common;
using HotelFlow.Domain.Enums;

// Domain/Entities/HousekeepingTask.cs
namespace HotelFlow.Domain.Entities;

public class HousekeepingTask : BaseEntity
{
    public Guid RoomId { get; private set; }
    public Room Room { get; private set; } = default!;

    public Guid? AssignedToUserId { get; private set; }
    public User? AssignedToUser { get; private set; } = default!;

    public HousekeepingTaskType Type { get; private set; }
    public HousekeepingTaskStatus Status { get; private set; }
    public string Description { get; private set; } = default!;
    public DateTime? CompletedAt { get; private set; }
    public DateTime Deadline { get; private set; }
    public string? Notes { get; private set; }

    private HousekeepingTask() { }

    public HousekeepingTask(
        Guid roomId,
        Guid? assignedToUserId,
        HousekeepingTaskType type,
        DateTime deadline,
        string description)
    {
        Id = Guid.NewGuid();
        RoomId = roomId;
        AssignedToUserId = assignedToUserId;
        Type = type;
        Status = HousekeepingTaskStatus.Pending;
        Description = description;
        CreatedAt = DateTime.UtcNow;
        Deadline = deadline;
    }

    public void Start()
    {
        if (Status != HousekeepingTaskStatus.Pending)
            throw new Exception("Task can only be started from Pending status");

        Status = HousekeepingTaskStatus.InProgress;
    }

    public void Complete(string? notes = null)
    {
        if (Status != HousekeepingTaskStatus.InProgress)
            throw new Exception("Task can only be completed from InProgress status");

        Status = HousekeepingTaskStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        Notes = notes;
    }

    public void Cancel(string? reason = null)
    {
        Status = HousekeepingTaskStatus.Cancelled;
        Notes = reason;
    }

    public void Reassign(Guid? newUserId)
    {
        AssignedToUserId = newUserId;
    }

    public void UpdateDeadline(DateTime newDeadline)
    {
        if (newDeadline < DateTime.UtcNow)
            throw new Exception("Deadline cannot be in the past");

        Deadline = newDeadline;
    }

    public void Fail(string? reason = null)
    {
        if (Status == HousekeepingTaskStatus.Completed || Status == HousekeepingTaskStatus.Cancelled)
            throw new Exception($"Cannot fail task with status: {Status}");

        Status = HousekeepingTaskStatus.Failed;
        Notes = reason;
    }
}

