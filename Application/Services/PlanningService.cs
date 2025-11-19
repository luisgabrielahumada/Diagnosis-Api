using Application.Dto;
using Application.Factory.Interface;
using Application.Services;
using DocumentFormat.OpenXml.Spreadsheet;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Infrastructure.Dto;
using Infrastructure.Dto.Filters;
using Shared.Enums;
using Shared.Extensions;
using Shared.MapperModel;
using Shared.Pagination;
using Shared.Response;
using System.Linq.Expressions;

namespace Infrastructure.Services
{
    public interface IPlanningService
    {
        Task<ServiceResponse<PlanningInboxDto>> GetAllAsync(int pageIndex, int pageSize, PlanningInfoDto filter, bool onlyCount = false);
        Task<ServiceResponse<PlanningDto>> GetByIdAsync(Guid id);
        Task<ServiceResponse<Guid>> CreateAsync(PlanningDto data);
        Task<ServiceResponse> UpdateAsync(Guid id, PlanningDto data);
        Task<ServiceResponse> DeleteAsync(Guid id);
        Task<ServiceResponse> CloneAsync(Guid id, PlanningCloneLogDto data, bool isAsync = false, Guid? userId = null, string userEmail = null);

        public class PlanningService : IPlanningService
        {
            private readonly IReadRepository<Subject> _readSubject;
            private readonly IReadRepository<AcademicPeriod> _readPeriod;
            private readonly IReadRepository<Planning> _read;
            private readonly IWriteRepository<Planning> _write;
            private readonly IIdentityUserService _identityUser;
            private readonly IUserService _userService;
            private readonly IAppNotifierService _noty;
            private readonly IPlanningRepository _planning;
            private readonly IJobScheduler _scheduler;
            public PlanningService(
                IReadRepository<Planning> read,
                IWriteRepository<Planning> write,
                IIdentityUserService identityUser,
                IReadRepository<Subject> readSubject,
                IReadRepository<AcademicPeriod> readPeriod,
                IUserService userService,
                IAppNotifierService notificationService,
                IPlanningRepository planning,
                IJobScheduler scheduler)
            {
                _read = read;
                _write = write;
                _identityUser = identityUser;
                _readSubject = readSubject;
                _readPeriod = readPeriod;
                _userService = userService;
                _noty = notificationService;
                _planning = planning;
                _scheduler = scheduler;
            }

