namespace Shared.Pagination
{
    public class PaginatedList<T>
    {
        public int Count { get; set; }
        public IList<T> List { get; set; }
    }
}