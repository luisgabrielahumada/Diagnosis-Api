using Application.Dtos;
using Application.Strategies.Helper;
using Application.Strategies.Interface;
using Domain.Entities;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication;
using Newtonsoft.Json;
using Shared;
using Shared.Response;
using System.Text;

namespace Application.Strategies
{
    public class ZombieDiagnosisStrategy : IDiagnosisStrategy
    {
        public string TypeName => Constants.DiagnosisType.Zombie;
        private readonly IPatientRepository _patient;
        private readonly IDiagnosisRepository _diagnosis;
        public ZombieDiagnosisStrategy(IPatientRepository patient, IDiagnosisRepository diagnosis)
        {
            _patient = patient;
            _diagnosis = diagnosis;
        }

        public async Task<ServiceResponse<PatientDiagnosisResponseDto>> ExecuteAsync(PatientDiagnosisRequestDto input)
        {
            var response = new ServiceResponse<PatientDiagnosisResponseDto>();

            try
            {
                var patientData = await _patient.GetByIdAsync(input.PatientId.Value);
                if (!patientData.Status)
                {
                    response.AddErrors(patientData.Errors);
                    return response;
                }

                if(patientData.Data is not Patient)
                {
                    response.AddError("Paciente no encontrado.");
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
                    PatientId = input.PatientId.Value,
                    DiagnosisType = TypeName,
                    IsInfected = isZombie,
                    CreatedAt = DateTime.UtcNow,
                    GeneticCodeHash = Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Join(",", input.GeneticCode))),
                    GeneticCodeJson = JsonConvert.SerializeObject(input.GeneticCode),
                    Id = Guid.NewGuid(),
                };

                await _diagnosis.AddAsync(diagnosisRecord);

            }
            catch (Exception ex)
            {
                response.AddError(ex);
            }

            return response;
        }
    }
}
