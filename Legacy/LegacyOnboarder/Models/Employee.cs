using System.ComponentModel.DataAnnotations.Schema;

namespace LegacyOnboarder.Models;

public class Employee
{
    public int Id { get; set; }
    public string EmployeeFirstName { get; set; }
    public string EmployeeLastName { get; set; }
    public int DepartmentId { get; set; }
    [NotMapped] public string DepartmentName { get; set; }
    public EmployeeType EmployeeType { get; set; }
    public int TitleId { get; set; }
    [NotMapped] public string Title { get; set; }
    public string? TitleDescription { get; set; }
    public int? RequestRecordId { get; set; }
}
