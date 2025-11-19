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
    public interface IAcademicEssentialKnowledgeService
    {
        Task<ServiceResponse<PaginatedList<AcademicEssentialKnowledgeDto>>> GetAllAsync(int pageIndex, int pageSize, AcademicEssentialKnowledgeInfoDto filter = null);
        Task<ServiceResponse<AcademicEssentialKnowledgeDto>> GetByIdAsync(Guid id);
        Task<ServiceResponse> SaveAsync(Guid? id, AcademicEssentialKnowledgeDto data);
        Task<ServiceResponse> DeleteAsync(Guid id);
    }

    public class AcademicEssentialKnowledgeService : IAcademicEssentialKnowledgeService
    {
        private readonly IReadRepository<AcademicEssentialKnowledge> _read;
        private readonly IWriteRepository<AcademicEssentialKnowledge> _write;
        private readonly IIdentityUserService _identityUser;
        private readonly ILoggingService _log;
        public AcademicEssentialKnowledgeService(IReadRepository<AcademicEssentialKnowledge> read, IWriteRepository<AcademicEssentialKnowledge> write, IIdentityUserService identityUser)
        {
            _read = read;
            _write = write;
            _identityUser = identityUser;
        }
        public async Task<ServiceResponse<PaginatedList<AcademicEssentialKnowledgeDto>>> GetAllAsync(int pageIndex, int pageSize, AcademicEssentialKnowledgeInfoDto filter = null)
        {
            var sr = new ServiceResponse<PaginatedList<AcademicEssentialKnowledgeDto>>();
            try
            {
                var parameters = new PagerParameters(pageIndex, pageSize);
                parameters.SortDirection = "DESC";
                parameters.SortField = "createdAt";
                var includes = new[]
                {
                    nameof(AcademicEssentialKnowledge.AcademicObjective),
                };
                var text = filter?.Text?.Trim();
                var hasText = !string.IsNullOrWhiteSpace(text);
                Expression<Func<AcademicEssentialKnowledge, bool>> predicate = a =>
                    (!hasText
                        || a.Code.Contains(text, StringComparison.OrdinalIgnoreCase)
                        || a.Description.Contains(text, StringComparison.OrdinalIgnoreCase))
                    && (!filter.AcademicObjectiveId.HasValue || a.AcademicObjectiveId == filter.AcademicObjectiveId.Value)
                    && (!filter.StartDate.HasValue || a.CreatedAt.Date >= filter.StartDate.Value)
                    && (!filter.EndDate.HasValue || a.CreatedAt.Date <= filter.EndDate.Value)
                    && (!filter.IsActive.HasValue || a.IsActive == filter.IsActive.Value)
                    && (!filter.IsDeleted.HasValue || a.IsDeleted == filter.IsDeleted.Value);
                var itemsDB = await _read
                                    .GetPaginationAsync<object>(parameters: parameters, includes: includes, filter: predicate, orderBy: null);
                if (!itemsDB.Status)
                {
                    return new ServiceResponse<PaginatedList<AcademicEssentialKnowledgeDto>>
                    { Errors = itemsDB.Errors };
                }

                var data = new PaginatedList<AcademicEssentialKnowledgeDto>
                {
                    Count = itemsDB.Data.Count,
                    List = MappingHelper
                    .MapEntityListToMapperModelList<AcademicEssentialKnowledge, AcademicEssentialKnowledgeDto>
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
        public async Task<ServiceResponse<AcademicEssentialKnowledgeDto>> GetByIdAsync(Guid id)
        {
            var sr = new ServiceResponse<AcademicEssentialKnowledgeDto>();
            try
            {

                var item = await _read
                                    .GetByIdAsync(id);
                if (!item.Status)
                {
                    sr.AddErrors(item.Errors);
                    return sr;
                }

                sr.Data = new AcademicEssentialKnowledgeDto(item.Data);

            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }
            return sr;
        }
        public async Task<ServiceResponse> SaveAsync(Guid? id, AcademicEssentialKnowledgeDto data)
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
                    item.Data.AcademicObjectiveId = data.AcademicObjectiveId ?? item.Data.AcademicObjectiveId;
                    item.Data.Code = data.Code ?? item.Data.Code;
                    item.Data.Description = data.Description ?? item.Data.Description;
                    item.Data.DescriptionEn = data.DescriptionEn ?? item.Data.DescriptionEn;
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