using Shared.Extensions;
using Shared.Pagination;
using System.Linq.Expressions;

namespace Infrastructure.Extensions
{
    public static class PaginationExtensions
    {
        public static IQueryable<T> Paginate<T, TKey>(this IQueryable<T> query, PagerParameters parms,
                                                        Expression<Func<T, TKey>> orderByDefaultExpression,
                                                        Expression<Func<T, bool>> filter = null)
        {
            if (filter != null)
                query = query.Where(filter);

            if (parms.PageSize == null)
                return query;

            if (parms.SortDirection.IsNullOrEmpty())
                parms.SortDirection = "DESC";
            if (!parms.SortField.IsNullOrEmpty())
                query = query.OrderBy(parms.SortField + " " + parms.SortDirection);
            else
                query = query.OrderByDescending(orderByDefaultExpression);

            parms.PageIndex ??= 0;


            return query
                .Skip((parms.PageIndex.ToInt() - 1) * parms.PageSize.ToInt())
                .Take(parms.PageSize.ToInt());
        }

    }
}