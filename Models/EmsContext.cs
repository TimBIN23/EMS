using Microsoft.EntityFrameworkCore;
using EM.Models;

namespace EM.Data
{
    public class EmsContext : DbContext
    {
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Leave> Leaves { get; set; }
        public DbSet<Performance> Performances { get; set; }
        public DbSet<Payroll> Payrolls { get; set; }
        public DbSet<Training> Trainings { get; set; }
        public DbSet<Compliance> Compliances { get; set; }
        public DbSet<User> Users { get; set; }

        public EmsContext(DbContextOptions options) : base(options)
        {
        }
    }
}