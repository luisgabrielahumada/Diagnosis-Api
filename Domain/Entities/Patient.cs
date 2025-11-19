namespace Domain.Entities
{
    public class Patient
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string DocumentNumber { get; set; } = string.Empty;
        public string? Gender { get; set; }
        public DateTime CreatedAt { get; set; }
        public ICollection<Diagnosis> Diagnoses { get; set; } = new List<Diagnosis>();
    }

}
