using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EM.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Employee is required")]
        public int EmployeeId { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, ErrorMessage = "Username cannot be longer than 50 characters")]
        public string Username { get; set; }

        // Remove [Required] from PasswordHash since we handle it manually
        public string PasswordHash { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required")]
        [StringLength(50, ErrorMessage = "Role cannot be longer than 50 characters")]
        public string Role { get; set; }

        // Navigation property
        public virtual Employee? Employee { get; set; }
    }
}