using Microsoft.EntityFrameworkCore;

namespace LegacyOnboarder.Models
{
    // Terrible name, lives in Models, used directly in controllers
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
        
        public DbSet<RequestRecord> Requests { get; set; }
        public DbSet<RequestTask> ProvisioningTasks { get; set; }

        public DbSet<Department> Departments { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Title> Titles { get; set; }
    }
}