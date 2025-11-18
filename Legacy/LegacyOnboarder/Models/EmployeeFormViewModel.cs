using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LegacyOnboarder.Models
{
    public class EmployeeFormViewModel
    {
        public int? Id { get; set; }

        public string FirstName { get; set; } = "";
        public string LastName  { get; set; } = "";

        public string DepartmentId { get; set; } = "";
        public string? TitleId { get; set; }
        public string? CustomTitle { get; set; }
        public DateTime? StartDate { get; set; }
        public string SupervisorId { get; set; } = "";

        // Intentionally nullable to cause issues if someone forgets to populate them
        public IEnumerable<SelectListItem> Departments { get; set; } = null;
        public IEnumerable<SelectListItem> Titles { get; set; } = null;
        public IEnumerable<SelectListItem> HiringManagers { get; set; } = null;
    }
}