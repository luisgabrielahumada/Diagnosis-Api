using Application.Dto.Common;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Dto;
using Shared.Response;
using System.Linq.Expressions;
namespace Infrastructure.Services
{
    public interface ICycleObjectiveService
    {
        Task<ServiceResponse<List<PlanningItemDto>>> GetAllAsync(Guid id);
        Task<ServiceResponse<CycleObjectiveDto>> GetByIdAsync(Guid id);
        Task<ServiceResponse> SelectAsync(Guid id, PlanningItemDto data);
        Task<ServiceResponse> UnSelectAsync(Guid id);

    }

    public class CycleObjectiveService : ICycleObjectiveService
    {
        private readonly IReadRepository<CycleObjective> _read;
        private readonly IWriteRepository<CycleObjective> _write;
        private readonly IIdentityUserService _identityUser;
        private readonly ILoggingService _log;
        private readonly IReadRepository<PlanningCycle> _readPlanningCycle;
        private readonly IReadRepository<AcademicUnit> _readUnit;
        private readonly IReadRepository<AcademicObjective> _readAcademicObjective;

        public CycleObjectiveService(IReadRepository<CycleObjective> read,
            IWriteRepository<CycleObjective> write,
            IIdentityUserService identityUser,
            IReadRepository<PlanningCycle> readPlanningCycle,
            IReadRepository<AcademicObjective> readAcademicObjective,
            IReadRepository<AcademicUnit> readUnit)
        {
            _read = read;
            _write = write;
            _identityUser = identityUser;
            _readPlanningCycle = readPlanningCycle;
            _readAcademicObjective = readAcademicObjective;
            _readUnit = readUnit;

        }
        public async Task<ServiceResponse<List<PlanningItemDto>>> GetAllAsync(Guid cycleId)
        {
            var sr = new ServiceResponse<List<PlanningItemDto>>();

            try
            {
                // 1) Cargo ciclo + PlanningUnits + CycleObjectives
                var includesCycle = new[]
                {
                    nameof(PlanningCycle.Planning),
                    $"{nameof(PlanningCycle.Planning)}.{nameof(Planning.PlanningUnits)}",
                    nameof(PlanningCycle.CycleObjectives)
                };
                var cycleResp = await _readPlanningCycle.GetByIdAsync(cycleId, includesCycle);
                if (!cycleResp.Status)
                {
                    sr.AddErrors(cycleResp.Errors);
                    return sr;
                }
                var cycle = cycleResp.Data!;

                // 2) Diccionario de PlanningUnit.Id → nombre de la unidad (si AcademicUnitId != null)
                var puDict = cycle.Planning.PlanningUnits?
                    .Where(pu => pu.AcademicUnitId.HasValue)
                    .ToDictionary(
                        pu => pu.Id,
                        pu => pu // guardamos el PlanningUnit entero
                    )
                    ?? new Dictionary<Guid, PlanningUnit>();

                // 3) Cargo las AcademicUnit correspondientes + sus AcademicObjective
                var unitIds = puDict.Values.Select(pu => pu.AcademicUnitId!.Value).ToHashSet();
                Expression<Func<AcademicUnit, bool>> filterUnits = u => unitIds.Contains(u.Id.Value);
                var respUnits = await _readUnit.GetAllAsync(
                    new[] { nameof(AcademicUnit.AcademicObjectives) },
                    predicate: filterUnits
                );
                if (!respUnits.Status)
                {
                    sr.AddErrors(respUnits.Errors);
                    return sr;
                }
                var units = respUnits.Data!; // lista de AcademicUnit con .AcademicObjective

                // 4) Diccionario AcademicObjectiveId → CycleObjective (si existe)
                var coDict = cycle.CycleObjectives
                    .Where(co => co.AcademicObjectiveId.HasValue)
                    .ToDictionary(co => co.AcademicObjectiveId!.Value, co => co);

                var result = new List<PlanningItemDto>();

                // 5) Para cada PlanningUnit con AcademicUnitId:
                foreach (var pu in puDict.Values)
                {
                    // encuentro la AcademicUnit
                    var au = units.FirstOrDefault(u => u.Id == pu.AcademicUnitId.Value);
                    if (au == null) continue;

                    // nombre de la unidad
                    var unitName = au.Description;

                    // por cada AcademicObjective de esa unidad
                    foreach (var obj in au.AcademicObjectives ?? Enumerable.Empty<AcademicObjective>())
                    {
                        var objId = obj.Id!.Value;

                        if (coDict.TryGetValue(objId, out var co))
                        {
                            // 5.1) Viene de la relación PlanningUnit + CycleObjective
                            result.Add(new PlanningItemDto
                            {
                                // CASE 2 según tu regla:
                                Id = co.Id,         // ID de CycleObjective
                                ReferenceId = objId,         // AcademicObjectiveId
                                SelectedItemId = pu.Id,         // PlanningUnitId
                                IsSelected = true,
                                IsOwner = false,
                                Name = co.Name,       // nombre transaccional
                                NameEn = co.NameEn,
                                Description = co.Description,
                                DescriptionEn = co.DescriptionEn,
                                LastUpdatedAt = co.UpdatedAt
                            });
                        }
                        else
                        {
                            // 5.2) Viene de AcademicUnit sin CycleObjective
                            result.Add(new PlanningItemDto
                            {
                                // CASE 1 según tu regla:
                                Id = Guid.NewGuid(),   // ¡nuevo!
                                ReferenceId = objId,            // AcademicObjectiveId
                                SelectedItemId = pu.Id,            // PlanningUnitId
                                IsSelected = false,
                                IsOwner = false,
                                Name = unitName,         // nombre de la unidad
                                NameEn = string.Empty,
                                Description = obj.Description,
                                DescriptionEn = obj.DescriptionEn
                            });
                        }
                    }
                }

                // 6) Extras: CycleObjectives que no tienen PlanningUnit asociado (AcademicUnitId null)
                var puObjIds = puDict.Values
                    .SelectMany(pu => (units
                        .FirstOrDefault(u => u.Id == pu.AcademicUnitId!.Value)?
                        .AcademicObjectives ?? Enumerable.Empty<AcademicObjective>()))
                    .Select(o => o.Id!.Value)
                    .ToHashSet();

                var extras = cycle.CycleObjectives
                    .Where(co =>
                        !co.AcademicObjectiveId.HasValue
                        || !puObjIds.Contains(co.AcademicObjectiveId.Value)
                    )
                    .Select(co => new PlanningItemDto
                    {
                        Id = co.Id,
                        ReferenceId = co.AcademicObjectiveId,
                        SelectedItemId = null,
                        IsSelected = true,
                        IsOwner = true,
                        Name = co.Name,
                        NameEn = co.NameEn,
                        Description = co.Description,
                        DescriptionEn = co.DescriptionEn,
                        LastUpdatedAt = co.UpdatedAt
                    });

                sr.Data = result.Concat(extras).ToList();
            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }

            return sr;
        }

