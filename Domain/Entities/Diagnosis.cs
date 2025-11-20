namespace Domain.Entities
{
    public class Diagnosis
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string GeneticCodeHash { get; set; } = string.Empty;
        public string DiagnosisType { get; set; } = string.Empty; // e.g. Zombie, Covid, etc.
        public string GeneticCodeJson { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public Patient Patient { get; set; } = null!;
        public bool IsInfected { get; set; } = false;
    }
}
