using Application.Dto.Common;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Infrastructure.Dto;
using Shared.Pagination;
using Shared.Response;
using System.Linq.Expressions;
namespace Infrastructure.Services
{
    public interface IPlanningPerformanceService
    {
        Task<ServiceResponse<List<PlanningItemDto>>> GetAllAsync(Guid id);
        Task<ServiceResponse<PlanningPerformanceDto>> GetByIdAsync(Guid id);
        Task<ServiceResponse> SelectAsync(Guid id, PlanningItemDto data);
        Task<ServiceResponse> UnSelectAsync(Guid id);
    }

    public class PlanningPerformanceService : IPlanningPerformanceService
    {
        private readonly IReadRepository<PlanningPerformance> _read;
        private readonly IWriteRepository<PlanningPerformance> _write;
        private readonly IIdentityUserService _identityUser;
        private readonly ILoggingService _log;
        private readonly IReadRepository<AcademicPerformance> _readPerformance;
        private readonly IReadRepository<Planning> _readPlanning;
        private readonly IReadRepository<MainCurriculumFramework> _readMainCurriculumFramework;

        public PlanningPerformanceService(IReadRepository<PlanningPerformance> read,
            IWriteRepository<PlanningPerformance> write, IIdentityUserService identityUser,
            IReadRepository<AcademicPerformance> readPerformance,
            IReadRepository<Planning> readPlanning, IReadRepository<MainCurriculumFramework> readMainCurriculumFramework)
        {
            _read = read;
            _write = write;
            _identityUser = identityUser;
            _readPerformance = readPerformance;
            _readPlanning = readPlanning;
            _readMainCurriculumFramework = readMainCurriculumFramework;
        }
        public async Task<ServiceResponse<PlanningPerformanceDto>> GetByIdAsync(Guid id)
        {
            var sr = new ServiceResponse<PlanningPerformanceDto>();
            try
            {

                var item = await _read
                                    .GetByIdAsync(id);
                if (!item.Status)
                {
                    sr.AddErrors(item.Errors);
                    return sr;
                }

                sr.Data = new PlanningPerformanceDto(item.Data);

            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }
            return sr;
        }
        public async Task<ServiceResponse<List<PlanningItemDto>>> GetAllAsync(Guid id)
        {
            var sr = new ServiceResponse<List<PlanningItemDto>>();

            try
            {
                // 1) Parámetros de paginación
                var parameters = new PagerParameters(1, int.MaxValue)
                {
                    SortDirection = "ASC",
                    SortField = "CreatedAt"
                };

                // 2) Carga la planificación con sus transacciones
                var includes = new[] { nameof(Planning.PlanningPerformances) };
                var planResp = await _readPlanning.GetByIdAsync(id, includes);
                if (!planResp.Status)
                {
                    sr.AddErrors(planResp.Errors);
                    return sr;
                }
                var plan = planResp.Data;
                var txItems = plan.PlanningPerformances
                                        .Where(x => (x.IsActive ?? false) && !(x.IsDeleted ?? false))
                                        .ToList();


                // 3) Carga la lista maestra de performances
                Expression<Func<AcademicPerformance, bool>> perfFilter = x =>
                       x.AcademicPeriodId == plan.AcademicPeriodId
                    && x.AcademicAreaId == plan.AcademicAreaId
                    && x.GradeId == plan.GradeId
                    && x.LanguageId == plan.LanguageId
                    && x.SubjectId == plan.SubjectId;
                var currrirulumDB = await _readMainCurriculumFramework.GetAllAsync(null, c =>
                       c.AcademicPeriodId == plan.AcademicPeriodId
                    && c.AcademicAreaId == plan.AcademicAreaId
                    && c.GradeId == plan.GradeId
                    && c.LanguageId == plan.LanguageId
                    && c.SubjectId == plan.SubjectId);
                if (!currrirulumDB.Status)
                {
                    sr.AddErrors(currrirulumDB.Errors);
                    return sr;
                }
                var masterList = new List<AcademicPerformance>();
                if (currrirulumDB.Data.Any(t => t.Status != CurriculumStatus.Draft.ToString().ToLower()))
                {
                    var masterResp = await _readPerformance.GetPaginationAsync(
                    parameters,
                    includes: null,
                    perfFilter,
                    orderBy: q => q.CreatedAt
                );
                    if (!masterResp.Status)
                    {
                        sr.AddErrors(masterResp.Errors);
                        return sr;
                    }
                    masterList = masterResp.Data.List.ToList();
                }
                var masterIds = masterList.Select(m => m.Id.Value).ToHashSet();

                // 4) Construye el listado combinado
                var result = new List<PlanningItemDto>();

                // a) Items de la tabla maestra
                foreach (var m in masterList)
                {
                    var tx = txItems.FirstOrDefault(t => t.AcademicPerformanceId == m.Id);
                    if (tx != null)
                    {
                        // vínculo existente
                        result.Add(new PlanningItemDto
                        {
                            Id = tx.Id,                // Id de la transacción
                            ReferenceId = m.Id.Value,           // referencia al maestro
                            Description = m.Description,
                            DescriptionEn = m.DescriptionEn,
                            IsSelected = true,
                            IsOwner = false,
                            SelectedItemId = m.Id.Value,
                            LastUpdatedAt = tx.UpdatedAt
                        });
                    }
                    else
                    {
                        // sin vínculo
                        result.Add(new PlanningItemDto
                        {
                            Id = Guid.NewGuid(),           // Id del maestro
                            ReferenceId = m.Id.Value,           // referencia al maestro
                            Description = m.Description,
                            DescriptionEn = m.DescriptionEn,
                            IsSelected = false,
                            IsOwner = false,
                            SelectedItemId = m.Id.Value,
                            LastUpdatedAt = null
                        });
                    }
                }

                // b) Extras propios (transacciones sin AcademicPerformanceId)
                var extras = txItems
                    .Where(t => !t.AcademicPerformanceId.HasValue)
                    .Select(t => new PlanningItemDto
                    {
                        Id = t.Id,
                        ReferenceId = null,
                        Description = t.Description,
                        DescriptionEn = t.DescriptionEn,
                        IsSelected = true,
                        IsOwner = true,
                        SelectedItemId = null,
                        LastUpdatedAt = t.UpdatedAt
                    });

                sr.Data = result.Concat(extras).ToList();
            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }

            return sr;
        }
        public async Task<ServiceResponse> SelectAsync(Guid id, PlanningItemDto data)
        {
            var sr = new ServiceResponse();
            try
            {
                var itemDB = await _read.GetByIdAsync(data.Id);
                if (!itemDB.Status)
                {
                    sr.AddErrors(itemDB.Errors);
                    return sr;
                }

                if (itemDB.Data is not PlanningPerformance)
                {
                    var item = new PlanningPerformance
                    {
                        AcademicPerformanceId = !data.IsOwner.Value ? data.ReferenceId : null,
                        UserId = _identityUser.UserId,
                        PlanningId = id,
                        Description = data.Description.Trim(),
                        DescriptionEn = data.DescriptionEn,
                        IsOwner = data.IsOwner,
                        Name = data.Name,
                        NameEn = data.NameEn,
                        Id = data.Id
                    };
                    await _write.AddAsync(_identityUser.UserEmail, item);
                }
                else
                {
                    itemDB.Data.DescriptionEn = data.DescriptionEn;
                    itemDB.Data.Description = data.Description.Trim();
                    itemDB.Data.Name = data.Name;
                    itemDB.Data.NameEn = data.NameEn;
                    itemDB.Data.UserId = _identityUser.UserId;
                    itemDB.Data.UpdatedAt = DateTime.UtcNow;
                    itemDB.Data.IsOwner = data.IsOwner.Value;
                    itemDB.Data.AcademicPerformanceId = !data.IsOwner.Value ? itemDB.Data.AcademicPerformanceId : null;
                    await _write.UpdateAsync(_identityUser.UserEmail, itemDB.Data);
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

                var item = await _read.GetByIdAsync(id);
                if (!item.Status)
                {
                    sr.AddErrors(item.Errors);
                    return sr;
                }

                await _write.DeleteAsync(_identityUser.UserEmail, item.Data);
            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }
            return sr;
        }

    }
}