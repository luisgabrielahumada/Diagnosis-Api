using Application.Dto.Filters;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Dto;
using Shared;
using Shared.MapperModel;
using Shared.Pagination;
using Shared.Response;
using System.Linq.Expressions;
namespace Infrastructure.Services
{
    public interface IAcademicPerformanceService
    {
        Task<ServiceResponse<PaginatedList<AcademicPerformanceDto>>> GetAllAsync(int pageIndex, int pageSize, AcademicPerformanceInfoDto filter = null);
        Task<ServiceResponse<AcademicPerformanceDto>> GetByIdAsync(Guid id);
        Task<ServiceResponse> SaveAsync(Guid? id, AcademicPerformanceDto data);
        Task<ServiceResponse> DeleteAsync(Guid id);
    }

    public class AcademicPerformanceService : IAcademicPerformanceService
    {
        private readonly IReadRepository<AcademicPerformance> _read;
        private readonly IWriteRepository<AcademicPerformance> _write;
        private readonly IIdentityUserService _identityUser;
        private readonly ILoggingService _log;
        private readonly IReadRepository<AcademicPeriod> _readAcademicPeriod;
        public AcademicPerformanceService(IReadRepository<AcademicPerformance> read,
            IWriteRepository<AcademicPerformance> write, IIdentityUserService identityUser, IReadRepository<AcademicPeriod> readAcademicPeriod)
        {
            _read = read;
            _write = write;
            _identityUser = identityUser;
            _readAcademicPeriod = readAcademicPeriod;
        }
        public async Task<ServiceResponse<PaginatedList<AcademicPerformanceDto>>> GetAllAsync(int pageIndex, int pageSize, AcademicPerformanceInfoDto filter = null)
        {
            var sr = new ServiceResponse<PaginatedList<AcademicPerformanceDto>>();
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
                    nameof(AcademicPerformance.AcademicArea),
                    nameof(AcademicPerformance.Grade),
                    nameof(AcademicPerformance.Language),
                    nameof(AcademicPerformance.Subject),
                    nameof(AcademicPerformance.AcademicPeriod),
                };
                var text = filter?.Text?.Trim();
                var hasText = !string.IsNullOrWhiteSpace(text);
                Expression<Func<AcademicPerformance, bool>> predicate = a =>
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

                var data = new PaginatedList<AcademicPerformanceDto>
                {
                    Count = itemsDB.Data.Count,
                    List = MappingHelper
                    .MapEntityListToMapperModelList<AcademicPerformance, AcademicPerformanceDto>
                        (itemsDB.Data.List)
                };

                sr.Data = data;

            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }
            return sr;
        }
        public async Task<ServiceResponse<AcademicPerformanceDto>> GetByIdAsync(Guid id)
        {
            var sr = new ServiceResponse<AcademicPerformanceDto>();
            try
            {

                var item = await _read
                                    .GetByIdAsync(id);
                if (!item.Status)
                {
                    sr.AddErrors(item.Errors);
                    return sr;
                }

                sr.Data = new AcademicPerformanceDto(item.Data);

            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }
            return sr;
        }
        public async Task<ServiceResponse> SaveAsync(Guid? id, AcademicPerformanceDto data)
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
                    item.Data.Code = data.Code ?? item.Data.Code;
                    item.Data.Description = data.Description ?? item.Data.Description;
                    item.Data.DescriptionEn = data.DescriptionEn ?? item.Data.DescriptionEn;
                    item.Data.GradeId = data.GradeId ?? item.Data.GradeId;
                    item.Data.AcademicPeriodId = data.AcademicPeriodId ?? item.Data.AcademicPeriodId;
                    item.Data.DisplayOrder = data.DisplayOrder ?? item.Data.DisplayOrder;
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