        public async Task<ServiceResponse<CycleObjectiveDto>> GetByIdAsync(Guid id)
        {
            var sr = new ServiceResponse<CycleObjectiveDto>();
            try
            {

                var item = await _read
                                    .GetByIdAsync(id);
                if (!item.Status)
                {
                    sr.AddErrors(item.Errors);
                    return sr;
                }

                sr.Data = new CycleObjectiveDto(item.Data);

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

                if (itemDB.Data is not CycleObjective)
                {
                    var item = new CycleObjective
                    {
                        AcademicObjectiveId = !data.IsOwner.Value ? data.ReferenceId.Value : null,
                        PlanningUnitId = data.SelectedItemId.Value,
                        UserId = _identityUser.UserId,
                        PlanningCycleId = id,
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
                    //itemDB.Data.Name = data.Name;
                    //itemDB.Data.NameEn = data.NameEn;
                    itemDB.Data.UserId = _identityUser.UserId;
                    itemDB.Data.UpdatedAt = DateTime.UtcNow;
                    itemDB.Data.AcademicObjectiveId = !data.IsOwner.Value ? itemDB.Data.AcademicObjectiveId : null;
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
                //var includes = new[]
                //{
                //    nameof(CycleObjective.CycleKnowledges)
                //};
                var itemDB = await _read.GetByIdAsync(id);
                if (!itemDB.Status)
                {
                    sr.AddErrors(itemDB.Errors);
                    return sr;
                }
                await _write.DeleteAsync(_identityUser.UserEmail, itemDB.Data);
            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }
            return sr;
        }

    }
}