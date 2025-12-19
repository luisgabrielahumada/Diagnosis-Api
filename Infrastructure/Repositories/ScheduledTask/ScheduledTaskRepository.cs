using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Shared.Response;

namespace Infrastructure.Repositories
{
    public interface IScheduledTaskRepository
    {
        Task<ServiceResponse> AddAsync(ScheduledTask input);
        Task<ServiceResponse> UpdateAsync(ScheduledTask input);
        Task<ServiceResponse> ListAsync(string module);
    }

    public class ScheduledTaskRepository : IScheduledTaskRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<ScheduledTask> _dbSet;

        public ScheduledTaskRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<ScheduledTask>();
        }

        public async Task<ServiceResponse> AddAsync(ScheduledTask input)
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

        public async Task<ServiceResponse> ListAsync(string module)
        {
            var sr = new ServiceResponse();

            try
            {
                var resp = await _dbSet
                         .AsNoTracking()
                         .Where(p => p.MethodName == module)
                         .ToListAsync();
                sr.Data = resp;
            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }

            return sr;
        }

        public async Task<ServiceResponse> UpdateAsync(ScheduledTask input)
        {
            var sr = new ServiceResponse();

            try
            {
                _dbSet.Update(input);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }

            return sr;
        }
    }
}