using Microsoft.EntityFrameworkCore;

namespace LegacyOnboarder.Models
{
    // Terrible name, lives in Models, used directly in controllers – perfect legacy energy
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
        
        public DbSet<RequestRecord> Requests { get; set; }
        public DbSet<ProvisioningTask> ProvisioningTasks { get; set; }
    }
}