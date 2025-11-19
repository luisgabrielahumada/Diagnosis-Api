using Shared.Extensions;
using System.Linq.Expressions;
using System.Reflection;

namespace Infrastructure.Extensions
{
    public static class OrderByExtensions
    {
        public static IEnumerable<T> OrderBy<T>(this IEnumerable<T> enumerable, string orderBy)
        {
            return enumerable.AsQueryable().OrderBy(orderBy).AsEnumerable();
        }
        public static IQueryable<T> OrderBy<T>(this IQueryable<T> collection, string orderBy)
        {
            return ParseOrderBy(orderBy).Aggregate(collection, ApplyOrderBy);
        }

        private static IQueryable<T> ApplyOrderBy<T>(IQueryable<T> collection, OrderByInfo orderByInfo)
        {
            var props = orderByInfo.PropertyName.Split('.');
            var type = typeof(T);

            var arg = Expression.Parameter(type, "x");
            Expression expr = arg;

            foreach (var prop in props)
            {
                // Use reflection (not ComponentModel) to mirror LINQ
                var pi = type.GetProperty(prop, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                var all = type.GetProperties(BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                if (pi == null)
                    throw new ArgumentException(
                        $"Invalid OrderBy field name: {prop}. Available fields: {all.Select(x => x.Name).Aggregate((i, j) => i + "," + j)}");

                expr = Expression.Property(expr, pi);
                type = pi.PropertyType;
            }

            var delegateType = typeof(Func<,>).MakeGenericType(typeof(T), type);
            var lambda = Expression.Lambda(delegateType, expr, arg);
            var methodName = String.Empty;

            if (!orderByInfo.Initial && collection is IOrderedQueryable<T>)
            {
                if (orderByInfo.Direction == SortDirection.ASCENDING)
                    methodName = "ThenBy";
                else
                    methodName = "ThenByDescending";
            }
            else
            {
                if (orderByInfo.Direction == SortDirection.ASCENDING)
                    methodName = "OrderBy";
                else
                    methodName = "OrderByDescending";
            }

            //TODO: apply caching to the generic methodsinfos?
            return (IOrderedQueryable<T>)typeof(Queryable).GetMethods().Single(
                method => method.Name == methodName
                        && method.IsGenericMethodDefinition
                        && method.GetGenericArguments().Length == 2
                        && method.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(T), type)
                .Invoke(null, new object[] { collection, lambda });
        }
        private static IEnumerable<OrderByInfo> ParseOrderBy(string orderBy)
        {
            if (orderBy.IsNullOrEmpty())
                yield break;

            var items = orderBy.Split(',');
            var initial = true;

            foreach (var item in items)
            {
                var pair = item.Trim().Split(' ');

                if (pair.Length > 2)
                    throw new ArgumentException($"Invalid OrderBy string '{item}'. Order By Format: Property, Property2 ASC, Property2 DESC");

                var prop = pair[0].Trim();

                if (prop.IsNullOrEmpty())
                    throw new ArgumentException("Invalid Property. Order By Format: Property, Property2 ASC, Property2 DESC");

                var dir = SortDirection.ASCENDING;

                if (pair.Length == 2)
                    dir = ("desc".Equals(pair[1].Trim(), StringComparison.OrdinalIgnoreCase) ? SortDirection.DESCENDING : SortDirection.ASCENDING);

                yield return new OrderByInfo() { PropertyName = prop, Direction = dir, Initial = initial };

                initial = false;
            }
        }



        private class OrderByInfo
        {
            public string PropertyName { get; set; }
            public SortDirection Direction { get; set; }
            public bool Initial { get; set; }
        }

        private enum SortDirection
        {
            ASCENDING = 0,
            DESCENDING = 1
        }
    }
}