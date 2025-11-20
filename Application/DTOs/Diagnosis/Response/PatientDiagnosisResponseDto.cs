using Application.DTOs;

namespace Application.Dtos
{
    public class PatientDiagnosisResponseDto : PatientDto
    {
        public bool Infected { get; set; }
        public string Message { get; set; }
    }
}
