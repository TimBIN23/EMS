namespace EM.Models
{
    public class Performance
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public DateTime ReviewDate { get; set; }
        public decimal Score { get; set; }
        public string Comments { get; set; }

        // Navigation property - make it nullable
        public virtual Employee? Employee { get; set; }
    }
}