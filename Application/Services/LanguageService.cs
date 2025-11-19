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
    public interface ILanguageService
    {
        Task<ServiceResponse<PaginatedList<LanguageDto>>> GetAllAsync(int pageIndex, int pageSize);
        Task<ServiceResponse<LanguageDto>> GetByIdAsync(Guid id);
        Task<ServiceResponse> SaveAsync(Guid id, LanguageDto data);
        Task<ServiceResponse> DeleteAsync(Guid id);
    }

    public class LanguageService : ILanguageService
    {
        private readonly IReadRepository<Language> _read;
        private readonly IWriteRepository<Language> _write;
        private readonly IIdentityUserService _identityUser;
        private readonly ILoggingService _log;
        private readonly IMemoryCache _cache;
        public LanguageService(IReadRepository<Language> read, IWriteRepository<Language> write, IIdentityUserService identityUser, IMemoryCache cache)
        {
            _read = read;
            _write = write;
            _identityUser = identityUser;
            _cache = cache;
        }
        public async Task<ServiceResponse<PaginatedList<LanguageDto>>> GetAllAsync(int pageIndex, int pageSize)
        {
            var sr = new ServiceResponse<PaginatedList<LanguageDto>>();

            try
            {
                var parameters = new PagerParameters(pageIndex, pageSize);

                // Build dynamic filter expression
                Expression<Func<Language, bool>> predicate = null;

                // Default sorting by CreatedAt
                Func<IQueryable<Language>, IOrderedQueryable<Language>> orderBy = q => q.OrderBy(al => al.CreatedAt);

                // Generic repository call
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

                sr.Data = new PaginatedList<LanguageDto>
                {
                    Count = itemsDB.Data.Count,
                    List = MappingHelper.MapEntityListToMapperModelList<Language, LanguageDto>(itemsDB.Data.List)
                };
            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }

            return sr;
        }
        public async Task<ServiceResponse<LanguageDto>> GetByIdAsync(Guid id)
        {
            var sr = new ServiceResponse<LanguageDto>();
            try
            {

                var item = await _read
                                    .GetByIdAsync(id);
                if (!item.Status)
                {
                    sr.AddErrors(item.Errors);
                    return sr;
                }

                sr.Data = new LanguageDto(item.Data);

            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }
            return sr;
        }
        public async Task<ServiceResponse> SaveAsync(Guid id, LanguageDto data)
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