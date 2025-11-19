using Application.Dto.Campus;
using Application.Dto.Mcc;
using Application.Dto.Security;
using Domain.Entities;
using Shared.MapperModel;
using Shared.Pagination;

namespace Application.Dto.Planning
{

    public class PlanningInboxDto
    {
        public PaginatedList<PlanningDto> PlanningList { get; set; }
        public Dictionary<string?, int> Status { get; set; }
    }
    public class PlanningDto : MapperModel<PlanningDto, Domain.Entities.Planning, int>
    {
        public Guid? Id { get; set; }
        public Guid? SubjectId { get; set; }
        public Guid? GradeId { get; set; }
        public Guid? AcademicPeriodId { get; set; }
        public Guid? CampusId { get; set; }
        public string? AcademicYear { get; set; }
        public Guid? AcademicAreaId { get; set; }
        public int? TeachingTime { get; set; }
        public Guid? TeacherId { get; set; }
        public Guid? CourseId { get; set; }
        public Guid? LanguageId { get; set; }
        public string? Status { get; set; } = "draft";
        public bool? IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool? IsDeleted { get; set; } = false;
        public string? AssessmentTasks { get; set; }
        public string? Performance { get; set; }
        public string? LinkingQuestions { get; set; }
        public DateTime? StartingDate { get; set; }
        public DateTime? FinalDate { get; set; }
        public int? ScheduleHours { get; set; }
        public GradeDto? Grade { get; set; }
        public SubjectDto? Subject { get; set; }
        public AcademicPeriodDto? AcademicPeriod { get; set; }
        public AcademicAreaDto? AcademicArea { get; set; }
        public UserDto? Teacher { get; set; }
        public CampusDto? Campus { get; set; }
        public List<PlanningCompetenceDto>? PlanningCompetencies { get; set; } = new();
        public List<PlanningMethodologyDto>? PlanningMethodologies { get; set; } = new();
        public List<PlanningPerformanceDto>? PlanningPerformances { get; set; } = new();
        public List<PlanningUnitDto>? PlanningUnits { get; set; } = new();
        public List<PlanningCycleDto>? PlanningCycles { get; set; } = new();
        public LanguageDto? Language { get; set; }
        public CourseDto? Course { get; set; }
        public int IsCloned { get; set; } = 0;

        public PlanningDto() { }

        public PlanningDto(Domain.Entities.Planning entity) : base(entity) { }

        public override void InitializateData() { }

        protected override void ExtraMapFromEntity(Domain.Entities.Planning entity)
        {
            if (entity.AcademicArea is AcademicArea)
                AcademicArea = new AcademicAreaDto(entity.AcademicArea);
            if (entity.Grade is Grade)
                Grade = new GradeDto(entity.Grade);
            if (entity.Subject is Subject)
                Subject = new SubjectDto(entity.Subject);
            if (entity.AcademicPeriod is AcademicPeriod)
                AcademicPeriod = new AcademicPeriodDto(entity.AcademicPeriod);
            if (entity.AcademicArea is AcademicArea)
                AcademicArea = new AcademicAreaDto(entity.AcademicArea);
            if (entity.Campus is Domain.Entities.Campus)
                Campus = new CampusDto(entity.Campus);
            if (entity.Teacher is User)
                Teacher = new UserDto(entity.Teacher);
            if (entity.PlanningCompetencies is List<PlanningCompetence>)
                PlanningCompetencies = MappingHelper.MapEntityListToMapperModelList<PlanningCompetence, PlanningCompetenceDto>(entity.PlanningCompetencies);
            if (entity.PlanningMethodologies is List<PlanningMethodology>)
                PlanningMethodologies = MappingHelper.MapEntityListToMapperModelList<PlanningMethodology, PlanningMethodologyDto>(entity.PlanningMethodologies);
            if (entity.PlanningPerformances is List<PlanningPerformance>)
                PlanningPerformances = MappingHelper.MapEntityListToMapperModelList<PlanningPerformance, PlanningPerformanceDto>(entity.PlanningPerformances);
            if (entity.PlanningUnits is List<PlanningUnit>)
                PlanningUnits = MappingHelper.MapEntityListToMapperModelList<PlanningUnit, PlanningUnitDto>(entity.PlanningUnits);
            if (entity.PlanningCycles is List<PlanningCycle>)
                PlanningCycles = MappingHelper.MapEntityListToMapperModelList<PlanningCycle, PlanningCycleDto>(entity.PlanningCycles);
            if (entity.Language is Language)
                Language = new LanguageDto(entity.Language);
            if (entity.Course is Course)
                Course = new CourseDto(entity.Course);
            if (entity.CloneLogsAsTarget is List<PlanningCloneLog>)
                IsCloned = entity.CloneLogsAsTarget.Count();
            Status = entity.Status.ToLower();
        }

        protected override void ExtraMapToEntity(Domain.Entities.Planning entity) { }
    }
}
