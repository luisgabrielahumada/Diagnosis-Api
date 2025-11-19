using Application.Dto.Common;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Dto;
using Shared.Pagination;
using Shared.Response;
using System.Linq.Expressions;
namespace Infrastructure.Services
{
    public interface IPlanningMethodologyService
    {
        Task<ServiceResponse<List<PlanningItemDto>>> GetAllAsync(Guid id);
        Task<ServiceResponse<PlanningMethodologyDto>> GetByIdAsync(Guid id);
        Task<ServiceResponse> SelectAsync(Guid id, PlanningItemDto data);
        Task<ServiceResponse> UnSelectAsync(Guid id);
    }

    public class PlanningMethodologyService : IPlanningMethodologyService
    {
        private readonly IReadRepository<PlanningMethodology> _read;
        private readonly IWriteRepository<PlanningMethodology> _write;
        private readonly IIdentityUserService _identityUser;
        private readonly ILoggingService _log;
        private readonly IReadRepository<AcademicMethodology> _readMethodologies;
        private readonly IReadRepository<Planning> _readPlanning;
        public PlanningMethodologyService(IReadRepository<PlanningMethodology> read, IWriteRepository<PlanningMethodology> write, IIdentityUserService identityUser, IReadRepository<AcademicMethodology> readPerformance, IReadRepository<Planning> readPlanning)
        {
            _read = read;
            _write = write;
            _identityUser = identityUser;
            _readMethodologies = readPerformance;
            _readPlanning = readPlanning;
        }
        public async Task<ServiceResponse<PlanningMethodologyDto>> GetByIdAsync(Guid id)
        {
            var sr = new ServiceResponse<PlanningMethodologyDto>();
            try
            {

                var item = await _read
                                    .GetByIdAsync(id);
                if (!item.Status)
                {
                    sr.AddErrors(item.Errors);
                    return sr;
                }

                sr.Data = new PlanningMethodologyDto(item.Data);

            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }
            return sr;
        }
        public async Task<ServiceResponse<List<PlanningItemDto>>> GetAllAsync(Guid planId)
        {
            var sr = new ServiceResponse<List<PlanningItemDto>>();

            try
            {
                // 1) Obtener la planeación con sus metodologías transaccionales
                var planResp = await _readPlanning.GetByIdAsync(
                    planId,
                    new[] { nameof(Planning.PlanningMethodologies) }
                );
                if (!planResp.Status)
                {
                    sr.AddErrors(planResp.Errors);
                    return sr;
                }
                var plan = planResp.Data;
                var txItems = plan.PlanningMethodologies
                    .Where(x => (x.IsActive ?? false) && !(x.IsDeleted ?? false))
                    .ToList();

                // 2) Cargar el catálogo maestro filtrado
                var masterResp = await _readMethodologies.GetPaginationAsync(
                    new PagerParameters(1, int.MaxValue) { SortField = "CreatedAt", SortDirection = "ASC" },
                    includes: null,
                    m => m.LanguageId == plan.LanguageId,
                    orderBy: q => q.CreatedAt
                );
                if (!masterResp.Status)
                {
                    sr.AddErrors(masterResp.Errors);
                    return sr;
                }
                var masterList = masterResp.Data.List;

                // 3) Construir el resultado:
                var result = new List<PlanningItemDto>();

                // 3a) Para cada maestro
                foreach (var m in masterList)
                {
                    // buscar transacción vinculada
                    var tx = txItems.FirstOrDefault(t => t.AcademicMethodologyId == m.Id);
                    if (tx != null)
                    {
                        // existe relación
                        result.Add(new PlanningItemDto
                        {
                            // Id del transaccional
                            Id = tx.Id,
                            // referencia al maestro
                            ReferenceId = m.Id.Value,
                            Description = m.Description,
                            DescriptionEn = m.DescriptionEn,
                            IsSelected = true,
                            IsOwner = false,
                            SelectedItemId = m.Id.Value
                        });
                    }
                    else
                    {
                        // no existe relación
                        result.Add(new PlanningItemDto
                        {
                            // Id y ReferenceId = maestro
                            Id = Guid.NewGuid(),
                            ReferenceId = m.Id.Value,
                            Description = m.Description,
                            DescriptionEn = m.DescriptionEn,
                            IsSelected = false,
                            IsOwner = false,
                            SelectedItemId = m.Id.Value
                        });
                    }
                }

                // 3b) Opcional: si hay txItems **sin** AcademicMethodologyId (propios extras)
                var extras = txItems
                    .Where(t => !t.AcademicMethodologyId.HasValue)
                    .Select(t => new PlanningItemDto
                    {
                        // Propios fuera de maestros
                        Id = t.Id,
                        ReferenceId = null,
                        Description = t.Description,
                        DescriptionEn = t.DescriptionEn,
                        IsSelected = true,
                        IsOwner = true,
                        SelectedItemId = null,
                        LastUpdatedAt = t.UpdatedAt,
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

                if (itemDB.Data is not PlanningMethodology)
                {
                    var item = new PlanningMethodology
                    {
                        AcademicMethodologyId = !data.IsOwner.Value ? data.ReferenceId : null,
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
                    itemDB.Data.AcademicMethodologyId = !data.IsOwner.Value ? itemDB.Data.AcademicMethodologyId : null;

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