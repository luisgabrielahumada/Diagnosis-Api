using Application.Dto;
using Domain.Entities;
using Shared.MapperModel;
using Shared.Pagination;

namespace Infrastructure.Dto
{

    public class PlanningInboxDto
    {
        public PaginatedList<PlanningDto> Planning { get; set; }
        public Dictionary<string?, int> Status { get; set; }
    }
    public class PlanningDto : MapperModel<PlanningDto, Planning, int>
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

        public PlanningDto(Planning entity) : base(entity) { }

        public override void InitializateData() { }

        protected override void ExtraMapFromEntity(Planning entity)
        {
            if (entity.AcademicArea is AcademicArea)
                this.AcademicArea = new AcademicAreaDto(entity.AcademicArea);
            if (entity.Grade is Grade)
                this.Grade = new GradeDto(entity.Grade);
            if (entity.Subject is Subject)
                this.Subject = new SubjectDto(entity.Subject);
            if (entity.AcademicPeriod is AcademicPeriod)
                this.AcademicPeriod = new AcademicPeriodDto(entity.AcademicPeriod);
            if (entity.AcademicArea is AcademicArea)
                this.AcademicArea = new AcademicAreaDto(entity.AcademicArea);
            if (entity.Campus is Campus)
                this.Campus = new CampusDto(entity.Campus);
            if (entity.Teacher is User)
                this.Teacher = new UserDto(entity.Teacher);
            if (entity.PlanningCompetencies is List<PlanningCompetence>)
                this.PlanningCompetencies = MappingHelper.MapEntityListToMapperModelList<PlanningCompetence, PlanningCompetenceDto>(entity.PlanningCompetencies);
            if (entity.PlanningMethodologies is List<PlanningMethodology>)
                this.PlanningMethodologies = MappingHelper.MapEntityListToMapperModelList<PlanningMethodology, PlanningMethodologyDto>(entity.PlanningMethodologies);
            if (entity.PlanningPerformances is List<PlanningPerformance>)
                this.PlanningPerformances = MappingHelper.MapEntityListToMapperModelList<PlanningPerformance, PlanningPerformanceDto>(entity.PlanningPerformances);
            if (entity.PlanningUnits is List<PlanningUnit>)
                this.PlanningUnits = MappingHelper.MapEntityListToMapperModelList<PlanningUnit, PlanningUnitDto>(entity.PlanningUnits);
            if (entity.PlanningCycles is List<PlanningCycle>)
                this.PlanningCycles = MappingHelper.MapEntityListToMapperModelList<PlanningCycle, PlanningCycleDto>(entity.PlanningCycles);
            if (entity.Language is Language)
                this.Language = new LanguageDto(entity.Language);
            if (entity.Course is Course)
                this.Course = new CourseDto(entity.Course);
            if(entity.CloneLogsAsTarget is List<PlanningCloneLog> )
                this.IsCloned = entity.CloneLogsAsTarget.Count();
            this.Status = entity.Status.ToLower();
        }

        protected override void ExtraMapToEntity(Planning entity) { }
    }
}
