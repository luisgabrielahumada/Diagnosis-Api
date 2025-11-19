using Application.Dto.Filters;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Infrastructure.Dto;
using Shared;
using Shared.MapperModel;
using Shared.Pagination;
using Shared.Response;
using System.Linq.Expressions;
namespace Infrastructure.Services
{
    public interface IAcademicUnitService
    {
        Task<ServiceResponse<PaginatedList<AcademicUnitDto>>> GetAllAsync(int pageIndex, int pageSize, AcademicUnitInfoDto filter = null);
        Task<ServiceResponse<AcademicUnitDto>> GetByIdAsync(Guid id);
        Task<ServiceResponse> SaveAsync(Guid? id, AcademicUnitDto data);
        Task<ServiceResponse> DeleteAsync(Guid id);
    }

    public class AcademicUnitService : IAcademicUnitService
    {
        private readonly IReadRepository<AcademicUnit> _read;
        private readonly IWriteRepository<AcademicUnit> _write;
        private readonly IIdentityUserService _identityUser;
        private readonly ILoggingService _log;
        private readonly IReadRepository<AcademicPeriod> _readAcademicPeriod;
        public AcademicUnitService(IReadRepository<AcademicUnit> read, IWriteRepository<AcademicUnit> write,
            IIdentityUserService identityUser, IReadRepository<AcademicPeriod> readAcademicPeriod)
        {
            _read = read;
            _write = write;
            _identityUser = identityUser;
            _readAcademicPeriod = readAcademicPeriod;
        }
        public async Task<ServiceResponse<PaginatedList<AcademicUnitDto>>> GetAllAsync(int pageIndex, int pageSize, AcademicUnitInfoDto filter = null)
        {
            var sr = new ServiceResponse<PaginatedList<AcademicUnitDto>>();
            try
            {
                if (!filter.AcademicPeriodId.HasValue)
                {
                    var academicPeriod = await _readAcademicPeriod
                                                    .GetAllAsync(predicate: x => x.IsActive == true);
                    if (!academicPeriod.Status)
                    {
                        sr.AddErrors(academicPeriod.Errors);
                        return sr;
                    }

                    filter.AcademicPeriodId = academicPeriod.Data.FirstOrDefault()?.Id;
                }
                var parameters = new PagerParameters(pageIndex, pageSize);
                parameters.SortDirection = "DESC";
                parameters.SortField = "createdAt";
                var includes = new[]
                {
                    nameof(AcademicUnit.AcademicArea),
                    nameof(AcademicUnit.Grade),
                    nameof(AcademicUnit.Language),
                    nameof(AcademicUnit.Subject),
                    nameof(AcademicUnit.AcademicPeriod),
                    nameof(AcademicUnit.AcademicObjectives),
                    $"{nameof(AcademicUnit.AcademicObjectives)}.{nameof(AcademicObjective.AcademicEssentialKnowledges)}"
                };
                var text = filter?.Text?.Trim();
                var hasText = !string.IsNullOrWhiteSpace(text);
                Expression<Func<AcademicUnit, bool>> predicate = a =>
                    (!hasText
                        || a.Code.Contains(text, StringComparison.OrdinalIgnoreCase)
                        || a.Description.Contains(text, StringComparison.OrdinalIgnoreCase))
                    && (a.AcademicPeriodId == filter.AcademicPeriodId.Value)
                    && (!filter.AcademicAreaId.HasValue || a.AcademicAreaId == filter.AcademicAreaId.Value)
                    && (!filter.SubjectId.HasValue || a.SubjectId == filter.SubjectId.Value)
                    && (!filter.LanguageId.HasValue || a.LanguageId == filter.LanguageId.Value)
                    && (!filter.GradeId.HasValue || a.GradeId == filter.GradeId.Value)
                    && (!filter.StartDate.HasValue || a.CreatedAt.Date >= filter.StartDate.Value)
                    && (!filter.EndDate.HasValue || a.CreatedAt.Date <= filter.EndDate.Value)
                    && (!filter.IsActive.HasValue || a.IsActive == filter.IsActive.Value)
                    && (!filter.IsDeleted.HasValue || a.IsDeleted == filter.IsDeleted.Value);

                var itemsDB = await _read
                                    .GetPaginationAsync<object>(parameters: parameters, includes: includes, filter: predicate, orderBy: null);
                if (!itemsDB.Status)
                {
                    sr.AddErrors(itemsDB.Errors);
                    return sr;
                }

                var units = itemsDB.Data?.List ?? new List<AcademicUnit>();
                var items = MappingHelper.MapEntityListToMapperModelList<AcademicUnit, AcademicUnitDto>(units);
                var byId = units
                            .Where(u => u.Id.HasValue)
                            .ToDictionary(u => u.Id!.Value);

                foreach (var dto in items)
                {
                    if (dto?.Id == null) continue;
                    if (!byId.TryGetValue(dto.Id.Value, out var entity)) continue;

                    dto.AcademicObjectives = (entity.AcademicObjectives ?? Enumerable.Empty<AcademicObjective>())
                        .Select(o => new AcademicObjectiveDto(o))
                        .OrderBy(d => d.Id)
                        .ToList();

                    dto.AcademicEssentialKnowledges = (entity.AcademicObjectives ?? Enumerable.Empty<AcademicObjective>())
                        .SelectMany(o => o.AcademicEssentialKnowledges ?? Enumerable.Empty<AcademicEssentialKnowledge>())
                        .GroupBy(k => k.Id)
                        .Select(g => new AcademicEssentialKnowledgeDto(g.First()))
                        .OrderBy(d => d.Id)
                        .ToList();
                }

                var data = new PaginatedList<AcademicUnitDto>
                {
                    Count = itemsDB.Data.Count,
                    List = items
                };

                sr.Data = data;

            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }
            return sr;
        }
        public async Task<ServiceResponse<AcademicUnitDto>> GetByIdAsync(Guid id)
        {
            var sr = new ServiceResponse<AcademicUnitDto>();
            try
            {
                var includes = new[]
                {
                    nameof(AcademicUnit.AcademicObjectives),
                    $"{nameof(AcademicUnit.AcademicObjectives)}.{nameof(AcademicObjective.AcademicEssentialKnowledges)}"
                };
                var item = await _read
                                    .GetByIdAsync(id, includes);
                if (!item.Status)
                {
                    sr.AddErrors(item.Errors);
                    return sr;
                }

                sr.Data = new AcademicUnitDto(item.Data);

                if (item.Data.AcademicObjectives.Any())
                {
                    sr.Data.AcademicObjectives = item.Data.AcademicObjectives
                        .Select(x => new AcademicObjectiveDto(x))
                        .ToList();

                    sr.Data.AcademicEssentialKnowledges = item.Data.AcademicObjectives
                        .SelectMany(x => x.AcademicEssentialKnowledges)
                        .Select(x => new AcademicEssentialKnowledgeDto(x))
                        .ToList();
                }

            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }
            return sr;
        }
        public async Task<ServiceResponse> SaveAsync(Guid? id, AcademicUnitDto data)
        {
            var sr = new ServiceResponse();
            try
            {
                if (id == Guid.Empty || id == null)
                {
                    var user = data.ToEntity();
                    var resp = await _write.AddAsync(_identityUser.UserEmail, user);
                    if (resp == null)
                    {
                        sr.AddError(Errors.ENTITY_NOT_FOUND, Errors.ENTITY_NOT_FOUND_MESSAGE);
                        return sr;
                    }
                    sr.Data = resp.Id.Value;
                }
                else
                {
                    var item = await _read.GetByIdAsync(id.Value);
                    if (!item.Status)
                    {
                        sr.AddErrors(item.Errors);
                        return sr;
                    }
                    item.Data.AcademicAreaId = data.AcademicAreaId ?? item.Data.AcademicAreaId;
                    item.Data.Priority = data.Priority ?? item.Data.Priority;
                    item.Data.Name = data.Name ?? item.Data.Name;
                    item.Data.NameEn = data.NameEn ?? item.Data.NameEn;
                    item.Data.Code = data.Code ?? item.Data.Code;
                    item.Data.Description = data.Description ?? item.Data.Description;
                    item.Data.DescriptionEn = data.DescriptionEn ?? item.Data.DescriptionEn;
                    item.Data.GradeId = data.GradeId ?? item.Data.GradeId;
                    item.Data.AcademicPeriodId = data.AcademicPeriodId ?? item.Data.AcademicPeriodId;
                    item.Data.EstimatedHours = data.EstimatedHours  ;
                    item.Data.DisplayOrder = data.DisplayOrder ;
                    item.Data.IsActive = data.IsActive ?? item.Data.IsActive;
                    item.Data.UpdatedAt = DateTime.UtcNow;
                    item.Data.IsDeleted = data.IsDeleted ?? item.Data.IsDeleted;
                    await _write.UpdateAsync(_identityUser.UserEmail, item.Data);
                }
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