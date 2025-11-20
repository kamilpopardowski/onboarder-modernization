using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace LegacyOnboarder.Models
{
    // This pretends to be a DB entity + DTO + view-model all at once :D
    public class RequestRecord
    {
        public int Id { get; set; }
        public string EmployeeFirstName { get; set; }
        public string EmployeeLastName { get; set; }
        public int DepartmentId { get; set; }
        [NotMapped] public string DepartmentName { get; set; }
        public EmployeeType EmployeeType { get; set; }
        public int ProcessManagerId { get; set; }
        [NotMapped] public string ProcessManagerName { get; set; }
        public string? StartDate { get; set; }
        public string? TerminationDate { get; set; }
        public int? TitleId { get; set; }
        [NotMapped] public string? TitleName { get; set; }
        public string? TitleDescription { get; set; }
        public bool Rehire { get; set; }
        public RequestStatus RequestStatus { get; set; }
        public bool IsEditing { get; set; }
        public bool IsOffboarding { get; set; }
    }
}