            public async Task<ServiceResponse<PlanningInboxDto>> GetAllAsync(int pageIndex, int pageSize, PlanningInfoDto filter, bool onlyCount = false)
            {
                var sr = new ServiceResponse<PlanningInboxDto>();
                try
                {
                    var data = new PaginatedList<PlanningDto>();
                    var parameters = new PagerParameters(pageIndex, pageSize);
                    parameters.SortDirection = "DESC";
                    parameters.SortField = "CreatedAt";

                    var currentUser = await _userService.GetByIdAsync(_identityUser.UserEmail);
                    if (!currentUser.Status)
                    {
                        sr.AddErrors(currentUser.Errors);
                        return sr;
                    }

                    var myUserId = currentUser.Data.Id;
                    bool isAdminUser = currentUser.Data?.Role?.Code == UserRole.Admin;
                    bool isAdvisorUser = currentUser.Data?.Role?.Code == UserRole.Planning_Advisor;
                    var isDraft = string.Equals(filter.Status, "draft", StringComparison.OrdinalIgnoreCase);
                    var includes = new[]
                    {
                        nameof(Planning.AcademicArea),
                        $"{nameof(Planning.AcademicArea)}.{nameof(AcademicArea.Reviewers)}",
                        $"{nameof(Planning.AcademicArea)}.{nameof(AcademicArea.Reviewers)}.{nameof(CampusAcademicAreaReviewer.Reviewer)}",
                        nameof(Planning.AcademicPeriod),
                        nameof(Planning.Grade),
                        nameof(Planning.Subject),
                        nameof(Planning.Campus),
                        nameof(Planning.Teacher),
                        nameof(Planning.Course),
                        nameof(Planning.Language)
                    };
                    if (!onlyCount)
                    {
                        Expression<Func<Planning, bool>> predicate = x =>
                          (string.IsNullOrEmpty(filter.Status) || x.Status == filter.Status) &&
                          (!filter.IsActive.HasValue || x.IsActive == filter.IsActive.Value) &&
                          (!filter.IsDeleted.HasValue || x.IsDeleted == filter.IsDeleted.Value) &&
                          (!filter.Campus.HasValue || x.CampusId == filter.Campus.Value) &&
                          (!filter.Subject.HasValue || x.SubjectId == filter.Subject.Value) &&
                          (!filter.Grade.HasValue || x.GradeId == filter.Grade.Value) &&
                          (!filter.Period.HasValue || x.AcademicPeriodId == filter.Period.Value) &&
                          (!filter.Area.HasValue || x.AcademicAreaId == filter.Area.Value) &&
                          (!filter.Course.HasValue || x.CourseId == filter.Course.Value) &&
                          (!filter.Language.HasValue || x.LanguageId == filter.Language.Value) &&
                          (!filter.StartDate.HasValue || x.StartingDate.Value.Date >= filter.StartDate.Value.Date) &&
                          (!filter.EndDate.HasValue || x.FinalDate.Value.Date <= filter.EndDate.Value.Date) &&
                          (
                              // Admin: como estaba, con filtro de Teacher si viene
                              (isAdminUser && (!filter.Teacher.HasValue || x.TeacherId == filter.Teacher.Value))
                              ||
                              // Advisor:
                              (isAdvisorUser &&
                                  (
                                      // En draft: SOLO mis planeaciones
                                      (isDraft && x.TeacherId == myUserId)
                                      ||
                                      // En otros estados: mías o de docentes de mi área/campus
                                      (!isDraft && (
                                          x.TeacherId == myUserId ||
                                          x.AcademicArea.Reviewers.Any(ar =>
                                              ar.ReviewerId == myUserId && ar.CampusId == x.CampusId
                                          )
                                      ))
                                  )
                                  // En draft ignoramos filter.Teacher; en otros estados sí lo aplicamos
                                  && (!isDraft ? (!filter.Teacher.HasValue || x.TeacherId == filter.Teacher.Value) : true)
                              )
                              ||
                              // Teacher: solo mis planeaciones
                              (!isAdminUser && !isAdvisorUser && x.TeacherId == myUserId)
                          );
                        Expression<Func<Planning, object>> orderBy = x => x.CreatedAt;


                        var itemsDB = await _read.GetPaginationAsync<object>(
                            parameters: parameters,
                            includes: includes,
                            filter: predicate,
                            orderBy: q => q.CreatedAt
                        );

                        if (!itemsDB.Status)
                        {
                            sr.AddErrors(itemsDB.Errors);
                            return sr;
                        }

                        // Mapear la lista de resultados a DTO
                        data = new PaginatedList<PlanningDto>
                        {
                            Count = itemsDB.Data.Count,
                            List = MappingHelper.MapEntityListToMapperModelList<Planning, PlanningDto>(itemsDB.Data.List)
                        };

                    }
                    var draft = PlanningStatus.Draft.ToString().ToLowerInvariant();
                    // Contamos la cantidad de planeaciones basadas en los filtros
                    Expression<Func<Planning, bool>> predicateCount = x =>
                        // 1) ADMIN: todo
                        isAdminUser

                        ||

                        // 2) ADVISOR:
                        (isAdvisorUser &&
                            (
                                // En draft: SOLO mis registros
                                (x.Status.ToLower() == draft && x.TeacherId == myUserId)
                                ||
                                // En otros estados: míos + docentes de mi área/campus
                                (x.Status.ToLower() != draft && (
                                    x.TeacherId == myUserId ||
                                    x.AcademicArea.Reviewers.Any(ar =>
                                        ar.ReviewerId == myUserId && ar.CampusId == x.CampusId)
                                ))
                            )
                        )

                        ||

                        // 3) TEACHER: solo los suyos
                        (!isAdminUser && !isAdvisorUser && x.TeacherId == myUserId);


                    var plannings = await _read.GetAllAsync(
                        includes: includes,
                        predicate: predicateCount
                    );

                    if (!plannings.Status)
                    {
                        sr.AddErrors(plannings.Errors);
                        return sr;
                    }

                    // Agrupar por estado de las planeaciones
                    var allStatus = Enum.GetNames(typeof(PlanningStatus));
                    var statusWithQuantity = plannings
                        .Data
                        .Where(t => t.IsDeleted.Value == filter.IsDeleted)
                        .GroupBy(p => p.Status.ToLower()) // Normaliza el casing
                        .ToDictionary(g => g.Key, g => g.Count());
                    var status = allStatus.ToDictionary(
                        s => s.ToLower(),
                        s => statusWithQuantity.ContainsKey(s.ToLower()) ? statusWithQuantity[s.ToLower()] : 0
                    );


                    // Retornar los datos empaquetados
                    sr.Data = new PlanningInboxDto
                    {
                        Planning = data,
                        Status = status
                    };
                }
                catch (Exception ex)
                {
                    sr.AddError(ex);
                }

                return sr;
            }

