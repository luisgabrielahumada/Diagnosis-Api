using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Infrastructure.Dto;
using Infrastructure.Dto.Filters;
using Microsoft.Extensions.Caching.Memory;
using Shared.Enums;
using Shared.MapperModel;
using Shared.Pagination;
using Shared.Response;
using System.Linq.Expressions;
using static Shared.Constants;
namespace Infrastructure.Services
{
    public interface ISubjectsService
    {
        Task<ServiceResponse<PaginatedList<SubjectDto>>> GetAllAsync(int pageIndex, int pageSize, SubjectInfoDto filter = null);
        Task<ServiceResponse<SubjectDto>> GetByIdAsync(Guid id);
        Task<ServiceResponse> SaveAsync(Guid id, SubjectDto data);
        Task<ServiceResponse> DeleteAsync(Guid id);
    }

    public class SubjectsService : ISubjectsService
    {
        private readonly IReadRepository<Subject> _read;
        private readonly IWriteRepository<Subject> _write;
        private readonly IIdentityUserService _identityUser;
        private readonly ILoggingService _log;
        private readonly IMemoryCache _cache;
        private readonly IAppNotifierService _noty;

        public SubjectsService(IReadRepository<Subject> read, IWriteRepository<Subject> write, IIdentityUserService identityUser, IMemoryCache cache, IAppNotifierService notificationService)
        {
            _read = read;
            _write = write;
            _identityUser = identityUser;
            _cache = cache;
            _noty = notificationService;
        }
        public async Task<ServiceResponse<PaginatedList<SubjectDto>>> GetAllAsync(int pageIndex, int pageSize, SubjectInfoDto filter = null)
        {
            var sr = new ServiceResponse<PaginatedList<SubjectDto>>();
            try
            {
                var parameters = new PagerParameters(pageIndex, pageSize);
                parameters.SortDirection = "ASC";
                parameters.SortField = "Name";  
                var text = filter?.Text?.Trim().ToLower();

                Expression<Func<Subject, bool>> predicate = x =>
                    (string.IsNullOrWhiteSpace(text)
                        || (x.Name != null && x.Name.ToLower().Contains(text))
                        || (x.Code != null && x.Code.ToLower().Contains(text))
                        || (x.Description != null && x.Description.ToLower().Contains(text)))
                    && (!filter.StartDate.HasValue || x.CreatedAt.Date >= filter.StartDate.Value.Date)
                    && (!filter.EndDate.HasValue || x.CreatedAt.Date <= filter.EndDate.Value.Date)
                    && (!filter.IsActive.HasValue || (x.IsActive.HasValue && x.IsActive == filter.IsActive.Value))
                    && (!filter.IsDeleted.HasValue || (x.IsDeleted.HasValue && x.IsDeleted == filter.IsDeleted.Value))
                    && (!filter.Bilingual.HasValue || (x.IsBilingual.HasValue && x.IsBilingual == filter.Bilingual.Value));

                var defaultOrder = (Expression<Func<Subject, object>>)(x => x.CreatedAt);

                var includes = new[]
                {
                    nameof(Subject.AcademicArea),
                    //nameof(Subject.Campus),
                    //nameof(Subject.Teacher)
                };

                var itemsDB = await _read.GetPaginationAsync<object>(
                    parameters: parameters,
                    includes: includes,
                    filter: predicate,
                    orderBy:null
                );

                if (!itemsDB.Status)
                {
                    sr.AddErrors(itemsDB.Errors);
                    return sr;
                }
                sr.Data = new PaginatedList<SubjectDto>
                {
                    Count = itemsDB.Data.Count,
                    List = MappingHelper.MapEntityListToMapperModelList<Subject, SubjectDto>(itemsDB.Data.List)
                };
            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }
            return sr;
        }

        public async Task<ServiceResponse> SaveAsync(Guid id, SubjectDto data)
        {
            var sr = new ServiceResponse();
            try
            {
                if (id == Guid.Empty)
                {
                    var entity = data.ToEntity();
                    entity.CreatedAt = DateTime.UtcNow;
                    await _write.AddAsync(_identityUser.UserEmail, entity);

                    await _noty.NotifyAsync(
                            NotificationAction.Created, new { data = entity.Name, type = "Asignatura" });
                }
                else
                {
                    var getResp = await _read.GetByIdAsync(id);
                    if (!getResp.Status)
                    {
                        sr.AddErrors(getResp.Errors);
                        return sr;
                    }

                    var entity = getResp.Data;
                    entity.Name = data.Name;
                    entity.Alias = data.Alias;
                    entity.AcademicAreaId = data.AcademicAreaId;
                    // entity.Code         = entity.Code; // conservar
                    entity.WeeklyHours = data.WeeklyHours;
                    entity.IsBilingual = data.IsBilingual;
                    entity.Description = data.Description;
                    entity.IsActive = data.IsActive;
                    entity.UpdatedAt = DateTime.UtcNow;

                    await _write.UpdateAsync(_identityUser.UserEmail, entity);


                    await _noty.NotifyAsync(
                            NotificationAction.Updated, new { data = entity.Name, tipo = "Asignatura" });
                }

                _cache.Remove(KeyCache.Subjects);
                _cache.Remove(KeyCache.CampusConfiguration);
            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }
            return sr;
        }
        public async Task<ServiceResponse<SubjectDto>> GetByIdAsync(Guid id)
        {
            var sr = new ServiceResponse<SubjectDto>();
            try
            {

                var item = await _read
                                    .GetByIdAsync(id);
                if (!item.Status)
                {
                    sr.AddErrors(item.Errors);
                    return sr;
                }

                sr.Data = new SubjectDto(item.Data);

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
                await _noty.NotifyAsync(
                   item.Data.IsDeleted.Value ? NotificationAction.Deleted : NotificationAction.Restored,
                   new { data = item.Data.Name, type = "Asignatura" });

                _cache.Remove(KeyCache.Subjects);
                _cache.Remove(KeyCache.CampusConfiguration);
            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }
            return sr;
        }
    }
}