using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EM.Models
{
    public class Employee
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [StringLength(100, ErrorMessage = "First name cannot be longer than 100 characters")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(100, ErrorMessage = "Last name cannot be longer than 100 characters")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [Phone(ErrorMessage = "Invalid phone number")]
        public string? Phone { get; set; }

        [StringLength(100, ErrorMessage = "Department cannot be longer than 100 characters")]
        public string? Department { get; set; }

        [StringLength(100, ErrorMessage = "Position cannot be longer than 100 characters")]
        public string? Position { get; set; }

        [Required(ErrorMessage = "Hire date is required")]
        public DateTime HireDate { get; set; }

        [Required(ErrorMessage = "Salary is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Salary must be a positive number")]
        public decimal Salary { get; set; }

        // Navigation properties - make them nullable and initialize as empty collections
        public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public virtual ICollection<Leave> Leaves { get; set; } = new List<Leave>();
        public virtual ICollection<Performance> Performances { get; set; } = new List<Performance>();
        public virtual ICollection<Payroll> Payrolls { get; set; } = new List<Payroll>();
        public virtual ICollection<Training> Trainings { get; set; } = new List<Training>();
        public virtual ICollection<Compliance> Compliances { get; set; } = new List<Compliance>();
        public virtual User? User { get; set; }
    }
}