using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Shared.Extensions;
using Shared.Response;

public class PlanningRepository : IPlanningRepository
{
    private readonly ApplicationDbContext _dbContext;

    public PlanningRepository(ApplicationDbContext ctx)
    {
        _dbContext = ctx;
    }

    public async Task<ServiceResponse> CloneAsync(Guid sourcePlanningId,
                                                        IEnumerable<Guid> gradeIds,
                                                        IEnumerable<Guid> courseIds,
                                                        Guid academicPeriodId,
                                                        string academicYear,
                                                        Guid userId, string userEmail)
    {
        var sr = new ServiceResponse();
        try
        {
            _dbContext.Database.SetCommandTimeout(180);
            // 1) Cargo el planning original con todo el graph
            var source = await _dbContext.Set<Planning>()
                .AsNoTracking()
                .Include(p => p.PlanningCompetencies)
                .Include(p => p.PlanningMethodologies)
                .Include(p => p.PlanningPerformances)
                    .ThenInclude(c => c.PlanningCycles)
                .Include(p => p.PlanningUnits)
                .Include(p => p.PlanningCycles)
                    .ThenInclude(c => c.CycleObjectives)
                        .ThenInclude(o => o.CycleKnowledges)
                .FirstOrDefaultAsync(p => p.Id == sourcePlanningId);


            if (source == null)
            {
                sr.AddError("Source planning not found.");
                return sr;
            }


            foreach (var gradeId in gradeIds)
            {
                foreach (var courseId in courseIds)
                {
                    // 2) Map to a new Planning
                    var clone = new Planning
                    {
                        Id = Guid.NewGuid(),
                        CreatedAt = DateTime.UtcNow,
                        Status = PlanningStatus.Draft.ToString().ToLower(),
                        GradeId = gradeId,
                        CourseId = courseId,
                        AcademicPeriodId = academicPeriodId,
                        AcademicYear = academicYear,
                        SubjectId = source.SubjectId,
                        CampusId = source.CampusId,
                        AcademicAreaId = source.AcademicAreaId,
                        TeachingTime = source.TeachingTime,
                        AssessmentTasks = source.AssessmentTasks,
                        Performance = source.Performance,
                        LinkingQuestions = source.LinkingQuestions,
                        StartingDate = source.StartingDate,
                        FinalDate = source.FinalDate,
                        ScheduleHours = source.ScheduleHours,
                        TeacherId = source.TeacherId,
                        LanguageId = source.LanguageId,
                        IsActive = true,
                    };

                    // 3) Clone Competencies, Methodologies, Performances, Units
                    clone.PlanningCompetencies = source.PlanningCompetencies
                        .Select(pc => new PlanningCompetence
                        {
                            Id = Guid.NewGuid(),
                            PlanningId = clone.Id.Value,
                            CompetenceId = null,
                            Description = pc.Description,
                            DescriptionEn = pc.DescriptionEn,
                            Name = pc.Name,
                            NameEn = pc.NameEn,
                            UserId = userId,
                            IsActive = true,
                            IsOwner = false
                        }).ToList();

                    clone.PlanningMethodologies = source.PlanningMethodologies
                        .Select(pm => new PlanningMethodology
                        {
                            Id = Guid.NewGuid(),
                            PlanningId = clone.Id.Value,
                            AcademicMethodologyId = null,
                            Description = pm.Description,
                            DescriptionEn = pm.DescriptionEn,
                            Name = pm.Name,
                            NameEn = pm.NameEn,
                            UserId = userId,
                            IsActive = true,
                            IsOwner = false
                        }).ToList();

                    clone.PlanningPerformances = source.PlanningPerformances
                        .Select(pp => new PlanningPerformance
                        {
                            Id = Guid.NewGuid(),
                            PlanningId = clone.Id.Value,
                            AcademicPerformanceId = null,
                            Description = pp.Description,
                            DescriptionEn = pp.DescriptionEn,
                            Name = pp.Name,
                            NameEn = pp.NameEn,
                            UserId = userId,
                            IsActive = true,
                            IsOwner = false
                        }).ToList();

                    // 4) Clone Units
                    clone.PlanningUnits = source.PlanningUnits
                        .Select(pu => new PlanningUnit
                        {
                            Id = Guid.NewGuid(),
                            PlanningId = clone.Id.Value,
                            AcademicUnitId = null,
                            Description = pu.Description,
                            DescriptionEn = pu.DescriptionEn,
                            Name = pu.Name,
                            NameEn = pu.NameEn,
                            UserId = userId,
                            IsActive = true,
                            IsOwner = false,
                        }).ToList();

                    // 5) Clone Cycles, incluyendo su Performance (1–1) y Objectives/Knowledges
                    clone.PlanningCycles = source.PlanningCycles
                        .Where(t => t.IsActive.Value == true && t.IsDeleted.Value == false)
                        .Select(c =>
                    {
                        var newCycle = new PlanningCycle
                        {
                            Id = Guid.NewGuid(),
                            StartingDate = c.StartingDate,
                            FinalDate = c.FinalDate,
                            Session = c.Session,
                            Activity = c.Activity,
                            ActivityEn = c.ActivityEn,
                            Resources = c.Resources,
                            ResourcesEn = c.ResourcesEn,
                            Observations = c.Observations,
                            ObservationsEn = c.ObservationsEn,
                            IsApproved = (int)PlanningStatus.Draft,
                            IsActive = true,
                            Name = c.Name,
                            Order = c.Order,
                            //  PlanningPerformanceId = c.PlanningPerformanceId,
                            PlanningId = clone.Id.Value,
                            IsOwner = true,
                            LinkingQuestions = c.LinkingQuestions,
                        };

                        //TODO:Pending Clone list PlanningCyclePerformance for cycle.
                        newCycle.CyclePerformances = c.CyclePerformances.Select(t => new PlanningCyclePerformance
                        {
                            PlanningCycleId = newCycle.Id,
                            PlanningPerformanceId = t.PlanningPerformanceId,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = userEmail,
                            Id = Guid.NewGuid(),
                            Sequence = t.Sequence,
                        }).ToList();
                        // 5c) Clone Objectives and link to Units
                        newCycle.CycleObjectives = c.CycleObjectives
                            .Select(obj =>
                            {
                                var objNew = new CycleObjective
                                {
                                    Id = Guid.NewGuid(),
                                    AcademicObjectiveId = null,
                                    PlanningUnitId = obj.PlanningUnitId,
                                    PlanningCycleId = newCycle.Id,
                                    Description = obj.Description,
                                    DescriptionEn = obj.DescriptionEn,
                                    Name = obj.Name,
                                    NameEn = obj.NameEn,
                                    UserId = userId,
                                    IsActive = true,
                                    IsOwner = true
                                };

                                objNew.CycleKnowledges = obj.CycleKnowledges
                                    .Select(ck => new CycleKnowledge
                                    {
                                        Id = Guid.NewGuid(),
                                        AcademicEssentialKnowledgeId = null,
                                        Description = ck.Description,
                                        DescriptionEn = ck.DescriptionEn,
                                        Name = ck.Name,
                                        NameEn = ck.NameEn,
                                        UserId = userId,
                                        IsActive = true,
                                        IsOwner = true
                                    }).ToList();

                                return objNew;
                            }).ToList();



                        return newCycle;
                    }).ToList();
                    _dbContext.Set<Planning>().Add(clone);
                    await _dbContext.SaveChangesAsync(userEmail);

                    var log = new PlanningCloneLog
                    {
                        SourcePlanningId = sourcePlanningId,
                        ClonedPlanningId = clone.Id.Value,
                        UserId = userId,
                        UserEmail = userEmail,
                        CreatedAt = DateTime.UtcNow,
                        Notes = "Clonación automática desde interfaz"
                    };

                    _dbContext.Set<PlanningCloneLog>().Add(log);
                    await _dbContext.SaveChangesAsync(userEmail);
                }
            }


        }
        catch (Exception ex)
        {
            sr.AddError(ex);
        }

        return sr;
    }
}
