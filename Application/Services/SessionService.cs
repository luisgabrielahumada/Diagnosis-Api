using Microsoft.Extensions.Caching.Memory;
using Shared.Response;
using static Shared.Constants;
namespace Infrastructure.Services
{
    public interface ISessionService
    {
        Task<ServiceResponse> ClearAll(string userEmail);
    }

    public class SessionService : ISessionService
    {

        private readonly IMemoryCache _cache;
        private readonly IUserService _userService;

        public SessionService(IMemoryCache cache, IUserService userService)
        {
            _cache = cache;
            _userService = userService;
        }

        public async Task<ServiceResponse> ClearAll(string userEmail)
        {
            var sr = new ServiceResponse();
            try
            {
                var currentUser = await _userService.GetByIdAsync(userEmail);
                if (!currentUser.Status)
                {
                    sr.AddErrors(currentUser.Errors);
                    return sr;
                }
                var roleCode = currentUser.Data.RoleCode;
                _cache.Remove($"{currentUser.Data.RoleCode}_{KeyCache.Areas}");
                _cache.Remove($"{currentUser.Data.RoleCode}_{KeyCache.Subjects}");
                _cache.Remove($"{currentUser.Data.RoleCode}_{KeyCache.CampusesAllAreas}");
                _cache.Remove($"{currentUser.Data.RoleCode}_{KeyCache.Campuses}");
            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }
            return sr;
        }
    }
}