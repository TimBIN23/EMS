using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EM.Models
{
    public class Leave
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Employee is required")]
        public int EmployeeId { get; set; }

        [Required(ErrorMessage = "Leave type is required")]
        [StringLength(50, ErrorMessage = "Leave type cannot be longer than 50 characters")]
        public string LeaveType { get; set; }

        [Required(ErrorMessage = "Start date is required")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required")]
        public DateTime EndDate { get; set; }

        [Required(ErrorMessage = "Status is required")]
        [StringLength(50, ErrorMessage = "Status cannot be longer than 50 characters")]
        public string Status { get; set; }

        // Navigation property - make it nullable
        public virtual Employee? Employee { get; set; }
    }
}