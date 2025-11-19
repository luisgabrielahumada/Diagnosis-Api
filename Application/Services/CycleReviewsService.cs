using Application.Services;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Dto;
using Shared.Enums;
using Shared.MapperModel;
using Shared.Pagination;
using Shared.Response;
using System.Linq.Expressions;
namespace Infrastructure.Services
{
    public interface ICycleReviewsService
    {
        Task<ServiceResponse<PaginatedList<CycleReviewDto>>> GetAllAsync(Guid id, int pageIndex = 1, int pageSize = 25);
        Task<ServiceResponse<CycleReviewDto>> GetByIdAsync(Guid id);
        Task<ServiceResponse> SaveAsync(Guid id, CycleReviewDto data);
        Task<ServiceResponse> DeleteAsync(Guid id);
    }

    public class CycleReviewsService : ICycleReviewsService
    {
        private readonly IReadRepository<PlanningCycle> _readPlanningCycle;
        private readonly IReadRepository<CycleReview> _read;
        private readonly IWriteRepository<CycleReview> _write;
        private readonly IIdentityUserService _identityUser;
        private readonly ILoggingService _log;
        private readonly IAppNotifierService _noty;
        public CycleReviewsService(IReadRepository<CycleReview> read, 
            IWriteRepository<CycleReview> write, IIdentityUserService identityUser, IAppNotifierService noty, IReadRepository<PlanningCycle> readPlanningCycle)
        {
            _read = read;
            _write = write;
            _identityUser = identityUser;
            _noty = noty;
            _readPlanningCycle = readPlanningCycle;
        }
        public async Task<ServiceResponse<PaginatedList<CycleReviewDto>>> GetAllAsync(Guid id, int pageIndex = 1, int pageSize = 25)
        {
            var sr = new ServiceResponse<PaginatedList<CycleReviewDto>>();
            try
            {
                var parameters = new PagerParameters(pageIndex, pageSize);
                Expression<Func<CycleReview, bool>> predicate = t => t.PlanningCycleId == id;
                var items = await _read
                                    .GetPaginationAsync<object>(parameters, includes: null, predicate, orderBy: q => q.CreatedAt);
                if (!items.Status)
                {
                    return new ServiceResponse<PaginatedList<CycleReviewDto>>
                    { Errors = items.Errors };
                }

                var data = new PaginatedList<CycleReviewDto>
                {
                    Count = items.Data.Count,
                    List = MappingHelper
                    .MapEntityListToMapperModelList<CycleReview, CycleReviewDto>
                        (items.Data.List)
                };

                sr.Data = data;

            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }
            return sr;
        }
        public async Task<ServiceResponse<CycleReviewDto>> GetByIdAsync(Guid id)
        {
            var sr = new ServiceResponse<CycleReviewDto>();
            try
            {

                var item = await _read
                                    .GetByIdAsync(id);
                if (!item.Status)
                {
                    sr.AddErrors(item.Errors);
                    return sr;
                }

                sr.Data = new CycleReviewDto(item.Data);

            }
            catch (Exception ex)
            {
                sr.AddError(ex);
                await _noty.NotifyAsync(
                    NotificationAction.Error,
                    new { error = ex.Message });
            }
            return sr;
        }
        public async Task<ServiceResponse> SaveAsync(Guid id, CycleReviewDto data)
        {
            var sr = new ServiceResponse();
            try
            {
                var itemsDB = await _readPlanningCycle.GetByIdAsync(data.PlanningCycleId.Value);
                if (!itemsDB.Status)
                {
                    sr.AddErrors(itemsDB.Errors);
                    return sr;
                }
                var planningCycle= itemsDB.Data;

                if (id == Guid.Empty)
                {
                    var item = data.ToEntity();
                    item.Status = data.Status;
                    item.UserName = _identityUser.UserEmail;
                    await _write.AddAsync(_identityUser.UserEmail, item);

                    await _noty.NotifyAsync(
                      NotificationAction.Updated, new { type = "Revisor de Ciclo", data = planningCycle.Name });
                }
                else
                {
                    var item = await _read.GetByIdAsync(id);
                    if (!item.Status)
                    {
                        sr.AddErrors(item.Errors);
                        return sr;
                    }
                    item.Data.Status = data.Status;
                    item.Data.Comments = data.Comments;
                    item.Data.InternalNotes = data.InternalNotes;
                    item.Data.ReviewedAt = data.ReviewedAt;
                    item.Data.PlanningCycleId = data.PlanningCycleId;
                    item.Data.UserName = _identityUser.UserEmail;
                    item.Data.IsActive = data.IsActive;
                    item.Data.UpdatedAt = DateTime.UtcNow;
                    item.Data.IsDeleted = data.IsDeleted;
                    await _write.UpdateAsync(_identityUser.UserEmail, item.Data);
                }
            }
            catch (Exception ex)
            {
                sr.AddError(ex);
                await _noty.NotifyAsync(
                    NotificationAction.Error,
                    new { error = ex.Message });
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
                await _noty.NotifyAsync(
                    NotificationAction.Error,
                    new { error = ex.Message });
        }
            return sr;
        }
    }
}