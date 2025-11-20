using Application.DTOs;
using System.ComponentModel.DataAnnotations;

namespace Application.Dtos
{
    public class PatientDiagnosisRequestDto: PatientDto
    {
        public List<string> GeneticCode { get; set; }
    }
}
