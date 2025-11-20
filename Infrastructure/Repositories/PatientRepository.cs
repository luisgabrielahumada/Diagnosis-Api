using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Shared.Response;

namespace Infrastructure.Repositories
{
    public interface IPatientRepository
    {
        Task<ServiceResponse<Patient>> GetByIdAsync(Guid id);
    }

    public class PatientRepository : IPatientRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<Patient> _dbSet;

        public PatientRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<Patient>();
        }

        public async Task<ServiceResponse<Patient>> GetByIdAsync(Guid id)
        {
            var sr = new ServiceResponse<Patient>();

            try
            {
                var entity = await _dbSet.FindAsync(id);

                if (entity != null)
                    _context.Entry(entity).State = EntityState.Detached;

                sr.Data = entity;
            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }

            return sr;
        }
    }
}
