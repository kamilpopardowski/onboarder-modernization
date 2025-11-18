using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using LegacyOnboarder.Models;

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
        var requests = _db.Requests
            .OrderByDescending(r => r.Id)
            .ToList(); // entity = view model

        return View(requests);
    }


    public IActionResult Privacy()
    {
        return View();
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
        int employeeType,
        int supervisor,
        string? rehire = null,
        string? startDate = null,
        string? terminationDate = null,
        string? customTitle = null)
    {
       // editing = any existing requestId > 0 or submit text contains "edit"
        var isEditing = requestID > 0 || (submit != null && submit.Contains("edit", StringComparison.OrdinalIgnoreCase));
        
        var employerRequest = new RequestRecord
        {
            Id = requestID,
            EmployeeFirstName = firstName,
            EmployeeLastName  = lastName,
            DepartmentId      = department,
            EmployeeTypeId    = employeeType,
            HiringManagerId   = supervisor,
            StartDate         = startDate,
            TerminationDate   = terminationDate,
            TitleId           = title,
            TitleDescription  = customTitle,
            Rehire            = rehire != null && rehire.ToLower() == "on",
            RequestStatus     = RequestStatus.Submitted,
            IsEditing         = isEditing
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

            // reload list for Index view, entity == view model
            var invalidList = _db.Requests
                .OrderByDescending(r => r.Id)
                .ToList();

            return View("Index", invalidList);
        }

        // Decide status based on the magical 'submit' value
        var isCancel = string.Equals(submit, "cancel", StringComparison.OrdinalIgnoreCase);
        var isSend   = submit != null && submit.Contains("send", StringComparison.OrdinalIgnoreCase);
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
            existing.EmployeeLastName  = employerRequest.EmployeeLastName;
            existing.DepartmentId      = employerRequest.DepartmentId;
            existing.EmployeeTypeId    = employerRequest.EmployeeTypeId;
            existing.HiringManagerId   = employerRequest.HiringManagerId;
            existing.StartDate         = employerRequest.StartDate;
            existing.TerminationDate   = employerRequest.TerminationDate;
            existing.TitleId           = employerRequest.TitleId;
            existing.TitleDescription  = employerRequest.TitleDescription;
            existing.Rehire            = employerRequest.Rehire;
            existing.RequestStatus     = employerRequest.RequestStatus;
            existing.IsEditing         = employerRequest.IsEditing;
        }
        else
        {
            _db.Requests.Add(employerRequest);
        }

        _db.SaveChanges();

        // ViewBag message lottery
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

        // Reload list for Index
        var list = _db.Requests
            .OrderByDescending(r => r.Id)
            .ToList();

        return View("Index", list);
    }
    
    private bool IsValidDate(string date)
    {
        var pattern = @"^\d{4}-\d{2}-\d{2}$";
        return Regex.IsMatch(date, pattern);
    }
}