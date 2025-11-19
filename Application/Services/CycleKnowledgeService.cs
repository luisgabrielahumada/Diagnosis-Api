using Application.Dto.Common;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Dto;
using Shared.Response;
using System.Linq.Expressions;
namespace Infrastructure.Services
{
    public interface ICycleKnowledgeService
    {
        Task<ServiceResponse<List<PlanningItemDto>>> GetAllAsync(Guid id, Guid cycleObjetiveId);
        Task<ServiceResponse<CycleKnowledgeDto>> GetByIdAsync(Guid id);
        Task<ServiceResponse<List<LookupDto>>> GetCycleObjectiveKnowledgeById(Guid id);
        Task<ServiceResponse> SelectAsync(Guid id, PlanningItemDto data);
        Task<ServiceResponse> UnSelectAsync(Guid id);
    }

    public class CycleKnowledgeService : ICycleKnowledgeService
    {
        private readonly IReadRepository<CycleKnowledge> _read;
        private readonly IWriteRepository<CycleKnowledge> _write;
        private readonly IIdentityUserService _identityUser;
        private readonly ILoggingService _log;
        private readonly IReadRepository<CycleObjective> _readCycleObjective;
        private readonly IReadRepository<PlanningCycle> _readPlanningCycle;
        private readonly IReadRepository<AcademicEssentialKnowledge> _readAcademicEssentialKnowledge;
        private readonly IReadRepository<AcademicObjective> _readAcademicObjective;
        public CycleKnowledgeService(IReadRepository<CycleKnowledge> read, 
            IWriteRepository<CycleKnowledge> write, 
            IIdentityUserService identityUser,
            IReadRepository<AcademicEssentialKnowledge> readAcademicEssentialKnowledge, 
            IReadRepository<CycleObjective> readCycleObjective,
            IReadRepository<PlanningCycle> readPlanningCycle,
            IReadRepository<AcademicObjective> readAcademicObjective
            )
        {
            _read = read;
            _write = write;
            _identityUser = identityUser;
            _readAcademicEssentialKnowledge = readAcademicEssentialKnowledge;
            _readCycleObjective = readCycleObjective;
            _readPlanningCycle = readPlanningCycle;
            _readAcademicObjective = readAcademicObjective;
        }
        public async Task<ServiceResponse<List<PlanningItemDto>>> GetAllAsync(Guid planningCycleId, Guid cycleObjectiveId)
        {
            var sr = new ServiceResponse<List<PlanningItemDto>>();

            try
            {
                // 1) Cargo PlanningCycle + PlanningUnits + CycleObjectives + CycleKnowledges
                var includes = new[]
                {
                    nameof(PlanningCycle.Planning),
                    $"{nameof(PlanningCycle.Planning)}.{nameof(Planning.PlanningUnits)}",
                    nameof(PlanningCycle.CycleObjectives),
                    $"{nameof(PlanningCycle.CycleObjectives)}.{nameof(CycleObjective.CycleKnowledges)}"
                };
                var cycleResp = await _readPlanningCycle.GetByIdAsync(planningCycleId, includes);
                if (!cycleResp.Status)
                {
                    sr.AddErrors(cycleResp.Errors);
                    return sr;
                }
                var cycle = cycleResp.Data!;

                // 2) Encuentro la CycleObjective solicitada
                var co = cycle.CycleObjectives.FirstOrDefault(x => x.Id == cycleObjectiveId);
                if (co == null)
                {
                    //sr.AddError($"CycleObjective {cycleObjectiveId} no encontrada en PlanningCycle {planningCycleId}.");
                    sr.Data= new List<PlanningItemDto>();
                    return sr;
                }

                // 3) Recojo los AcademicUnitId de las PlanningUnits activas
                var unitIds = cycle.Planning.PlanningUnits
                    .Where(u => u.AcademicUnitId.HasValue
                             && (u.IsActive ?? false)
                             && !(u.IsDeleted ?? false))
                    .Select(u => u.AcademicUnitId!.Value)
                    .ToHashSet();

                // 4) Cargo maestros: AcademicObjective + sus AcademicEssentialKnowledges
                //    filtrando sólo por unidades activas
                var masterObjResp = await _readAcademicObjective.GetAllAsync(
                    includes: new[] {
                nameof(AcademicObjective.AcademicUnit),
                nameof(AcademicObjective.AcademicEssentialKnowledges)
                    },
                    predicate: ao => ao.AcademicUnitId.HasValue
                                 && unitIds.Contains(ao.AcademicUnitId.Value)
                );
                if (!masterObjResp.Status)
                {
                    sr.AddErrors(masterObjResp.Errors);
                    return sr;
                }
                var masterObjectives = masterObjResp.Data!;

                // 5) Construyo un lookup de AcademicEssentialKnowledgeId → lista de CycleKnowledge
                var linkedLookup = co.CycleKnowledges
                    .Where(k => (k.IsActive ?? false) && !(k.IsDeleted ?? false))
                    .Where(k => k.AcademicEssentialKnowledgeId.HasValue)
                    .ToLookup(k => k.AcademicEssentialKnowledgeId!.Value);

                var result = new List<PlanningItemDto>();

                // 6) Por cada objetivo maestro, recorro sus conocimientos maestros
                foreach (var ao in masterObjectives)
                {
                    var objectiveId = ao.Id!.Value;
                    foreach (var ke in ao.AcademicEssentialKnowledges ?? new List<AcademicEssentialKnowledge>())
                    {
                        var key = ke.Id!.Value;
                        var linkedCks = linkedLookup[key];

                        if (linkedCks.Any())
                        {
                            // si hay uno o más ciclos enlazados, devuelvo uno por cada uno
                            foreach (var ck in linkedCks)
                            {
                                result.Add(new PlanningItemDto
                                {
                                    Id = ck.Id,
                                    ReferenceId = key,
                                    SelectedItemId = objectiveId,
                                    IsSelected = true,
                                    IsOwner = false,
                                    Name = co.Name,
                                    NameEn = string.Empty,
                                    Description = ck.Description ?? ke.Description,
                                    DescriptionEn = ck.DescriptionEn ?? ke.DescriptionEn,
                                    LastUpdatedAt = ck.UpdatedAt
                                });
                            }
                        }
                        else
                        {
                            // no hay transacción: genero GUID nuevo
                            result.Add(new PlanningItemDto
                            {
                                Id = Guid.NewGuid(),
                                ReferenceId = key,
                                SelectedItemId = objectiveId,
                                IsSelected = false,
                                IsOwner = false,
                                Name = co.Name,
                                NameEn = string.Empty,
                                Description = ke.Description,
                                DescriptionEn = null
                            });
                        }
                    }
                }

                // 7) Extras “propios”: CycleKnowledge sin AcademicEssentialKnowledgeId
                var extras = co.CycleKnowledges
                    .Where(k => !k.AcademicEssentialKnowledgeId.HasValue
                             && (k.IsActive ?? false)
                             && !(k.IsDeleted ?? false))
                    .Select(k => new PlanningItemDto
                    {
                        Id = k.Id,
                        ReferenceId = null,
                        SelectedItemId = co.Id,
                        IsSelected = true,
                        IsOwner = true,
                        Name = co.Name,
                        NameEn = k.NameEn,
                        Description = k.Description,
                        DescriptionEn = k.DescriptionEn,
                        LastUpdatedAt = k.UpdatedAt
                    });

                sr.Data = result.Concat(extras).ToList();
            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }

            return sr;
        }

