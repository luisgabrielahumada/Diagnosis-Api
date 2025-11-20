using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class PatientDto
    {
        public Guid? PatientId { get; set; } = Guid.Empty;
        public string? FullName { get; set; }
        public string? DocumentNumber { get; set; }
    }
}
