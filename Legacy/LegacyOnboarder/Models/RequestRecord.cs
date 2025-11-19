using System;

namespace LegacyOnboarder.Models
{
    // This pretends to be a DB entity + DTO + view-model all at once 🤮
    public class RequestRecord
    {
        public int Id { get; set; }

        public string EmployeeFirstName { get; set; }
        public string EmployeeLastName { get; set; }

        public int DepartmentId { get; set; }
        public int EmployeeTypeId { get; set; }
        public int HiringManagerId { get; set; }

        public string? StartDate { get; set; }
        public string? TerminationDate { get; set; }

        public int TitleId { get; set; }
        public string? TitleDescription { get; set; }

        public bool Rehire { get; set; }

        public RequestStatus RequestStatus { get; set; }

        public bool IsEditing { get; set; }
        public bool IsOffboarding { get; set; }
    }
}