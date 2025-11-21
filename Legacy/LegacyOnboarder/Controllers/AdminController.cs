using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using LegacyOnboarder.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
namespace LegacyOnboarder.Controllers;

public class AdminController : Controller
{
    private readonly ILogger<AdminController> _logger;
    private readonly AppDbContext _db;

    public AdminController(ILogger<AdminController> logger, AppDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    public IActionResult Index()
    {
        return BuildIndexView();
    }

    private IActionResult BuildIndexView()
    {
        var requests = _db.Requests
            .OrderByDescending(r => r.Id)
            .ToList();

        EnsureDefaultWorkflow(_db);
        PopulateViewBag();

        foreach (var r in requests)
        {
            if (string.IsNullOrEmpty(r.TitleDescription))
            {
                r.TitleName = ((List<SelectListItem>)ViewBag.Titles)
                    ?.FirstOrDefault(t => int.Parse(t.Value) == r.TitleId)?.Text;
            }

            r.DepartmentName = ((List<SelectListItem>)ViewBag.Departments)
                ?.FirstOrDefault(d => int.Parse(d.Value) == r.DepartmentId)?.Text;
        }

        var requestType = requests.FirstOrDefault()?.IsOffboarding ?? false;
        ViewBag.RequestType = requestType; // true for offboarding, false for onboarding

        return View("Index", requests);
    }

    private void PopulateViewBag()
    {
        // LOOKUP LISTS
        ViewBag.Departments = _db.Departments
            .OrderBy(d => d.DepartmentName)
            .Select(d => new SelectListItem
            {
                Value = d.Id.ToString(),
                Text = d.DepartmentName
            })
            .ToList();

        ViewBag.Titles = _db.Titles
            .OrderBy(t => t.Name)
            .Select(t => new SelectListItem
            {
                Value = t.Id.ToString(),
                Text = t.Name
            })
            .ToList();

        ViewBag.Employees = _db.Employees
            .Where(e => e.RequestRecordId == null)
            .OrderBy(e => e.EmployeeLastName)
            .ThenBy(e => e.EmployeeFirstName)
            .Select(e => new SelectListItem
            {
                Value = e.Id.ToString(),
                Text = e.EmployeeFirstName + " " + e.EmployeeLastName
            })
            .ToList();

        ViewBag.EmployeeTypes = Enum.GetValues<EmployeeType>()
            .Select(e => new SelectListItem
            {
                Value = ((int)e).ToString(),
                Text = e.ToString()
            })
            .ToList();
    }

    private void EnsureDefaultWorkflow(AppDbContext db)
    {
        if (db.ProvisioningTasks.Any(t => t.IsTemplate && t.TaskKind == TaskKind.Checklist))
            return; // already seeded

        var defaults = new List<RequestTask>
        {
            new RequestTask
            {
                IsTemplate = true,
                TaskKind = TaskKind.Checklist,
                IsOffboarding = false,
                TaskType = "HR_WelcomeEmail",
                DisplayName = "Send welcome email",
                DefaultAssignedToRole = "HR"
            },
            new RequestTask
            {
                IsTemplate = true,
                TaskKind = TaskKind.Checklist,
                IsOffboarding = false,
                TaskType = "MANAGER_IntroMeeting",
                DisplayName = "Schedule intro meeting",
                DefaultAssignedToRole = "Manager"
            },
            new RequestTask
            {
                IsTemplate = true,
                TaskKind = TaskKind.Checklist,
                IsOffboarding = true,
                TaskType = "HR_ExitInterview",
                DisplayName = "Conduct exit interview",
                DefaultAssignedToRole = "HR"
            },
            new RequestTask
            {
                IsTemplate = true,
                TaskKind = TaskKind.Checklist,
                IsOffboarding = true,
                TaskType = "MANAGER_ResponsibilityTransfer",
                DisplayName = "Transfer responsibilities",
                DefaultAssignedToRole = "Manager"
            }
        };

        db.ProvisioningTasks.AddRange(defaults);
        db.SaveChanges();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SaveEmployee(
        string submit,
        int requestID,
        string firstName,
        string lastName,
        int department,
        int title,
        EmployeeType employeeType,
        int supervisor,
        bool isOffboarding,
        string? rehire = null,
        string? startDate = null,
        string? terminationDate = null,
        string? customTitle = null)
    {
        // editing = any existing requestId > 0 or submit text contains "edit"
        var isEditing = requestID > 0 ||
                        (submit != null && submit.Contains("edit", StringComparison.OrdinalIgnoreCase));

        var employerRequest = new RequestRecord
        {
            Id = requestID,
            EmployeeFirstName = firstName,
            EmployeeLastName = lastName,
            DepartmentId = department,
            EmployeeType = employeeType,
            ProcessManagerId = supervisor,
            StartDate = startDate,
            TerminationDate = terminationDate,
            TitleId = title == -1 ? null : title,
            TitleDescription = title == -1 ? customTitle : null,
            Rehire = rehire != null && rehire.ToLower() == "on",
            RequestStatus = RequestStatus.Submitted,
            IsEditing = isEditing,
            IsOffboarding = isOffboarding
        };

        // inline validation & helpers
        bool ValidateRequest()
        {
            // "required" fields
            if (string.IsNullOrWhiteSpace(firstName)
                || string.IsNullOrWhiteSpace(lastName)
                || department == default
                || title == default
                || employeeType == default
                || supervisor == default)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(startDate) && !IsValidDate(startDate))
                return false;

            if (!string.IsNullOrEmpty(terminationDate) && !IsValidDate(terminationDate))
                return false;

            return true;
        }

        if (!ValidateRequest())
        {
            _logger.LogWarning("Invalid employee/request data: {@EmployerRequest}", employerRequest);
            ViewBag.Error = "The employee request is invalid. Please fix the fields and try again.";

            return BuildIndexView();
        }

        // Decide status based on the magical 'submit' value
        var isCancel = string.Equals(submit, "cancel", StringComparison.OrdinalIgnoreCase);
        var isSend = submit != null && submit.Contains("send", StringComparison.OrdinalIgnoreCase);
        var isOnHold = !isCancel && !isSend;

        if (isCancel)
        {
            employerRequest.RequestStatus = RequestStatus.Deleted;
        }
        else if (isOnHold)
        {
            employerRequest.RequestStatus = RequestStatus.OnHold;
        }
        else
        {
            employerRequest.RequestStatus = RequestStatus.Submitted;
        }

        // EF direct-in-controller
        RequestRecord? existing = null;
        if (requestID > 0)
        {
            existing = _db.Requests.FirstOrDefault(r => r.Id == requestID);
        }

        if (existing != null)
        {
            existing.EmployeeFirstName = employerRequest.EmployeeFirstName;
            existing.EmployeeLastName = employerRequest.EmployeeLastName;
            existing.DepartmentId = employerRequest.DepartmentId;
            existing.EmployeeType = employerRequest.EmployeeType;
            existing.ProcessManagerId = employerRequest.ProcessManagerId;
            existing.StartDate = employerRequest.StartDate;
            existing.TerminationDate = employerRequest.TerminationDate;
            existing.TitleId = employerRequest.TitleId;
            existing.TitleDescription = employerRequest.TitleDescription;
            existing.Rehire = employerRequest.Rehire;
            existing.RequestStatus = employerRequest.RequestStatus;
            existing.IsEditing = employerRequest.IsEditing;
            existing.IsOffboarding = employerRequest.IsOffboarding;
        }
        else
        {
            _db.Requests.Add(employerRequest);
        }

        _db.SaveChanges();

        UpsertEmployeeFromRequest(employerRequest, title, customTitle);
        _db.SaveChanges();

        var requestIdForTasks = existing?.Id ?? employerRequest.Id;

        if (requestIdForTasks == 0)
        {
            // refresh from DB in case identity was generated
            requestIdForTasks = _db.Requests
                .OrderByDescending(r => r.Id)
                .Select(r => r.Id)
                .FirstOrDefault();
        }

        if (requestIdForTasks == 0 && existing != null)
            requestIdForTasks = existing.Id;

        // only create tasks on brand new requests 
        if (!isEditing)
        {
            CreateChecklistTasksForRequest(requestIdForTasks, isOffboarding);
        }

        if (employerRequest.RequestStatus != RequestStatus.Deleted)
        {
            var tasks = employerRequest.IsOffboarding
                ? BuildOffboardingTasks(requestIdForTasks)
                : BuildOnboardingTasks(requestIdForTasks);

            _db.ProvisioningTasks.AddRange(tasks);
            _db.SaveChanges();
        }

        // ViewBag message
        if (isCancel)
        {
            ViewBag.Message = "Request cancelled.";
        }
        else if (isOnHold)
        {
            ViewBag.Message = isEditing
                ? "Request changes saved and put on hold."
                : "Request put on hold.";
        }
        else
        {
            ViewBag.Message = isEditing
                ? "Request saved."
                : "Request submitted.";
        }

        ViewBag.RequestType = isOffboarding;
        return BuildIndexView();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteRequest(int id)
    {
        var request = _db.Requests.FirstOrDefault(r => r.Id == id);
        if (request == null)
            return RedirectToAction(nameof(Index));

        var tasks = _db.ProvisioningTasks.Where(t => t.RequestRecordId == id).ToList();
        if (tasks.Any())
        {
            _db.ProvisioningTasks.RemoveRange(tasks);
        }

        _db.Requests.Remove(request);
        _db.SaveChanges();

        return RedirectToAction(nameof(Index));
    }

    private bool IsValidDate(string date)
    {
        var pattern = @"^\d{4}-\d{2}-\d{2}$";
        return Regex.IsMatch(date, pattern);
    }

    private void UpsertEmployeeFromRequest(RequestRecord employerRequest, int titleId, string? customTitle)
    {
        if (employerRequest.RequestStatus == RequestStatus.Deleted)
            return;

        var normalizedTitleId = titleId == -1 ? 0 : titleId;

        var employee = _db.Employees.FirstOrDefault(e =>
            e.EmployeeFirstName == employerRequest.EmployeeFirstName &&
            e.EmployeeLastName == employerRequest.EmployeeLastName);

        if (employee == null)
        {
            _db.Employees.Add(new Employee
            {
                EmployeeFirstName = employerRequest.EmployeeFirstName,
                EmployeeLastName = employerRequest.EmployeeLastName,
                DepartmentId = employerRequest.DepartmentId,
                EmployeeType = employerRequest.EmployeeType,
                TitleId = normalizedTitleId,
                TitleDescription = titleId == -1 ? customTitle : null,
                RequestRecordId = employerRequest.Id
            });
            return;
        }

        employee.DepartmentId = employerRequest.DepartmentId;
        employee.EmployeeType = employerRequest.EmployeeType;
        employee.TitleId = normalizedTitleId;
        employee.TitleDescription = titleId == -1 ? customTitle : null;
        employee.RequestRecordId = employerRequest.Id;
    }


    private List<RequestTask> BuildOnboardingTasks(int requestId)
    {
        return new List<RequestTask>
        {
            new RequestTask
            {
                RequestRecordId = requestId,
                TaskType = "IT_CreateAdAccount",
                AssignedToRole = "IT",
                IsOffboarding = false
            },
            new RequestTask
            {
                RequestRecordId = requestId,
                TaskType = "IT_CreateEmailAccount",
                AssignedToRole = "IT",
                IsOffboarding = false
            },
            new RequestTask
            {
                RequestRecordId = requestId,
                TaskType = "IT_ProvisionLaptop",
                AssignedToRole = "IT",
                IsOffboarding = false
            },
            new RequestTask
            {
                RequestRecordId = requestId,
                TaskType = "HR_WelcomeEmail",
                AssignedToRole = "HR",
                IsOffboarding = false
            },
            new RequestTask
            {
                RequestRecordId = requestId,
                TaskType = "MANAGER_IntroMeeting",
                AssignedToRole = "Manager",
                IsOffboarding = false
            }
        };
    }

    private List<RequestTask> BuildOffboardingTasks(int requestId)
    {
        return new List<RequestTask>
        {
            new RequestTask
            {
                RequestRecordId = requestId,
                TaskType = "IT_DisableAdAccount",
                AssignedToRole = "IT",
                IsOffboarding = true
            },
            new RequestTask
            {
                RequestRecordId = requestId,
                TaskType = "IT_DisableEmail",
                AssignedToRole = "IT",
                IsOffboarding = true
            },
            new RequestTask
            {
                RequestRecordId = requestId,
                TaskType = "SECURITY_DeactivateBadge",
                AssignedToRole = "Security",
                IsOffboarding = true
            },
            new RequestTask
            {
                RequestRecordId = requestId,
                TaskType = "HR_ExitInterview",
                AssignedToRole = "HR",
                IsOffboarding = true
            },
            new RequestTask
            {
                RequestRecordId = requestId,
                TaskType = "MANAGER_ResponsibilityTransfer",
                AssignedToRole = "Manager",
                IsOffboarding = true
            }
        };
    }

    private void CreateChecklistTasksForRequest(int requestId, bool isOffboarding)
    {
        var templates = _db.ProvisioningTasks
            .Where(t => t.IsTemplate
                        && t.TaskKind == TaskKind.Checklist
                        && t.IsOffboarding == isOffboarding)
            .ToList();

        var instances = templates.Select(t => new RequestTask
        {
            RequestRecordId = requestId,
            IsTemplate = false,
            TaskKind = TaskKind.Checklist,
            IsOffboarding = t.IsOffboarding,
            TaskType = t.TaskType,
            DisplayName = t.DisplayName,
            AssignedToRole = t.DefaultAssignedToRole,
            AssignedToUser = t.DefaultAssignedToUser,
            Status = ProvisioningStatus.Pending,
            CreatedAt = DateTime.UtcNow
        }).ToList();

        if (instances.Any())
        {
            _db.ProvisioningTasks.AddRange(instances);
            _db.SaveChanges();
        }
    }

    [HttpGet]
    public IActionResult Tasks(int id)
    {
        var request = _db.Requests.FirstOrDefault(r => r.Id == id);
        if (request == null)
            return NotFound();

        var tasks = _db.ProvisioningTasks
            .Where(t => t.RequestRecordId == id && t.TaskKind == TaskKind.Checklist && !t.IsTemplate)
            .OrderBy(t => t.Id)
            .ToList();

        ViewBag.Request = request;
        PopulateViewBag();
        return View(tasks);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SaveTasks(int requestId, List<int> taskId, List<string?> assignedToUser, List<string?> status)
    {
        // super bad binding: parallel arrays
        for (int i = 0; i < taskId.Count; i++)
        {
            var id = taskId[i];
            var task = _db.ProvisioningTasks.FirstOrDefault(t => t.Id == id);
            if (task == null) continue;

            task.AssignedToUser = string.IsNullOrWhiteSpace(assignedToUser[i])
                ? null
                : assignedToUser[i];

            // Done is Success, anything else Pending
            var s = (status[i] ?? "").ToLower();
            task.Status = s == "done" ? ProvisioningStatus.Success : ProvisioningStatus.Pending;
            task.CompletedAt = task.Status == ProvisioningStatus.Success
                ? DateTime.UtcNow
                : null;
        }

        _db.SaveChanges();

        return RedirectToAction("Tasks", new { id = requestId });
    }
}
