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
    public interface ICompetencyService
    {
        Task<ServiceResponse<PaginatedList<CompetenceDto>>> GetAllAsync(int pageIndex, int pageSize, CompetenceInfoDto filter = null);
        Task<ServiceResponse<CompetenceDto>> GetByIdAsync(Guid id);
        Task<ServiceResponse> SaveAsync(Guid? id, CompetenceDto data);
        Task<ServiceResponse> DeleteAsync(Guid id);
    }

    public class CompetencyService : ICompetencyService
    {
        private readonly IReadRepository<AcademicPeriod> _readAcademicPeriod;
        private readonly IReadRepository<Competence> _read;
        private readonly IWriteRepository<Competence> _write;
        private readonly IIdentityUserService _identityUser;
        private readonly ILoggingService _log;
        public CompetencyService(
            IReadRepository<Competence> read, 
            IWriteRepository<Competence> write, 
            IIdentityUserService identityUser,
            IReadRepository<AcademicPeriod> readAcademicPeriod
            )
        {
            _read = read;
            _write = write;
            _identityUser = identityUser;
            _readAcademicPeriod = readAcademicPeriod;
        }
        public async Task<ServiceResponse<PaginatedList<CompetenceDto>>> GetAllAsync(int pageIndex, int pageSize, CompetenceInfoDto filter = null)
        {
            var sr = new ServiceResponse<PaginatedList<CompetenceDto>>();
            try
            {

                if (!filter.AcademicPeriodId.HasValue)
                {
                    var academicPeriod = await _readAcademicPeriod
                                                    .GetAllAsync(predicate:x=> x.IsActive == true);
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
                    nameof(Competence.AcademicArea),
                    nameof(Competence.Grade),
                    nameof(Competence.Language),
                    nameof(Competence.Subject),
                    nameof(Competence.AcademicPeriod),
                };
                var text = filter?.Text?.Trim();
                var hasText = !string.IsNullOrWhiteSpace(text);
                Expression<Func<Competence, bool>> predicate = a =>
                    (!hasText
                        || a.Name.Contains(text, StringComparison.OrdinalIgnoreCase)
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
                var data = new PaginatedList<CompetenceDto>
                {
                    Count = itemsDB.Data.Count,
                    List = MappingHelper
                    .MapEntityListToMapperModelList<Competence, CompetenceDto>
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
        public async Task<ServiceResponse<CompetenceDto>> GetByIdAsync(Guid id)
        {
            var sr = new ServiceResponse<CompetenceDto>();
            try
            {

                var item = await _read
                                    .GetByIdAsync(id);
                if (!item.Status)
                {
                    sr.AddErrors(item.Errors);
                    return sr;
                }

                sr.Data = new CompetenceDto(item.Data);

            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }
            return sr;
        }
        public async Task<ServiceResponse> SaveAsync(Guid? id, CompetenceDto data)
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
                    item.Data.Name = data.Name ?? item.Data.Name;
                    item.Data.NameEn = data.NameEn ?? item.Data.NameEn;
                    item.Data.Code = data.Code ?? item.Data.Code;
                    item.Data.Description = data.Description ?? item.Data.Description;
                    item.Data.DescriptionEn = data.DescriptionEn ?? item.Data.DescriptionEn;
                    item.Data.GradeId = data.GradeId ?? item.Data.GradeId;
                    item.Data.AcademicPeriodId = data.AcademicPeriodId ?? item.Data.AcademicPeriodId;
                    item.Data.IsActive = data.IsActive ?? item.Data.IsActive;
                    item.Data.UpdatedAt = DateTime.UtcNow ;
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