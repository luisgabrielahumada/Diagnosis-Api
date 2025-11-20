using Infrastructure.Extensions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Shared.Pagination;
using Shared.Response;
using System.Linq.Expressions;

namespace Infrastructure.Repositories
{
    public class ReadRepository<T> : IReadRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<T> _dbSet;

        public ReadRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }
        public async Task<ServiceResponse<IList<T>>> GetAllAsync(
            IEnumerable<string> includes = null,
            Expression<Func<T, bool>> predicate = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            bool asNoTracking = true)
        {
            var response = new ServiceResponse<IList<T>>();

            try
            {
                var query = BuildBaseQuery(includes, asNoTracking);

                if (predicate != null)
                    query = query.Where(predicate);

                if (orderBy != null)
                    query = orderBy(query);

                response.Data = await query.ToListAsync();
            }
            catch (Exception ex)
            {
                response.AddError(ex);
            }

            return response;
        }

        public async Task<ServiceResponse<PaginatedList<T>>> GetPaginationAsync<TKey>(
            PagerParameters parameters,
            IEnumerable<string> includes = null,
            Expression<Func<T, bool>> filter = null,
            Expression<Func<T, TKey>> orderBy = null,
            bool asNoTracking = true)
        {
            var response = new ServiceResponse<PaginatedList<T>>();

            try
            {
                var query = BuildBaseQuery(includes, asNoTracking);

                if (filter != null)
                    query = query.Where(filter);

                var total = await query.CountAsync();

                var pagedQuery = query.Paginate(parameters, orderBy, filter);

                var list = await pagedQuery.ToListAsync();

                response.Data = new PaginatedList<T>
                {
                    Count = total,
                    List = list
                };
            }
            catch (Exception ex)
            {
                response.AddError(ex);
            }

            return response;
        }

        public async Task<ServiceResponse<T>> GetByIdAsync(
            Guid id,
            IEnumerable<string> includes = null,
            bool asNoTracking = true,
            bool splitQuery = true)
        {
            var response = new ServiceResponse<T>();

            try
            {
                var query = BuildBaseQuery(includes, asNoTracking);

                if (splitQuery)
                    query = query.AsSplitQuery();

                if (includes == null || !includes.Any())
                {
                    var entity = await _dbSet.FindAsync(id);
                    response.Data = entity;
                }
                else
                {
                    response.Data = await query
                        .FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id);
                }
            }
            catch (Exception ex)
            {
                response.AddError(ex);
            }

            return response;
        }

        private IQueryable<T> BuildBaseQuery(IEnumerable<string> includes, bool asNoTracking)
        {
            IQueryable<T> query = _dbSet;

            if (asNoTracking)
                query = query.AsNoTracking();

            if (includes != null && includes.Any())
            {
                query = query.AsSplitQuery();

                foreach (var path in includes)
                    query = query.Include(path);
            }
            else
            {
                query = query.AsSingleQuery();
            }

            return query;
        }
    }
}
