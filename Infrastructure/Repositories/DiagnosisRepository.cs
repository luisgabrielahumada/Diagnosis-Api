using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Shared.Response;

namespace Infrastructure.Repositories
{
    public interface IDiagnosisRepository
    {
        Task<ServiceResponse<(int infected, int notInfected)>> StatsCountAsync(string diagnosisType);
        Task<ServiceResponse> AddAsync(Diagnosis input);
    }

    public class DiagnosisRepository : IDiagnosisRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<Diagnosis> _dbSet;

        public DiagnosisRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<Diagnosis>();
        }

        public async Task<ServiceResponse> AddAsync(Diagnosis input)
        {
            var sr = new ServiceResponse();

            try
            {
                await _dbSet.AddAsync(input);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }

            return sr;
        }

        public async Task<ServiceResponse<(int infected, int notInfected)>> StatsCountAsync(string diagnosisType)
        {
            var sr = new ServiceResponse<(int infected, int notInfected)>();

            try
            {
                var stats = await _dbSet
                    .AsNoTracking()
                    .Where(x => x.DiagnosisType == diagnosisType)
                    .GroupBy(x => 1)
                    .Select(g => new
                    {
                        Infected = g.Count(x => x.IsInfected),
                        NotInfected = g.Count(x => !x.IsInfected)
                    })
                    .FirstOrDefaultAsync();

                sr.Data = stats == null
                    ? (0, 0)
                    : (stats.Infected, stats.NotInfected);
            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }

            return sr;
        }

    }
}