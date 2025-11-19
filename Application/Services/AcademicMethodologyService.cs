using Infrastructure.Dto;
using Domain.Entities;
using Domain.Interfaces;
using Shared.MapperModel;
using Shared.Pagination;
using Shared.Response;
using Application.Dto.Filters;
using System.Linq.Expressions;
namespace Infrastructure.Services
{
    public interface IAcademicMethodologyService
    {
        Task<ServiceResponse<PaginatedList<AcademicMethodologyDto>>> GetAllAsync(int pageIndex, int pageSize, AcademicMethodologyInfoDto filter = null);
        Task<ServiceResponse<AcademicMethodologyDto>> GetByIdAsync(Guid id);
        Task<ServiceResponse> SaveAsync(Guid id, AcademicMethodologyDto data);
        Task<ServiceResponse> DeleteAsync(Guid id);
    }

    public class AcademicMethodologyService : IAcademicMethodologyService
    {
        private readonly IReadRepository<AcademicMethodology> _read;
        private readonly IWriteRepository<AcademicMethodology> _write;
        private readonly IIdentityUserService _identityUser;
        private readonly ILoggingService _log;
        public AcademicMethodologyService(IReadRepository<AcademicMethodology> read, IWriteRepository<AcademicMethodology> write, IIdentityUserService identityUser)
        {
            _read = read;
            _write = write;
            _identityUser = identityUser;
        }
        public async Task<ServiceResponse<PaginatedList<AcademicMethodologyDto>>> GetAllAsync(int pageIndex, int pageSize, AcademicMethodologyInfoDto filter = null)
        {
            var sr = new ServiceResponse<PaginatedList<AcademicMethodologyDto>>();
            try
            {
                var parameters = new PagerParameters(pageIndex, pageSize);
                parameters.SortDirection = "DESC";
                parameters.SortField = "createdAt";
                var includes = new[]
                {
                    nameof(AcademicMethodology.Language)
                };
                var text = filter?.Text?.Trim();
                var hasText = !string.IsNullOrWhiteSpace(text);
                Expression<Func<AcademicMethodology, bool>> predicate = a =>
                    (!hasText
                        || a.Code.Contains(text, StringComparison.OrdinalIgnoreCase)
                        || a.Description.Contains(text, StringComparison.OrdinalIgnoreCase))
                    && (!filter.LanguageId.HasValue || a.LanguageId == filter.LanguageId.Value)
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

                var data = new PaginatedList<AcademicMethodologyDto>
                {
                    Count = itemsDB.Data.Count,
                    List = MappingHelper
                    .MapEntityListToMapperModelList<AcademicMethodology, AcademicMethodologyDto>
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
        public async Task<ServiceResponse<AcademicMethodologyDto>> GetByIdAsync(Guid id)
        {
            var sr = new ServiceResponse<AcademicMethodologyDto>();
            try
            {

                var item = await _read
                                    .GetByIdAsync(id);
                if (!item.Status)
                {
                    sr.AddErrors(item.Errors);
                    return sr;
                }

                sr.Data = new AcademicMethodologyDto(item.Data);

            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }
            return sr;
        }
        public async Task<ServiceResponse> SaveAsync(Guid id, AcademicMethodologyDto data)
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
                    //item.Data.AcademicUnitId = data.AcademicUnitId;
                    //item.Data.AcademicAreaId = data.AcademicAreaId;
                    item.Data.Code = data.Code;
                    item.Data.Description = data.Description;
                    item.Data.DescriptionEn = data.DescriptionEn;
                    //item.Data.GradeId = data.GradeId;
                    //item.Data.AcademicPeriodId = data.AcademicPeriodId;
                    item.Data.DisplayOrder = data.DisplayOrder;
                    item.Data.IsActive = data.IsActive;
                    item.Data.UpdatedAt = DateTime.UtcNow;
                    item.Data.IsDeleted = data.IsDeleted;
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