using System;

namespace LegacyOnboarder.Models
{
    public enum ProvisioningStatus
    {
        Pending = 0,
        Running = 1,
        Success = 2,
        Failed  = 3
    }
    
    public enum TaskKind
    {
        Provisioning = 0,
        Checklist    = 1
    }

    public class RequestTask
    {
        public int Id { get; set; }

        // null => this row is a template
        public int? RequestRecordId { get; set; }

        public string TaskType { get; set; } = "";    // e.g. "HR_WelcomeEmail"
        public string? DisplayName { get; set; }      // e.g. "Send welcome email"

        public ProvisioningStatus Status { get; set; } = ProvisioningStatus.Pending;

        public string? AssignedToUser { get; set; }   // actual instance
        public string? AssignedToRole { get; set; }

        // default assignment (used only if this is a template)
        public string? DefaultAssignedToUser { get; set; }
        public string? DefaultAssignedToRole { get; set; }

        public DateTime? DueDate { get; set; }
        public string? LastError { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }

        public bool IsOffboarding { get; set; }       // true = termination
        public bool IsTemplate { get; set; }          // true = row is default workflow
        public TaskKind TaskKind { get; set; }        // Provisioning vs non-technical
    }
}