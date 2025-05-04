using System;
using System.Collections.Generic;

namespace AspireAPI.Workflow
{
    /// <summary>
    /// Basic workflow definitions for the AspireAPI
    /// </summary>
    public class WorkflowDefinition
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<WorkflowStep> Steps { get; set; } = new List<WorkflowStep>();
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
        public WorkflowStatus Status { get; set; } = WorkflowStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }

    /// <summary>
    /// Represents a step in a workflow
    /// </summary>
    public class WorkflowStep
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public WorkflowStepType Type { get; set; }
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
        public WorkflowStepStatus Status { get; set; } = WorkflowStepStatus.Pending;
        public string DependsOn { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string Result { get; set; }
    }

    /// <summary>
    /// Enum representing the status of a workflow
    /// </summary>
    public enum WorkflowStatus
    {
        Pending,
        Running,
        Completed,
        Failed,
        Cancelled
    }

    /// <summary>
    /// Enum representing the status of a workflow step
    /// </summary>
    public enum WorkflowStepStatus
    {
        Pending,
        Running,
        Completed,
        Failed,
        Skipped
    }

    /// <summary>
    /// Enum representing the type of a workflow step
    /// </summary>
    public enum WorkflowStepType
    {
        ApiCall,
        Transformation,
        Validation,
        Notification,
        Decision,
        Custom
    }
}