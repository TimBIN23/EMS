using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EM.Models
{
    public class Compliance
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Employee is required")]
        public int EmployeeId { get; set; }

        [Required(ErrorMessage = "Policy is required")]
        [StringLength(200, ErrorMessage = "Policy cannot be longer than 200 characters")]
        public string Policy { get; set; }

        [Required(ErrorMessage = "Acknowledgment date is required")]
        public DateTime AcknowledgedOn { get; set; }

        [Required(ErrorMessage = "Status is required")]
        [StringLength(50, ErrorMessage = "Status cannot be longer than 50 characters")]
        public string Status { get; set; }

        // Navigation property - make it nullable
        public virtual Employee? Employee { get; set; }
    }
}