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
namespace Infrastructure.Services
{
    public interface IGradeService
    {
        Task<ServiceResponse<PaginatedList<GradeDto>>> GetAllAsync(int pageIndex, int pageSize, GradeInfoDto filter = null);
        Task<ServiceResponse<GradeDto>> GetByIdAsync(Guid id);
        Task<ServiceResponse> SaveAsync(Guid id, GradeDto data);
        Task<ServiceResponse> DeleteAsync(Guid id);
    }

    public class GradeService : IGradeService
    {
        private readonly IReadRepository<Grade> _read;
        private readonly IWriteRepository<Grade> _write;
        private readonly IIdentityUserService _identityUser;
        private readonly ILoggingService _log;
        private readonly IMemoryCache _cache;
        public GradeService(IReadRepository<Grade> read, IWriteRepository<Grade> write, IIdentityUserService identityUser, IMemoryCache cache)
        {
            _read = read;
            _write = write;
            _identityUser = identityUser;
            _cache = cache;
        }
        public async Task<ServiceResponse<PaginatedList<GradeDto>>> GetAllAsync(int pageIndex, int pageSize, GradeInfoDto filter)
        {
            var sr = new ServiceResponse<PaginatedList<GradeDto>>();

            try
            {
                var parameters = new PagerParameters(pageIndex, pageSize);
                parameters.SortDirection = "ASC";
                parameters.SortField = "Level";
                var text = filter?.Text?.ToLower();

                // Filtro compuesto
                Expression<Func<Grade, bool>> predicate = x =>
                    (string.IsNullOrWhiteSpace(text) ||
                        (!string.IsNullOrEmpty(x.Name) && x.Name.ToLower().Contains(text)) ||
                        (!string.IsNullOrEmpty(x.Code) && x.Code.ToLower().Contains(text)) ||
                        (!string.IsNullOrEmpty(x.Description) && x.Description.ToLower().Contains(text)))
                    &&
                    (!filter.StartDate.HasValue || x.CreatedAt.Date >= filter.StartDate.Value.Date)
                    &&
                    (!filter.EndDate.HasValue || x.CreatedAt.Date <= filter.EndDate.Value.Date)
                    &&
                    (!filter.IsActive.HasValue || (x.IsActive.HasValue && x.IsActive == filter.IsActive.Value))
                    &&
                    (!filter.IsDeleted.HasValue || (x.IsDeleted.HasValue && x.IsDeleted == filter.IsDeleted.Value));

                // Orden por defecto
                Func<IQueryable<Grade>, IOrderedQueryable<Grade>> orderBy = q => q.OrderBy(al => al.Level);


                // Llamada al método genérico
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

                sr.Data = new PaginatedList<GradeDto>
                {
                    Count = itemsDB.Data.Count,
                    List = MappingHelper.MapEntityListToMapperModelList<Grade, GradeDto>(itemsDB.Data.List)
                };
            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }

            return sr;
        }
        public async Task<ServiceResponse<GradeDto>> GetByIdAsync(Guid id)
        {
            var sr = new ServiceResponse<GradeDto>();
            try
            {

                var item = await _read
                                    .GetByIdAsync(id);
                if (!item.Status)
                {
                    sr.AddErrors(item.Errors);
                    return sr;
                }

                sr.Data = new GradeDto(item.Data);

            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }
            return sr;
        }
        public async Task<ServiceResponse> SaveAsync(Guid id, GradeDto data)
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
                    item.Data.Code = item.Data.Code;
                    //item.Data.Level = data.Level;
                    //item.Data.CampusId = data.CampusId;
                    //item.Data.Schedule = data.Schedule;
                    //item.Data.Student = data.Student;
                    // item.Data.Section = data.Section;
                    //item.Data.Capacity = data.Capacity;
                    //item.Data.DisplayOrder = data.DisplayOrder;
                    item.Data.Description = data.Description;
                    item.Data.IsActive = data.IsActive;
                    item.Data.UpdatedAt = DateTime.UtcNow;
                    //item.Data.IsDeleted = data.IsDeleted;
                    await _write.UpdateAsync(_identityUser.UserEmail, item.Data);
                }
                _cache.Remove(KeyCache.Grades);
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