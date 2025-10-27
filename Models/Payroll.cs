namespace EM.Models
{
    public class Payroll
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal Salary { get; set; }
        public decimal Bonus { get; set; }
        public decimal Deductions { get; set; }
        public decimal NetPay { get; set; }

        // Navigation property - make it nullable
        public virtual Employee? Employee { get; set; }
    }
}