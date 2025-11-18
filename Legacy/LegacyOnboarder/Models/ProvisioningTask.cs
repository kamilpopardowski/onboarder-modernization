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

    public class ProvisioningTask
    {
        public int Id { get; set; }
        
        public int RequestRecordId { get; set; }

        
        public string TaskType { get; set; } = "";

        public ProvisioningStatus Status { get; set; } = ProvisioningStatus.Pending;

        public string? AssignedToUser { get; set; }  
        public string? AssignedToRole { get; set; }  

        public DateTime? DueDate { get; set; }

        public string? LastError { get; set; }

        public DateTime CreatedAt  { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }

        public bool IsOffboarding { get; set; }
    }
}