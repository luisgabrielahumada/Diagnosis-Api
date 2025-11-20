using Shared.Pagination;
using Shared.Response;
using System.Linq.Expressions;

namespace Infrastructure.Repositories
{
    public interface IReadRepository<T> where T : class
    {
        Task<ServiceResponse<IList<T>>> GetAllAsync(
                       IEnumerable<string> includes = null,
                       Expression<Func<T, bool>> predicate = null,
                       Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
                       bool asNoTracking = true);



        Task<ServiceResponse<PaginatedList<T>>> GetPaginationAsync<TKey>(
                          PagerParameters parameters,
                          IEnumerable<string> includes = null,
                          Expression<Func<T, bool>> filter = null,
                          Expression<Func<T, TKey>> orderBy = null,
                          bool asNoTracking = true);

        Task<ServiceResponse<T>> GetByIdAsync(Guid id, IEnumerable<string> includes = null,
                                                            bool asNoTracking = true,
                                                            bool splitQuery = true);

    }

}