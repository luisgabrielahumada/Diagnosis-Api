using Application.Dto.Common;
using Application.Services;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Dto;
using Infrastructure.Dto.Filters;
using Shared.Enums;
using Shared.Pagination;
using Shared.Response;
using System.Linq.Expressions;
namespace Infrastructure.Services
{
    public interface IPlanningCycleService
    {
        Task<ServiceResponse<PaginatedList<PlanningCycleDto>>> GetAllAsync(Guid id, int pageIndex, int pageSize, PlanningCycleInfoDto filter);
        Task<ServiceResponse<PlanningCycleDto>> GetByIdAsync(Guid id);
        Task<ServiceResponse> SaveAsync(Guid id, PlanningCycleDto data);
        Task<ServiceResponse> DeleteAsync(Guid id);
        Task<ServiceResponse<List<LookupDto>>> GetPlanningUnitsByPlanningCycle(Guid id);
        Task<ServiceResponse<List<PlanningItemDto>>> GetAllPlanningPerformancesByPlanningCycle(Guid id, Guid? cycleId);
        Task<ServiceResponse> SelectAsync(Guid id, PlanningItemDto data);
        Task<ServiceResponse> UnSelectAsync(Guid id);
    }

    public class PlanningCycleService : IPlanningCycleService
    {
        private readonly IReadRepository<PlanningCycle> _read;
        private readonly IWriteRepository<PlanningCycle> _write;
        private readonly IIdentityUserService _identityUser;
        private readonly ILoggingService _log;
        private readonly IReadRepository<Planning> _readPlanning;
        private readonly IReadRepository<PlanningCyclePerformance> _readPlanningCyclePerformance;
        private readonly IWriteRepository<PlanningCyclePerformance> _writePlanningCyclePerformance;
        private readonly IAppNotifierService _noty;
        public PlanningCycleService(IReadRepository<PlanningCycle> read,
            IWriteRepository<PlanningCycle> write, IIdentityUserService identityUser,
            IReadRepository<Planning> readPlanning, IAppNotifierService noty,
            IWriteRepository<PlanningCyclePerformance> writePlanningCyclePerformance,
            IReadRepository<PlanningCyclePerformance> readPlanningCyclePerformance
             )
        {
            _read = read;
            _write = write;
            _identityUser = identityUser;
            _readPlanning = readPlanning;
            _noty = noty;
            _writePlanningCyclePerformance = writePlanningCyclePerformance;
            _readPlanningCyclePerformance = readPlanningCyclePerformance;
        }
        public async Task<ServiceResponse<PaginatedList<PlanningCycleDto>>> GetAllAsync(Guid id, int pageIndex, int pageSize, PlanningCycleInfoDto filter)
        {
            var sr = new ServiceResponse<PaginatedList<PlanningCycleDto>>();
            try
            {
                var parameters = new PagerParameters(pageIndex, pageSize);
                parameters.SortDirection = "ASC";
                parameters.SortField = "Order";
                var includes = new[]
                {
                    //nameof(PlanningPerformance),
                    nameof(PlanningCycle.CyclePerformances),
                    $"{nameof(PlanningCycle.CyclePerformances)}.{nameof(PlanningCyclePerformance.PlanningPerformance)}"

                };

                Expression<Func<PlanningCycle, bool>> predicate = x => (x.PlanningId == id && x.IsDeleted.Value == filter.IsDeleted);

                var itemsDB = await _read
                                    .GetPaginationAsync<object>(parameters, includes, predicate, orderBy: q => q.Order);
                if (!itemsDB.Status)
                {
                    return new ServiceResponse<PaginatedList<PlanningCycleDto>>
                    { Errors = itemsDB.Errors };
                }

                var data = new PaginatedList<PlanningCycleDto>
                {
                    Count = itemsDB.Data.Count,
                    List = itemsDB.Data.List
                        .Select(x => new PlanningCycleDto(x))
                        .ToList()
                };

                sr.Data = data;

            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }
            return sr;
        }
        public async Task<ServiceResponse<PlanningCycleDto>> GetByIdAsync(Guid id)
        {
            var sr = new ServiceResponse<PlanningCycleDto>();
            try
            {
                var includes = new[]
                {
                    nameof(PlanningPerformance),
                    nameof(PlanningCyclePerformance),
                };
                var item = await _read
                                    .GetByIdAsync(id, includes);
                if (!item.Status)
                {
                    sr.AddErrors(item.Errors);
                    return sr;
                }

                sr.Data = new PlanningCycleDto(item.Data);

            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }
            return sr;
        }
        public async Task<ServiceResponse> SaveAsync(Guid id, PlanningCycleDto data)
        {
            var sr = new ServiceResponse();
            try
            {

                if (id == Guid.Empty)
                {
                    var count = await _read.GetAllAsync(null, predicate: x => x.PlanningId == data.PlanningId && x.IsActive.Value && !x.IsDeleted.Value);
                    var info = data.ToEntity();
                    info.Order = count.Data.Count + 1;
                    info.Name = $"Ciclo / Semana {count.Data.Count + 1}";
                    await _write.AddAsync(_identityUser.UserEmail, info);
                }
                else
                {
                    var item = await _read.GetByIdAsync(id);
                    if (!item.Status)
                    {
                        sr.AddErrors(item.Errors);
                        return sr;
                    }
                    //item.Data.PlanningId = data.PlanningId;
                    item.Data.StartingDate = data.StartingDate;
                    item.Data.FinalDate = data.FinalDate;
                    item.Data.Session = data.Session;
                    item.Data.Activity = data.Activity;
                    item.Data.ActivityEn = data.ActivityEn;
                    item.Data.Code = data.Code;
                    item.Data.Resources = data.Resources;
                    item.Data.ResourcesEn = data.ResourcesEn;
                    item.Data.Observations = data.Observations;
                    item.Data.ObservationsEn = data.ObservationsEn;
                    item.Data.UserId = data.UserId;
                    item.Data.IsActive = data.IsActive;
                    item.Data.UpdatedAt = DateTime.UtcNow;
                    item.Data.IsDeleted = data.IsDeleted;
                    item.Data.IsApproved = data.IsApproved;
                    // item.Data.PlanningPerformanceId = data.PlanningPerformanceId;
                    item.Data.LinkingQuestions = data.LinkingQuestions;
                    item.Data.Name = data.Name;
                    if (item.Data.CyclePerformances is null)
                    {
                        item.Data.CyclePerformances = data.CyclePerformances?.Select(t => new PlanningCyclePerformance
                        {
                            Id = Guid.NewGuid(),
                            PlanningCycleId = id,
                            PlanningPerformanceId = t.Id.Value,
                            Sequence = +1
                        }).ToList();
                    }
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
        public async Task<ServiceResponse<List<LookupDto>>> GetPlanningUnitsByPlanningCycle(Guid id)
        {
            var sr = new ServiceResponse<List<LookupDto>>();
            try
            {
                var includes = new[]
                {
                    nameof(Planning.PlanningUnits)
                };
                var itemDB = await _readPlanning.GetByIdAsync(id, includes: includes);

                if (!itemDB.Status)
                {
                    sr.AddErrors(itemDB.Errors);
                    return sr;
                }

                sr.Data = itemDB.Data.PlanningUnits?.Select(t => new LookupDto
                {
                    Label = t.Description,
                    Value = 0,
                    Id = t.Id

                }).ToList() ?? new List<LookupDto>();

            }
            catch (Exception ex)
            {
                sr.AddError(ex);
                await _noty.NotifyAsync(
                     NotificationAction.Error, new { error = ex.Message });
            }
            return sr;
        }
        public async Task<ServiceResponse<List<PlanningItemDto>>> GetAllPlanningPerformancesByPlanningCycle(Guid id, Guid? cycleId)
        {
            var sr = new ServiceResponse<List<PlanningItemDto>>();

            try
            {
                var parameters = new PagerParameters(1, int.MaxValue)
                {
                    SortDirection = "ASC",
                    SortField = "CreatedAt"
                };

                var includes = new[] {
                    nameof(Planning.PlanningPerformances),
                    nameof(Planning.PlanningCycles),
                    $"{nameof(Planning.PlanningCycles)}.{nameof(PlanningCycle.CyclePerformances)}"
                };
                var planResp = await _readPlanning.GetByIdAsync(id, includes);
                if (!planResp.Status)
                {
                    sr.AddErrors(planResp.Errors);
                    return sr;
                }
                var items = planResp
                        .Data
                        .PlanningPerformances
                        .Select(t => new PlanningItemDto
                        {
                            Id = Guid.NewGuid(),
                            ReferenceId = t.Id,
                            Description = t.Description,
                            DescriptionEn = t.DescriptionEn,
                            IsSelected = false,
                            IsOwner = true,
                            SelectedItemId = t.Id,
                            LastUpdatedAt = t.UpdatedAt
                        }).ToList();
                if (cycleId != Guid.Empty)
                {
                    var cycle = planResp.Data.PlanningCycles
                        .FirstOrDefault(t => t.Id == cycleId);
                    if (cycle is PlanningCycle)
                    {
                        items = items
                                    .Select(t =>
                                    {
                                        t.IsSelected = cycle?.CyclePerformances.Any(m => m.PlanningPerformanceId == t.SelectedItemId) ?? false;
                                        t.Id = cycle?.CyclePerformances.FirstOrDefault(m => m.PlanningPerformanceId == t.SelectedItemId)?.Id ?? Guid.NewGuid();
                                        return t;
                                    })
                                    .ToList();
                    }
                }
                sr.Data = items;
            }
            catch (Exception ex)
            {
                sr.AddError(ex);
                await _noty.NotifyAsync(
                     NotificationAction.Error, new { error = ex.Message });
            }

            return sr;
        }
        public async Task<ServiceResponse> SelectAsync(Guid id, PlanningItemDto data)
        {
            var sr = new ServiceResponse();
            try
            {
                var itemDB = await _readPlanningCyclePerformance.GetByIdAsync(data.Id);
                if (!itemDB.Status)
                {
                    sr.AddErrors(itemDB.Errors);
                    return sr;
                }

                if (itemDB.Data is not PlanningCyclePerformance)
                {
                    var item = new PlanningCyclePerformance
                    {
                        Id = data.Id,
                        PlanningCycleId = id,
                        PlanningPerformanceId = data.ReferenceId.Value,
                        Sequence = +1,
                        CreatedBy = _identityUser.UserEmail,
                    };
                    await _writePlanningCyclePerformance.AddAsync(_identityUser.UserEmail, item);
                }
            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }
            return sr;
        }
        public async Task<ServiceResponse> UnSelectAsync(Guid id)
        {
            var sr = new ServiceResponse();
            try
            {

                var resp = await _readPlanningCyclePerformance.GetByIdAsync(id);
                if (!resp.Status)
                {
                    sr.AddErrors(resp.Errors);
                    return sr;
                }
                var planningCyclePerformance = resp.Data;

                await _writePlanningCyclePerformance.DeleteAsync(_identityUser.UserEmail, planningCyclePerformance);
            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }

            return sr;
        }
    }
}