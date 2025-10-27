namespace EM.Models
{
    public class Training
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string TrainingName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }

        // Navigation property - make it nullable
        public virtual Employee? Employee { get; set; }
    }
}