        public async Task<ServiceResponse<CycleKnowledgeDto>> GetByIdAsync(Guid id)
        {
            var sr = new ServiceResponse<CycleKnowledgeDto>();
            try
            {

                var item = await _read
                                    .GetByIdAsync(id);
                if (!item.Status)
                {
                    sr.AddErrors(item.Errors);
                    return sr;
                }

                sr.Data = new CycleKnowledgeDto(item.Data);

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
                var includes = new[]
                {
                    nameof(CycleKnowledge.CycleObjective)
                };
                var itemDB = await _read.GetByIdAsync(data.Id, includes);
                if (!itemDB.Status)
                {
                    sr.AddErrors(itemDB.Errors);
                    return sr;
                }

                if (itemDB.Data is not CycleKnowledge)
                {
                    var newItem = new CycleKnowledge
                    {
                        AcademicEssentialKnowledgeId = !data.IsOwner.Value ? data.ReferenceId.Value : null,
                        Description = data.Description,
                        DescriptionEn = data.DescriptionEn,
                        IsOwner = data.IsOwner,
                        Name = data.Name,
                        NameEn = data.NameEn,
                        UserId = _identityUser.UserId,
                        CycleObjectiveId = data.SelectedItemId!.Value,
                        Id = data.Id,
                    };

                    await _write.AddAsync(_identityUser.UserEmail, newItem);
                }
                else
                {
                    itemDB.Data.DescriptionEn = data.DescriptionEn;
                    itemDB.Data.Description = data.Description;
                    itemDB.Data.Name = data.Name;
                    itemDB.Data.NameEn = data.NameEn;
                    itemDB.Data.UserId = _identityUser.UserId;
                    itemDB.Data.UpdatedAt = DateTime.UtcNow;
                    itemDB.Data.AcademicEssentialKnowledgeId = !data.IsOwner.Value ? itemDB.Data.AcademicEssentialKnowledgeId : null;
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
        public async Task<ServiceResponse<List<LookupDto>>> GetCycleObjectiveKnowledgeById(Guid id)
        {
            var sr = new ServiceResponse<List<LookupDto>>();
            try
            {
                var includes = new[]
               {
                    nameof(PlanningCycle.CycleObjectives),
                    $"{nameof(PlanningCycle.CycleObjectives)}.{nameof(CycleObjective.PlanningUnit)}"
                };
                var item = await _readPlanningCycle
                                    .GetByIdAsync(id, includes);
                if (!item.Status)
                {
                    sr.AddErrors(item.Errors);
                    return sr;
                }

                sr.Data = item.Data.CycleObjectives.Select(t => new LookupDto
                {
                    Label = t.Description,
                    Description = t.PlanningUnit.Description,
                    Value = t.IsOwner.Value ? 1 : 0,
                    Id = t.Id

                }).ToList() ?? new List<LookupDto>();

            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }
            return sr;
        }
    }
}