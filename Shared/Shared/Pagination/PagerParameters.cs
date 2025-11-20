using static Shared.Constants;

namespace Shared.Pagination
{
    public class PagerParameters
    {
        public PagerParameters(int pageIndex, int pageSize, string statusFilter = "all")
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
            IsStatus = statusFilter == StatusFilter.Active ? true :
                       statusFilter == StatusFilter.InActive ? false : (bool?)null;
        }
        public string Query { get; set; }
        public bool? IsStatus { set; get; }
        public int? PageSize { get; set; }
        public int? PageIndex { get; set; }
        public string SortField { get; set; }
        public string SortDirection { get; set; }
    }
}
