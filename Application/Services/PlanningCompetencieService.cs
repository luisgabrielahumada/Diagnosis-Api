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
    public interface IPlanningCompetencieService
    {
        Task<ServiceResponse<List<PlanningItemDto>>> GetAllAsync(Guid id);
        Task<ServiceResponse<PlanningCompetenceDto>> GetByIdAsync(Guid id);
        Task<ServiceResponse> SelectAsync(Guid id, PlanningItemDto data);
        Task<ServiceResponse> UnSelectAsync(Guid id);
    }

    public class PlanningCompetencieService : IPlanningCompetencieService
    {
        private readonly IReadRepository<Competence> _readCompetencies;
        private readonly IReadRepository<MainCurriculumFramework> _readMainCurriculumFramework;
        private readonly IReadRepository<Planning> _readPlanning;
        private readonly IReadRepository<PlanningCompetence> _read;
        private readonly IWriteRepository<PlanningCompetence> _write;
        private readonly IIdentityUserService _identityUser;
        private readonly ILoggingService _log;
        public PlanningCompetencieService(IReadRepository<PlanningCompetence> read,
            IWriteRepository<PlanningCompetence> write, IIdentityUserService identityUser,
            IReadRepository<Competence> readCompetencies, IReadRepository<Planning> readPlanning,
            IReadRepository<MainCurriculumFramework> readMainCurriculumFramework)
        {
            _read = read;
            _write = write;
            _identityUser = identityUser;
            _readCompetencies = readCompetencies;
            _readPlanning = readPlanning;
            _readMainCurriculumFramework = readMainCurriculumFramework;
        }

        public async Task<ServiceResponse<PlanningCompetenceDto>> GetByIdAsync(Guid id)
        {
            var sr = new ServiceResponse<PlanningCompetenceDto>();
            try
            {

                var item = await _read
                                    .GetByIdAsync(id);
                if (!item.Status)
                {
                    sr.AddErrors(item.Errors);
                    return sr;
                }

                sr.Data = new PlanningCompetenceDto(item.Data);

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
                // 1) Parámetros y carga de la planificación + transacciones
                var parameters = new PagerParameters(1, int.MaxValue)
                {
                    SortDirection = "ASC",
                    SortField = "CreatedAt"
                };
                var includes = new[] { nameof(Planning.PlanningCompetencies) };
                var planRes = await _readPlanning.GetByIdAsync(id, includes);
                if (!planRes.Status)
                {
                    sr.AddErrors(planRes.Errors);
                    return sr;
                }
                var plan = planRes.Data;
                var txItems = plan.PlanningCompetencies
                                         .Where(x => (x.IsActive ?? false) && !(x.IsDeleted ?? false))
                                         .ToList();


                // 2) Cargar catálogo maestro de competencias
                Expression<Func<Competence, bool>> filter = c =>
                       c.AcademicPeriodId == plan.AcademicPeriodId
                    && c.AcademicAreaId == plan.AcademicAreaId
                    && c.GradeId == plan.GradeId
                    && c.LanguageId == plan.LanguageId
                    && c.SubjectId == plan.SubjectId;

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
                var masterList = new List<Competence>();
                if (currrirulumDB.Data.Any(t => t.Status != CurriculumStatus.Draft.ToString().ToLower()))
                {
                    var masterRes = await _readCompetencies.GetPaginationAsync(
                                       parameters,
                                       includes: null,
                                       filter,
                                       orderBy: q => q.CreatedAt
                                   );
                    if (!masterRes.Status)
                    {
                        sr.AddErrors(masterRes.Errors);
                        return sr;
                    }
                    masterList = masterRes.Data.List.ToList();
                }
                var masterIds = masterList.Select(c => c.Id).ToHashSet();

                // 3) Resultado combinado
                var result = new List<PlanningItemDto>();

                // a) Cada competencia maestra
                foreach (var m in masterList)
                {
                    // ¿existe transacción vinculada?
                    var tx = txItems.FirstOrDefault(t => t.CompetenceId == m.Id);
                    if (tx != null)
                    {
                        // caso “relacionado”:
                        // - Id de la transaccional
                        // - ReferenceId = maestro.Id
                        result.Add(new PlanningItemDto
                        {
                            Id = tx.Id,
                            ReferenceId = m.Id,
                            Name = m.Name,
                            NameEn = m.NameEn,
                            Description = m.Description,
                            DescriptionEn = m.DescriptionEn,
                            IsSelected = true,
                            IsOwner = false,
                            SelectedItemId = m.Id,
                            LastUpdatedAt = tx.UpdatedAt
                        });
                    }
                    else
                    {
                        // caso “sin relación”:
                        // - Id = maestro.Id
                        // - ReferenceId = maestro.Id
                        result.Add(new PlanningItemDto
                        {
                            Id = Guid.NewGuid(),
                            ReferenceId = m.Id,
                            Name = m.Name,
                            NameEn = m.NameEn,
                            Description = m.Description,
                            DescriptionEn = m.DescriptionEn,
                            IsSelected = false,
                            IsOwner = false,
                            SelectedItemId = m.Id,
                            LastUpdatedAt = null
                        });
                    }
                }

                // b) Extras propios (transacciones sin CompetenceId)
                var extras = txItems
                    .Where(t => !t.CompetenceId.HasValue)
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

                if (itemDB.Data is not PlanningCompetence)
                {
                    var item = new PlanningCompetence
                    {
                        CompetenceId = !data.IsOwner.Value ? data.ReferenceId : null,
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
                    itemDB.Data.CompetenceId = !data.IsOwner.Value ? itemDB.Data.CompetenceId : null;
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