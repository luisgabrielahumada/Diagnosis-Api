using Application.Dtos;
using Application.DTOs;
using Application.Factory;
using Domain.Entities;
using Infrastructure.Interfaces;
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
        private readonly IReadRepository<Diagnosis> _readDiagnosis;
        public DiagnosisService(IDiagnosisFactory factory, IReadRepository<Diagnosis> readDiagnosis)
        {
            _factory = factory;
            _readDiagnosis = readDiagnosis;
        }

        public async Task<ServiceResponse<PatientDiagnosisResponseDto>> CreateDiagnosisAsync(string diagnosisType, PatientDiagnosisRequestDto patient)
        {
            var sr = new ServiceResponse<PatientDiagnosisResponseDto>();
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
                var diagnosisData = await _readDiagnosis.GetAllAsync(predicate: d => d.DiagnosisType == diagnosisType);
                if (!diagnosisData.Status)
                {
                    sr.AddErrors(diagnosisData.Errors);
                    return sr;
                }

                var infected = diagnosisData.Data.Count(d => d.IsInfected);
                var humans = diagnosisData.Data.Count(x => !x.IsInfected);

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
