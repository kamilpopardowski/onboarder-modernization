using System.Collections.Generic;
using System.Linq;
using LegacyOnboarder.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    
    if (!db.Departments.Any())
    {
        db.Departments.AddRange(new List<Department>
        {
            new Department { DepartmentName = "HR" },              // Id = 1
            new Department { DepartmentName = "IT" },              // Id = 2
            new Department { DepartmentName = "IT Administration"},// Id = 3
            new Department { DepartmentName = "IT Management" },   // Id = 4
            new Department { DepartmentName = "Management" },      // Id = 5
            new Department { DepartmentName = "Executives" },      // Id = 6
        });
    }
    
    if (!db.Titles.Any())
    {
        db.Titles.AddRange(new List<Title>
        {
            new Title { Name = "Software Engineer" },      // Id = 1
            new Title { Name = "Senior Software Engineer" },// Id = 2
            new Title { Name = "IT Administrator" },       // Id = 3
            new Title { Name = "IT Manager" },             // Id = 4
            new Title { Name = "HR Specialist" },          // Id = 5
            new Title { Name = "HR Manager" },             // Id = 6
            new Title { Name = "Executive Assistant" },    // Id = 7
            new Title { Name = "CTO" },                    // Id = 8
            new Title { Name = "CEO" }                     // Id = 9
        });
    }
    
    if (!db.Employees.Any())
    {
        db.Employees.AddRange(new List<Employee>
        {
            new Employee
            {
                EmployeeFirstName = "Alice",
                EmployeeLastName  = "Henderson",
                DepartmentId      = 1,   
                EmployeeType      = EmployeeType.FullTime,
                TitleId           = 5,   
                TitleDescription  = null
            },
            new Employee
            {
                EmployeeFirstName = "Bob",
                EmployeeLastName  = "Smith",
                DepartmentId      = 2,   
                EmployeeType      = EmployeeType.Contractor,
                TitleId           = 1,   
                TitleDescription  = null
            },
            new Employee
            {
                EmployeeFirstName = "Carol",
                EmployeeLastName  = "Johnson",
                DepartmentId      = 3,   
                EmployeeType      = EmployeeType.FullTime,
                TitleId           = 3,   
                TitleDescription  = null
            },
            new Employee
            {
                EmployeeFirstName = "David",
                EmployeeLastName  = "Miller",
                DepartmentId      = 4,   
                EmployeeType      = EmployeeType.FullTime,
                TitleId           = 4,   
                TitleDescription  = null
            },
            new Employee
            {
                EmployeeFirstName = "Emma",
                EmployeeLastName  = "Brown",
                DepartmentId      = 5,   
                EmployeeType      = EmployeeType.FullTime,
                TitleId           = 6,  
                TitleDescription  = null
            },
            new Employee
            {
                EmployeeFirstName = "Frank",
                EmployeeLastName  = "Wilson",
                DepartmentId      = 6,   
                EmployeeType      = EmployeeType.FullTime,
                TitleId           = 9,   
                TitleDescription  = null
            }
        });
    }

    db.SaveChanges();
}


app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Admin}/{action=Index}/{id?}");

app.Run();