using Infrastructure.Dto;
using Infrastructure.Dto.Filters;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Shared.MapperModel;
using Shared.Pagination;
using Shared.Response;
using System.Linq.Expressions;
using static Shared.Constants;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
namespace Infrastructure.Services
{
    public interface IAcademicPeriodService
    {
        Task<ServiceResponse<PaginatedList<AcademicPeriodDto>>> GetAllAsync(int pageIndex, int pageSize, PeriodInfoDto filter = null);
        Task<ServiceResponse<List<AcademicPeriodDto>>> GetAllAsync();
        Task<ServiceResponse<AcademicPeriodDto>> GetByIdAsync(Guid id);
        Task<ServiceResponse> SaveAsync(Guid id, AcademicPeriodDto data);
        Task<ServiceResponse> DeleteAsync(Guid id);
    }

    public class AcademicPeriodService : IAcademicPeriodService
    {
        private readonly IReadRepository<AcademicPeriod> _read;
        private readonly IWriteRepository<AcademicPeriod> _write;
        private readonly IIdentityUserService _identityUser;
        private readonly ILoggingService _log;
        private readonly IMemoryCache _cache;
        public AcademicPeriodService(IReadRepository<AcademicPeriod> read, IWriteRepository<AcademicPeriod> write, IIdentityUserService identityUser, IMemoryCache cache)
        {
            _read = read;
            _write = write;
            _identityUser = identityUser;
            _cache = cache;
        }
        public async Task<ServiceResponse<PaginatedList<AcademicPeriodDto>>> GetAllAsync(int pageIndex, int pageSize, PeriodInfoDto filter)
        {
            var sr = new ServiceResponse<PaginatedList<AcademicPeriodDto>>();

            try
            {
                var parameters = new PagerParameters(pageIndex, pageSize);

                Expression<Func<AcademicPeriod, bool>> predicate = x =>
                    (!filter.IsActive.HasValue || (x.IsActive.HasValue && x.IsActive == filter.IsActive)) &&
                    (!filter.IsDeleted.HasValue || (x.IsDeleted.HasValue && x.IsDeleted == filter.IsDeleted)) &&
                    (!filter.Type.HasValue || (x.Type.HasValue && x.Type == filter.Type)) &&
                    (!filter.Year.HasValue || (
                        (x.StartDate.HasValue && x.EndDate.HasValue) &&
                        x.StartDate.Value.Year <= filter.Year.Value &&
                        x.EndDate.Value.Year >= filter.Year.Value
                    )) &&
                    (string.IsNullOrWhiteSpace(filter.Search) || (!string.IsNullOrEmpty(x.Name) && x.Name.Contains(filter.Search)));

                //var defaultOrder = new List<Expression<Func<AcademicPeriod, object>>>
                //{
                //    x => x.IsActive,
                //    x => x.Type,
                //    x => x.StartDate
                //};

                var itemsDB = await _read.GetPaginationAsync<object>(
                    parameters: parameters,
                    includes: null,
                    filter: predicate,
                    orderBy: q => q.CreatedAt
                );

                if (!itemsDB.Status)
                {
                    sr.AddErrors(itemsDB.Errors);
                    return sr;
                }

                sr.Data = new PaginatedList<AcademicPeriodDto>
                {
                    Count = itemsDB.Data.Count,
                    List = MappingHelper.MapEntityListToMapperModelList<AcademicPeriod, AcademicPeriodDto>(itemsDB.Data.List)
                };
            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }

            return sr;
        }

        public async Task<ServiceResponse<List<AcademicPeriodDto>>> GetAllAsync()
        {
            var sr = new ServiceResponse<List<AcademicPeriodDto>>();
            try
            {
                var itemsDB = await _read.GetAllAsync();

                if (!itemsDB.Status)
                {
                    sr.AddErrors(itemsDB.Errors);
                    return sr;
                }

                sr.Data = MappingHelper.MapEntityListToMapperModelList<AcademicPeriod, AcademicPeriodDto>(itemsDB.Data);
            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }

            return sr;
        }

        public async Task<ServiceResponse<AcademicPeriodDto>> GetByIdAsync(Guid id)
        {
            var sr = new ServiceResponse<AcademicPeriodDto>();
            try
            {

                var item = await _read
                                    .GetByIdAsync(id);
                if (!item.Status)
                {
                    sr.AddErrors(item.Errors);
                    return sr;
                }

                sr.Data = new AcademicPeriodDto(item.Data);

            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }
            return sr;
        }
        public async Task<ServiceResponse> SaveAsync(Guid id, AcademicPeriodDto data)
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
                    item.Data.Type = data.Type;
                    item.Data.StartDate = data.StartDate;
                    item.Data.EndDate = data.EndDate;
                    item.Data.IsActive = data.IsActive;
                    item.Data.UpdatedAt = DateTime.UtcNow;
                    //item.Data.IsDeleted = data.IsDeleted;
                    await _write.UpdateAsync(_identityUser.UserEmail, item.Data);
                }
                _cache.Remove(KeyCache.Periods);
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