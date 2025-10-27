using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EM.Models
{
    public class Attendance
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Employee is required")]
        public int EmployeeId { get; set; }

        [Required(ErrorMessage = "Date is required")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Check-in time is required")]
        public TimeSpan CheckInTime { get; set; }

        public TimeSpan? CheckOutTime { get; set; }

        [Required(ErrorMessage = "Status is required")]
        [StringLength(50, ErrorMessage = "Status cannot be longer than 50 characters")]
        public string Status { get; set; }

        // Navigation property - make it nullable
        public virtual Employee? Employee { get; set; }
    }
}