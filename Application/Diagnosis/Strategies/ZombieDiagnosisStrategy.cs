using Application.Dtos;
using Application.Strategies.Helper;
using Application.Strategies.Interface;
using Domain.Entities;
using Infrastructure.Interfaces;
using Newtonsoft.Json;
using Shared;
using Shared.Response;
using System.Text;

namespace Application.Strategies
{
    public class ZombieDiagnosisStrategy : IDiagnosisStrategy
    {
        public string TypeName => Constants.DiagnosisType.Zombie;
        private readonly IReadRepository<Patient> _readPatient;
        private readonly IWriteRepository<Diagnosis> _writeDiagnosis;
        public ZombieDiagnosisStrategy(IReadRepository<Patient> readPatient, IWriteRepository<Diagnosis> writeDiagnosis)
        {
            _readPatient = readPatient;
            _writeDiagnosis = writeDiagnosis;
        }

        public async Task<ServiceResponse<PatientDiagnosisResponseDto>> ExecuteAsync(PatientDiagnosisRequestDto input)
        {
            var response = new ServiceResponse<PatientDiagnosisResponseDto>();

            try
            {
                var patientData = await _readPatient.GetByIdAsync(input.PatientId);
                if (!patientData.Status)
                {
                    response.AddErrors(patientData.Errors);
                    return response;
                }

                bool isZombie = input.GeneticCode.ToArray().IsZombie();

                response.Data = new PatientDiagnosisResponseDto
                {
                    DocumentNumber = patientData.Data.DocumentNumber,
                    FullName = patientData.Data.FullName,
                    PatientId = input.PatientId,
                    Infected = isZombie,
                    Message = isZombie
                        ? "El paciente presenta secuencias compatibles con estado Zombie."
                        : "No se detectan patrones compatibles con infección Zombie."
                };


                var diagnosisRecord = new Diagnosis
                {
                    PatientId = input.PatientId,
                    DiagnosisType = TypeName,
                    IsInfected = isZombie,
                    CreatedAt = DateTime.UtcNow,
                    GeneticCodeHash = Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Join(",", input.GeneticCode))),
                    GeneticCodeJson = JsonConvert.SerializeObject(input.GeneticCode),
                    Id = Guid.NewGuid(),
                };

                await _writeDiagnosis.AddAsync(diagnosisRecord);

            }
            catch (Exception ex)
            {
                response.AddError(ex);
            }

            return response;
        }
    }
}