            public async Task<ServiceResponse<PlanningDto>> GetByIdAsync(Guid id)
            {
                var sr = new ServiceResponse<PlanningDto>();
                try
                {
                    var includes = new[]
                    {
                        nameof(Planning.AcademicArea),
                        nameof(Planning.AcademicPeriod),
                        nameof(Planning.Grade),
                        nameof(Planning.Subject),
                        nameof(Planning.Campus),
                        nameof(Planning.Teacher),
                        nameof(Planning.Course),
                        nameof(Planning.Language),
                        nameof(Planning.PlanningCompetencies),
                        nameof(Planning.PlanningMethodologies),
                        nameof(Planning.PlanningPerformances),
                        nameof(Planning.PlanningUnits),
                         nameof(Planning.PlanningCycles),
                        $"{nameof(Planning.PlanningCycles)}.{nameof(PlanningCycle.CyclePerformances)}",
                        $"{nameof(Planning.PlanningCycles)}.{nameof(PlanningCycle.CyclePerformances)}.{nameof(PlanningCyclePerformance.PlanningPerformance)}",
                        $"{nameof(Planning.PlanningCycles)}.{nameof(PlanningCycle.CycleObjectives)}",
                        $"{nameof(Planning.PlanningCycles)}.{nameof(PlanningCycle.CycleObjectives)}.{nameof(CycleObjective.CycleKnowledges)}",
                        //$"{nameof(Planning.PlanningUnits)}.{nameof(PlanningUnit.CycleObjectives)}",
                        //// IMPORTANTE: nietos (se había comentado)
                        //$"{nameof(Planning.PlanningUnits)}.{nameof(PlanningUnit.CycleObjectives)}.{nameof(CycleObjective.CycleKnowledges)}",
                        nameof(Planning.CloneLogsAsSource),
                    };

                    var item = await _read.GetByIdAsync(id, includes);
                    if (!item.Status)
                    {
                        sr.AddErrors(item.Errors);
                        return sr;
                    }

                    // Inicializo el DTO con datos base
                    var resp = new PlanningDto(item.Data);

                    // Mapeo siempre, usando colecciones vacías si es null
                    resp.PlanningCompetencies = MappingHelper.MapEntityListToMapperModelList<PlanningCompetence, PlanningCompetenceDto>(item.Data.PlanningCompetencies ?? new List<PlanningCompetence>());
                    resp.PlanningMethodologies = MappingHelper.MapEntityListToMapperModelList<PlanningMethodology, PlanningMethodologyDto>(item.Data.PlanningMethodologies ?? new List<PlanningMethodology>());
                    resp.PlanningPerformances = MappingHelper.MapEntityListToMapperModelList<PlanningPerformance, PlanningPerformanceDto>(item.Data.PlanningPerformances ?? new List<PlanningPerformance>());
                    resp.PlanningUnits = MappingHelper.MapEntityListToMapperModelList<PlanningUnit, PlanningUnitDto>(item.Data.PlanningUnits ?? new List<PlanningUnit>());
                    resp.PlanningCycles = item.Data.PlanningCycles
                                                ?.Select(q => new PlanningCycleDto(q))
                                                .ToList()
                                              ?? new List<PlanningCycleDto>();

                    //resp.PlanningCycles = resp.PlanningCycles.Select(t =>
                    //{
                    //    var cycle = item.Data.PlanningCycles.Where(m => m.Id == t.Id).FirstOrDefault();
                    //    t.CycleObjectives = unit.CycleObjectives.Select(c => new CycleObjectiveDto(c)
                    //    {
                    //        CycleKnowledges = c.CycleKnowledges.Select(n => new CycleKnowledgeDto(n)).ToList()
                    //    }).ToList();
                    //    return t;
                    //}).ToList();
                    //MappingHelper.MapEntityListToMapperModelList<PlanningCycle, PlanningCycleDto>(item.Data.PlanningCycles ?? new List<PlanningCycle>());
                    //resp.PlanningUnits = resp.PlanningUnits.Select(t =>
                    //{
                    //    var unit = item.Data.PlanningUnits.Where(m => m.Id == t.Id).FirstOrDefault();
                    //    t.CycleObjectives = unit.CycleObjectives.Select(c => new CycleObjectiveDto(c)
                    //    {
                    //        CycleKnowledges = c.CycleKnowledges.Select(n => new CycleKnowledgeDto(n)).ToList()
                    //    }).ToList();
                    //    return t;
                    //}).ToList();
                    sr.Data = resp;
                }
                catch (Exception ex)
                {
                    sr.AddError(ex);
                }

                return sr;
            }

