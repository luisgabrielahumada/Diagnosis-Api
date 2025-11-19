using Application.Dto;
using Application.Services;
using DocumentFormat.OpenXml.Spreadsheet;
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
    public interface IAcademicAreaService
    {
        Task<ServiceResponse<PaginatedList<AcademicAreaDto>>> GetAllAsync(int pageIndex, int pageSize, AcademicAreaInfoDto filter = null);
        Task<ServiceResponse<AcademicAreaDto>> GetByIdAsync(Guid id);
        Task<ServiceResponse> SaveAsync(Guid id, CreateAcademicAreaDto data);
        Task<ServiceResponse> DeleteAsync(Guid id);
    }

    public class AcademicAreaService : IAcademicAreaService
    {
        private readonly IReadRepository<AcademicArea> _read;
        private readonly IWriteRepository<AcademicArea> _write;
        private readonly IIdentityUserService _identityUser;
        private readonly ILoggingService _log;
        private readonly IMemoryCache _cache;
        private readonly IAppNotifierService _noty;

        public AcademicAreaService(
            IReadRepository<AcademicArea> read,
            IWriteRepository<AcademicArea> write,
            IIdentityUserService identityUser,
            ILoggingService log,
            IMemoryCache cache,
            IAppNotifierService notificationService)
        {
            _read = read;
            _write = write;
            _identityUser = identityUser;
            _log = log;
            _cache = cache;
            _noty = notificationService;

        }

        public async Task<ServiceResponse<PaginatedList<AcademicAreaDto>>> GetAllAsync(int pageIndex, int pageSize, AcademicAreaInfoDto filter = null)
        {
            var sr = new ServiceResponse<PaginatedList<AcademicAreaDto>>();
            try
            {
                var parameters = new PagerParameters(pageIndex, pageSize);
                parameters.SortDirection = "ASC";
                parameters.SortField = "Name";
                // Extraer fuera las variables de filtro
                var text = filter?.Text?.Trim();
                var hasText = !string.IsNullOrWhiteSpace(text);
                var startDate = filter != null && filter.StartDate.HasValue
                    ? filter.StartDate.Value.Date
                    : (DateTime?)null;
                var endDate = filter != null && filter.EndDate.HasValue
                    ? filter.EndDate.Value.Date
                    : (DateTime?)null;
                var isActive = filter != null && filter.IsActive.HasValue
                    ? filter.IsActive.Value
                    : (bool?)null;
                var isDeleted = filter != null && filter.IsDeleted.HasValue
                    ? filter.IsDeleted.Value
                    : (bool?)null;

                // Construir el predicado sin null-propagation
                Expression<Func<AcademicArea, bool>> predicate = a =>
                    (!hasText
                        || a.Name.Contains(text, StringComparison.OrdinalIgnoreCase)
                        || a.Code.Contains(text, StringComparison.OrdinalIgnoreCase)
                        || a.Description.Contains(text, StringComparison.OrdinalIgnoreCase))
                    && (!startDate.HasValue || a.CreatedAt.Date >= startDate.Value)
                    && (!endDate.HasValue || a.CreatedAt.Date <= endDate.Value)
                    && (!isActive.HasValue || a.IsActive == isActive.Value)
                    && (!isDeleted.HasValue || a.IsDeleted == isDeleted.Value);
                var includes = new[]
                {
                    nameof(AcademicArea.Subjects)
                };
                var itemsDB = await _read.GetPaginationAsync<object>(
                    parameters: parameters,
                    includes: includes,
                    filter: predicate,
                    orderBy: null
                );

                if (!itemsDB.Status)
                {
                    sr.AddErrors(itemsDB.Errors);
                    return sr;
                }

                var items = MappingHelper.MapEntityListToMapperModelList<AcademicArea, AcademicAreaDto>(itemsDB.Data.List);

                var subjectsByArea = itemsDB.Data.List.ToDictionary(
                    a => a.Id,
                    a => (a.Subjects ?? new List<Subject>())
                            .Select(s => new SubjectDto
                            {
                                Id = s.Id,
                                AcademicAreaId = s.AcademicAreaId,
                                Name = s.Name,
                                Code = s.Code,
                                Alias = s.Alias,
                                WeeklyHours = s.WeeklyHours,
                                IsBilingual = s.IsBilingual,
                                Description = s.Description,
                            })
                            .ToList()
);
                items = items.Select(dto =>
                {
                    dto.Subjects = subjectsByArea.TryGetValue(dto.Id, out var list)
                        ? list
                        : new List<SubjectDto>();
                    return dto;
                }).ToList();

                sr.Data = new PaginatedList<AcademicAreaDto>
                {
                    Count = itemsDB.Data.Count,
                    List = items
                };
            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }

            return sr;
        }


        public async Task<ServiceResponse<AcademicAreaDto>> GetByIdAsync(Guid id)
        {
            var sr = new ServiceResponse<AcademicAreaDto>();
            try
            {
                var resp = await _read.GetByIdAsync(id);
                if (!resp.Status)
                {
                    sr.AddErrors(resp.Errors);
                    return sr;
                }

                sr.Data = new AcademicAreaDto(resp.Data);
            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }
            return sr;
        }

        public async Task<ServiceResponse> SaveAsync(Guid id, CreateAcademicAreaDto data)
        {
            var sr = new ServiceResponse();
            try
            {
                if (id == Guid.Empty)
                {
                    // Crear nueva área
                    var entity = data.ToEntity();
                    entity.CreatedAt = DateTime.UtcNow;
                    var added = await _write.AddAsync(_identityUser.UserEmail, entity);
                    if (added is not AcademicArea)
                    {
                        sr.AddError("Error al guardar");
                        return sr;
                    }

                    await _noty.NotifyAsync(
                            NotificationAction.Created, new { data = entity.Name, type = "Area Academica" });
                }
                else
                {
                    // Actualizar existente
                    var getResp = await _read.GetByIdAsync(id);
                    if (!getResp.Status)
                    {
                        sr.AddErrors(getResp.Errors);
                        return sr;
                    }

                    var entity = getResp.Data;
                    entity.Name = data.Name;
                    entity.Code = data.Code;       // opcionalmente regenerar
                    entity.Description = data.Description;
                    entity.Color = data.Color;
                    entity.DisplayOrder = data.DisplayOrder;
                    entity.IsActive = data.IsActive;
                    entity.UpdatedAt = DateTime.UtcNow;

                    await _write.UpdateAsync(_identityUser.UserEmail, entity);


                    await _noty.NotifyAsync(
                            NotificationAction.Updated, new { data = entity.Name, type = "Area Academica" });
                }

                _cache.Remove(KeyCache.Areas);
                _cache.Remove(KeyCache.CampusConfiguration);
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
                var getResp = await _read.GetByIdAsync(id);
                if (!getResp.Status)
                {
                    sr.AddErrors(getResp.Errors);
                    return sr;
                }

                var entity = getResp.Data;
                entity.IsDeleted = !entity.IsDeleted;
                entity.UpdatedAt = DateTime.UtcNow;

                await _write.UpdateAsync(_identityUser.UserEmail, entity);
                _cache.Remove(KeyCache.Areas);
                await _noty.NotifyAsync(
                      entity.IsDeleted.Value ? NotificationAction.Deleted : NotificationAction.Restored,
                      new { data = entity.Name, type = "Area Academica" });

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
