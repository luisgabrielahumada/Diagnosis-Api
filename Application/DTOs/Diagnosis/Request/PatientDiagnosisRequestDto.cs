namespace Application.Dtos.Request
{
    public class PatientDiagnosisRequestDto
    {
        public Guid? PatientId { get; set; }
        public string? PatientFullName { get; set; }
        public string? PatientDocumentNumber { get; set; }
        public string? PatientGender { get; set; }
        public List<string> GeneticCode { get; set; } = new();
    }
}