            public async Task<ServiceResponse<Guid>> CreateAsync(PlanningDto data)
            {
                var sr = new ServiceResponse<Guid>();
                try
                {
                    var period = await _readPeriod.GetByIdAsync(data.AcademicPeriodId.Value);
                    var entity = data.ToEntity();
                    entity.Status = "draft";
                    entity.CampusId = data.CampusId;
                    entity.AcademicAreaId = data.AcademicAreaId;
                    entity.SubjectId = data.SubjectId;
                    entity.TeacherId = data.TeacherId;
                    entity.GradeId = data.GradeId;
                    entity.CourseId = data.CourseId;
                    entity.AcademicPeriodId = data.AcademicPeriodId;
                    entity.LanguageId = data.LanguageId;
                    entity.AcademicYear = period.Data.Name.PeriodYear(period.Data.StartDate.Value, period.Data.EndDate.Value);
                    entity.IsActive = true;
                    entity.CreatedAt = DateTime.UtcNow;
                    var result = await _write.AddAsync(_identityUser.UserEmail, entity);
                    sr.Data = result.Id.Value;

                    await _noty.NotifyAsync(
                             NotificationAction.Created, new { data = result.Id, type = "Planeación Academica" });
                }
                catch (Exception ex)
                {
                    sr.AddError(ex);
                    await _noty.NotifyAsync(
                          NotificationAction.Error, new { error = ex.Message });
                }

                return sr;
            }

