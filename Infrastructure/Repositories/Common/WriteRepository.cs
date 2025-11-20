using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class WriteRepository<T> : IWriteRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<T> _dbSet;

        public WriteRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task<T> AddAsync(T entity)
        {
            var originalState = _context.ChangeTracker.AutoDetectChangesEnabled;
            _context.ChangeTracker.AutoDetectChangesEnabled = false;

            try
            {
                await _dbSet.AddAsync(entity);
                await _context.SaveChangesAsync(acceptAllChangesOnSuccess: false);
                _context.ChangeTracker.AcceptAllChanges();
                return entity;
            }
            finally
            {
                _context.ChangeTracker.AutoDetectChangesEnabled = originalState;
            }
        }

        public async Task UpdateAsync(T entity)
        {
            var originalState = _context.ChangeTracker.AutoDetectChangesEnabled;
            _context.ChangeTracker.AutoDetectChangesEnabled = false;

            try
            {
                if (_context.Entry(entity).State == EntityState.Detached)
                    _dbSet.Attach(entity);

                _context.Entry(entity).State = EntityState.Modified;

                await _context.SaveChangesAsync(acceptAllChangesOnSuccess: false);
                _context.ChangeTracker.AcceptAllChanges();
            }
            finally
            {
                _context.ChangeTracker.AutoDetectChangesEnabled = originalState;
            }
        }

        public async Task DeleteAsync(T entity)
        {
            var originalState = _context.ChangeTracker.AutoDetectChangesEnabled;
            _context.ChangeTracker.AutoDetectChangesEnabled = false;

            try
            {
                if (_context.Entry(entity).State == EntityState.Detached)
                    _dbSet.Attach(entity);

                _dbSet.Remove(entity);

                await _context.SaveChangesAsync(acceptAllChangesOnSuccess: false);
                _context.ChangeTracker.AcceptAllChanges();
            }
            finally
            {
                _context.ChangeTracker.AutoDetectChangesEnabled = originalState;
            }
        }
    }
}
