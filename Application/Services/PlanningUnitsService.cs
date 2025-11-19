using Application.Dto.Common;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Infrastructure.Dto;
using Microsoft.Extensions.FileSystemGlobbing.Internal.PathSegments;
using Shared.Pagination;
using Shared.Response;
using System.Linq.Expressions;
namespace Infrastructure.Services
{
    public interface IPlanningUnitsService
    {
        Task<ServiceResponse<List<PlanningItemDto>>> GetAllAsync(Guid id);
        Task<ServiceResponse<PlanningUnitDto>> GetByIdAsync(Guid id);
        Task<ServiceResponse> SelectAsync(Guid id, PlanningItemDto data);
        Task<ServiceResponse> UnSelectAsync(Guid id);
    }

    public class PlanningUnitsService : IPlanningUnitsService
    {
        private readonly IReadRepository<PlanningUnit> _read;
        private readonly IWriteRepository<PlanningUnit> _write;
        private readonly IIdentityUserService _identityUser;
        private readonly ILoggingService _log;
        private readonly IReadRepository<AcademicUnit> _readUnit;
        private readonly IReadRepository<Planning> _readPlanning;
        IReadRepository<MainCurriculumFramework> _readMainCurriculumFramework;
        public PlanningUnitsService(IReadRepository<PlanningUnit> read,
            IWriteRepository<PlanningUnit> write, IIdentityUserService identityUser,
            IReadRepository<AcademicUnit> readUnit, IReadRepository<Planning> readPlanning,
            IReadRepository<MainCurriculumFramework> readMainCurriculumFramework)
        {
            _read = read;
            _write = write;
            _identityUser = identityUser;
            _readUnit = readUnit;
            _readPlanning = readPlanning;
            _readMainCurriculumFramework = readMainCurriculumFramework;
        }
        public async Task<ServiceResponse<PlanningUnitDto>> GetByIdAsync(Guid id)
        {
            var sr = new ServiceResponse<PlanningUnitDto>();
            try
            {

                var item = await _read
                                    .GetByIdAsync(id);
                if (!item.Status)
                {
                    sr.AddErrors(item.Errors);
                    return sr;
                }

                sr.Data = new PlanningUnitDto(item.Data);

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
                var parameters = new PagerParameters(1, 200)
                {
                    SortDirection = "ASC",
                    SortField = "CreatedAt"
                };

                // 2) Carga la planificación con sus unidades transaccionales
                var includes = new[] { nameof(Planning.PlanningUnits) };
                var planningRes = await _readPlanning.GetByIdAsync(id, includes);
                if (!planningRes.Status)
                {
                    sr.AddErrors(planningRes.Errors);
                    return sr;
                }
                var plan = planningRes.Data;
                // Todas las filas transaccionales activas
                var txUnits = plan.PlanningUnits
                    .Where(u => (u.IsActive ?? false) && !(u.IsDeleted ?? false))
                    .ToList();

                // 3) Carga la lista maestra de unidades académicas
                Expression<Func<AcademicUnit, bool>> filter = u =>
                       u.AcademicPeriodId == plan.AcademicPeriodId
                    && u.AcademicAreaId == plan.AcademicAreaId
                    && u.GradeId == plan.GradeId
                    && u.LanguageId == plan.LanguageId
                    && u.SubjectId == plan.SubjectId;
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
                var masterList = new List<AcademicUnit>();
                if (currrirulumDB.Data.Any(t => t.Status != CurriculumStatus.Draft.ToString().ToLower()))
                {
                    var masterRes = await _readUnit.GetPaginationAsync<object>(
                    parameters,
                    includes: null,
                    filter,
                    orderBy: q => q.CreatedAt);
                    if (!masterRes.Status)
                    {
                        sr.AddErrors(masterRes.Errors);
                        return sr;
                    }
                    masterList = masterRes.Data.List.ToList();
                }

                // 4) Construye el resultado
                var result = new List<PlanningItemDto>();

                // a) Itera sobre cada elemento maestro
                foreach (var m in masterList)
                {
                    // Busca transacción vinculada a este maestro
                    var tx = txUnits.FirstOrDefault(t => t.AcademicUnitId == m.Id);
                    if (tx != null)
                    {
                        // Hay relación => Id = transaccional, ReferenceId = maestro
                        result.Add(new PlanningItemDto
                        {
                            Id = tx.Id,
                            ReferenceId = m.Id.Value,
                            Name = m.Name,
                            NameEn = m.NameEn,
                            Description = m.Description,
                            DescriptionEn = m.DescriptionEn,
                            IsSelected = true,
                            IsOwner = false,
                            SelectedItemId = m.Id.Value
                        });
                    }
                    else
                    {
                        // No hay relación => Id y ReferenceId = maestro
                        result.Add(new PlanningItemDto
                        {
                            Id = Guid.NewGuid(),
                            ReferenceId = m.Id.Value,
                            Name = m.Name,
                            NameEn = m.NameEn,
                            Description = m.Description,
                            DescriptionEn = m.DescriptionEn,
                            IsSelected = false,
                            IsOwner = false,
                            SelectedItemId = m.Id.Value
                        });
                    }
                }

                // b) Extras: transacciones sin AcademicUnitId
                var extras = txUnits
                    .Where(t => !t.AcademicUnitId.HasValue)
                    .Select(t => new PlanningItemDto
                    {
                        Id = t.Id,
                        ReferenceId = null,
                        Name = t.Name,
                        NameEn = t.NameEn,
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

                if (itemDB.Data is not PlanningUnit)
                {
                    var item = new PlanningUnit
                    {
                        AcademicUnitId = !data.IsOwner.Value ? data.ReferenceId : null,
                        UserId = _identityUser.UserId,
                        PlanningId = id,
                        Description = data.Description,
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
                    itemDB.Data.Description = data.Description;
                    itemDB.Data.Name = data.Name;
                    itemDB.Data.NameEn = data.NameEn;
                    itemDB.Data.UserId = _identityUser.UserId;
                    itemDB.Data.UpdatedAt = DateTime.UtcNow;
                    itemDB.Data.IsOwner = data.IsOwner.Value;
                    itemDB.Data.AcademicUnitId = !data.IsOwner.Value ? itemDB.Data.AcademicUnitId : null;
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

                var planningUnitResp = await _read.GetByIdAsync(id);
                if (!planningUnitResp.Status)
                {
                    sr.AddErrors(planningUnitResp.Errors);
                    return sr;
                }
                var planningUnit = planningUnitResp.Data;

                await _write.DeleteAsync(_identityUser.UserEmail, planningUnit);
            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }

            return sr;
        }

    }
}