            public async Task<ServiceResponse> UpdateAsync(Guid id, PlanningDto data)
            {
                var sr = new ServiceResponse();
                try
                {
                    var includes = new[] { nameof(Planning.Teacher) };
                    var itemResp = await _read.GetByIdAsync(id, includes);
                    if (!itemResp.Status)
                    {
                        sr.AddErrors(itemResp.Errors);
                        return sr;
                    }

                    var entity = itemResp.Data;
                    // Solo actualizamos campos editables
                    entity.CourseId = data.CourseId ?? entity.CourseId;
                    entity.TeachingTime = data.TeachingTime ?? entity.TeachingTime;
                    entity.AssessmentTasks = data.AssessmentTasks ?? entity.AssessmentTasks;
                    entity.Performance = data.Performance ?? entity.Performance;
                    entity.LinkingQuestions = data.LinkingQuestions ?? entity.LinkingQuestions;
                    entity.Status = data.Status;
                    entity.UpdatedAt = DateTime.UtcNow;

                    await _write.UpdateAsync(_identityUser.UserEmail, entity);

                    await _noty.NotifyAsync(
                      NotificationAction.Updated, new { data = entity.Teacher.Name, type = "Planeación Academica" });
                }
                catch (Exception ex)
                {
                    sr.AddError(ex);
                    await _noty.NotifyAsync(
                          NotificationAction.Error, new { error = ex.Message });
                }

                return sr;
            }

            public async Task<ServiceResponse> DeleteAsync(Guid id)
            {
                var sr = new ServiceResponse();
                try
                {
                    var itemResp = await _read.GetByIdAsync(id);
                    if (!itemResp.Status)
                    {
                        sr.AddErrors(itemResp.Errors);
                        return sr;
                    }

                    var entity = itemResp.Data;
                    entity.IsDeleted = !entity.IsDeleted;
                    entity.UpdatedAt = DateTime.UtcNow;

                    await _write.UpdateAsync(_identityUser.UserEmail, entity);

                    await _noty.NotifyAsync(
                           entity.IsDeleted.Value ? NotificationAction.Deleted : NotificationAction.Restored,
                           new { data = id, type = "Planeación Academica" });
                }
                catch (Exception ex)
                {
                    sr.AddError(ex);
                    await _noty.NotifyAsync(
                          NotificationAction.Error, new { error = ex.Message });
                }

                return sr;
            }

            public async Task<ServiceResponse> CloneAsync(Guid id, PlanningCloneLogDto data, bool isAsync = false, Guid? userId = null, string userEmail = null)
            {
                var sr = new ServiceResponse();
                try
                {
                    if (!isAsync)
                    {
                        _scheduler.Enqueue<IPlanningService>(job => job.CloneAsync(id, data, true, _identityUser.UserId.Value, _identityUser.UserEmail));
                        await _noty.NotifyAsync(NotificationAction.Async, new { data = "La planeación automatica solicitada ha inicado cuando este lista sera notificado" });
                        return sr;
                    }

                    var period = await _readPeriod.GetByIdAsync(data.AcademicPeriodId);
                    var academicYear = period.Data.Name.PeriodYear(period.Data.StartDate.Value, period.Data.EndDate.Value);
                    var resp = await _planning.CloneAsync(id, data.GradeIds, data.CourseIds, data.AcademicPeriodId, academicYear, userId.Value, userEmail);
                    if (!resp.Status)
                    {
                        sr.AddErrors(resp.Errors);
                        await _noty.NotifyAsync(NotificationAction.Error, new { error = resp.Errors.FirstOrDefault().ErrorMessage, Role = UserRole.Teacher });
                        return sr;
                    }

                    await _noty.NotifyAsync(
                           NotificationAction.Created, new { data = "Nueva Planeación", type = "Planeación Academica" });
                    sr.Data = id.ToString();
                }
                catch (Exception ex)
                {
                    sr.AddError(ex);
                    await _noty.NotifyAsync(
                          NotificationAction.Error, new { error = ex.Message });
                }

                return sr;
            }
        }
    }
}
