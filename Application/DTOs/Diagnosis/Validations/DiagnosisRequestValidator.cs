using Application.Dtos;
using FluentValidation;
namespace Application.DTOs.Validations
{
    public class DiagnosisRequestValidator
    : AbstractValidator<PatientDiagnosisRequestDto>
    {
        public DiagnosisRequestValidator()
        {
            RuleFor(x => x.PatientId)
                 .NotNull().WithMessage("El PatientId es obligatorio.")
                 .Must(g => g != Guid.Empty)
                 .WithMessage("El PatientId no puede estar vacío.");
            RuleFor(x => x.GeneticCode)
               .NotEmpty()
               .WithMessage("El AND del paciente es requerido");
        }
    }
}
