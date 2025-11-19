using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Dto;
using Microsoft.Extensions.Caching.Memory;
using Shared.MapperModel;
using Shared.Pagination;
using Shared.Response;
using System.Linq.Expressions;
using static Shared.Constants;
namespace Infrastructure.Services
{
    public interface ICourseService
    {
        Task<ServiceResponse<PaginatedList<CourseDto>>> GetAllAsync(int pageIndex, int pageSize);
        Task<ServiceResponse<CourseDto>> GetByIdAsync(Guid id);
        Task<ServiceResponse> SaveAsync(Guid id, CourseDto data);
        Task<ServiceResponse> DeleteAsync(Guid id);
    }

    public class CourseService : ICourseService
    {
        private readonly IReadRepository<Course> _read;
        private readonly IWriteRepository<Course> _write;
        private readonly IIdentityUserService _identityUser;
        private readonly ILoggingService _log;
        private readonly IMemoryCache _cache;
        public CourseService(IReadRepository<Course> read, IWriteRepository<Course> write, IIdentityUserService identityUser, IMemoryCache cache)
        {
            _read = read;
            _write = write;
            _identityUser = identityUser;
            _cache = cache;
        }
        public async Task<ServiceResponse<PaginatedList<CourseDto>>> GetAllAsync(int pageIndex, int pageSize)
        {
            var sr = new ServiceResponse<PaginatedList<CourseDto>>();

            try
            {
                var parameters = new PagerParameters(pageIndex, pageSize);

                // Build dynamic filter expression
                Expression<Func<Course, bool>> predicate = null;
                parameters.SortDirection = "ASC";
                parameters.SortField = "Name";  // Default sorting by CreatedAt
                // Default sorting by CreatedAt

                // Generic repository call
                var itemsDB = await _read.GetPaginationAsync<object>(
                    parameters: parameters,
                    includes: null,
                    filter: predicate,
                    orderBy: null
                );

                if (!itemsDB.Status)
                {
                    sr.AddErrors(itemsDB.Errors);
                    return sr;
                }

                sr.Data = new PaginatedList<CourseDto>
                {
                    Count = itemsDB.Data.Count,
                    List = MappingHelper.MapEntityListToMapperModelList<Course, CourseDto>(itemsDB.Data.List)
                };
            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }

            return sr;
        }
        public async Task<ServiceResponse<CourseDto>> GetByIdAsync(Guid id)
        {
            var sr = new ServiceResponse<CourseDto>();
            try
            {

                var item = await _read
                                    .GetByIdAsync(id);
                if (!item.Status)
                {
                    sr.AddErrors(item.Errors);
                    return sr;
                }

                sr.Data = new CourseDto(item.Data);

            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }
            return sr;
        }
        public async Task<ServiceResponse> SaveAsync(Guid id, CourseDto data)
        {
            var sr = new ServiceResponse();
            try
            {
                if (id == Guid.Empty)
                {
                    var user = data.ToEntity();
                    await _write.AddAsync(_identityUser.UserEmail, user);
                }
                else
                {
                    var item = await _read.GetByIdAsync(id);
                    if (!item.Status)
                    {
                        sr.AddErrors(item.Errors);
                        return sr;
                    }
                    item.Data.Name = data.Name;
                    item.Data.IsActive = data.IsActive;
                    item.Data.UpdatedAt = DateTime.UtcNow;
                    item.Data.IsDeleted = data.IsDeleted;
                    await _write.UpdateAsync(_identityUser.UserEmail, item.Data);
                }
                _cache.Remove(KeyCache.Campuses);
            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }
            return sr;
        }
        public async Task<ServiceResponse> DeleteAsync(Guid id)
        {
            var sr = new ServiceResponse();
            try
            {

                var item = await _read.GetByIdAsync(id);
                if (!item.Status)
                {
                    sr.AddErrors(item.Errors);
                    return sr;
                }
                item.Data.IsDeleted = !item.Data.IsDeleted;
                await _write.UpdateAsync(_identityUser.UserEmail, item.Data);
            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }
            return sr;
        }
    }
}