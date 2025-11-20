using Application.Dtos;
using Application.DTOs;
using Application.Factory;
using Domain.Entities;
using Infrastructure.Repositories;
using Shared.Response;
namespace Application.Services
{
    public interface IDiagnosisService
    {
        Task<ServiceResponse<PatientDiagnosisResponseDto>> CreateDiagnosisAsync(string diagnosisType, PatientDiagnosisRequestDto patient);
        Task<ServiceResponse<StatsResponseDto>> StatsAsync(string diagnosisType);
    }

    public class DiagnosisService : IDiagnosisService
    {
        private readonly IDiagnosisFactory _factory;
        private readonly IDiagnosisRepository _diagnosis;
        public DiagnosisService(IDiagnosisFactory factory, IDiagnosisRepository diagnosis)
        {
            _factory = factory;
            _diagnosis = diagnosis;
        }

        public async Task<ServiceResponse<PatientDiagnosisResponseDto>> CreateDiagnosisAsync(string diagnosisType, PatientDiagnosisRequestDto patient)
        {
            var sr = new ServiceResponse<PatientDiagnosisResponseDto>()
            {
                Data = new PatientDiagnosisResponseDto()
                {
                    Infected = false,
                    Message = "No se pudo procesar el diagnóstico.",
                    PatientId = patient.PatientId,
                    DocumentNumber = patient.DocumentNumber,
                    FullName = patient.FullName
                }
            };
            try
            {

                var strategyResp = await _factory.GetStrategyAsync(diagnosisType);
                if (!strategyResp.Status)
                {
                    sr.AddErrors(strategyResp.Errors);
                    return sr;
                }

                var strategy = strategyResp.Data;
                var resp = await strategy.ExecuteAsync(patient);
                if (!resp.Status)
                {
                    sr.AddErrors(resp.Errors);
                    return sr;
                }
                sr.Data = resp.Data;
            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }
            return sr;
        }

        public async Task<ServiceResponse<StatsResponseDto>> StatsAsync(string diagnosisType)
        {
            var sr = new ServiceResponse<StatsResponseDto>();
            try
            {
                var diagnosisData = await _diagnosis.StatsCountAsync(diagnosisType);
                if (!diagnosisData.Status)
                {
                    sr.AddErrors(diagnosisData.Errors);
                    return sr;
                }

                var (infected, humans) = diagnosisData.Data;
                double ratio = humans == 0 ? 0 : (double)infected / humans;

                sr.Data = new StatsResponseDto
                {

                    CountInfected = infected,
                    CountNotInfected = humans,
                    DiagnosisType = diagnosisType,
                    Ratio = ratio
                };
            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }
            return sr;
        }
    }
}
