using Hangfire.Dashboard;

namespace Web.Api.Filter
{
    public class DashboardAuthorizationFilter : IDashboardAuthorizationFilter
    {
        private readonly Func<DashboardContext, bool> _authorize;

        public DashboardAuthorizationFilter(Func<DashboardContext, bool> authorize)
        {
            _authorize = authorize;
        }

        public bool Authorize(DashboardContext context) => _authorize(context);
    }

}
