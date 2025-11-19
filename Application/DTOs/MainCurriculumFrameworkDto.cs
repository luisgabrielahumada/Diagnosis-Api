using Application.Dto.Filters;
using Domain.Entities;
using Shared.MapperModel;
using Shared.Pagination;

namespace Infrastructure.Dto
{

    public class MainCurriculumFrameworkInboxDto
    {
        public PaginatedList<MainCurriculumFrameworkDto> Curriculum { get; set; }
        public Dictionary<string?, int> Status { get; set; }
    }
    public class MainCurriculumFrameworkDto : MapperModel<MainCurriculumFrameworkDto, MainCurriculumFramework, int>
    {
        public Guid? Id { get; set; }
        public string? AcademicPeriod { get; set; }
        public Guid? AcademicPeriodId { get; set; }
        public string? Subject { get; set; }
        public Guid? SubjectId { get; set; }
        public string? AcademicArea { get; set; }
        public Guid? AcademicAreaId { get; set; }
        public string? Language { get; set; }
        public Guid? LanguageId { get; set; }
        public string? Grade { get; set; }
        public Guid? GradeId { get; set; }
        public string? Course { get; set; }
        public string? Color { get; set; }
        public string? CreatedAt { get; set; }
        public string? UpdatedAt { get; set; }
        public string? Status { get; set; }
        public bool? IsActive { get; set; } = true;
        public bool? IsDeleted { get; set; } = false;
        public List<MainCurriculumFrameworkFileDto>? MainCurriculumFrameworkFile { get; set; }
        public List<CompetenceDto>? Competencies { get; set; }
        public List<AcademicPerformanceDto>? AcademicPerformances { get; set; }
        public List<AcademicUnitDto>? AcademicUnits { get; set; }

        public MainCurriculumFrameworkDto() { }

        public MainCurriculumFrameworkDto(MainCurriculumFramework entity) : base(entity) { }

        public override void InitializateData() { }

        protected override void ExtraMapFromEntity(MainCurriculumFramework entity) { }

        protected override void ExtraMapToEntity(MainCurriculumFramework entity) { }
    }
    public class MainCurriculumFrameworkFileDto : MapperModel<MainCurriculumFrameworkFileDto, MainCurriculumFrameworkFile, int>
    {
        public Guid? Id { get; set; }
        public Guid? MainCurriculumFrameworkFileId { get; set; }
        public MainCurriculumFrameworkDto? MainCurriculumFramework { get; set; }
        public string? FileName { get; set; }
        public int? ImportCount { get; set; }
        public byte[]? Content { get; set; }
        public string? CreatedAt { get; set; }
        public string? UpdatedAt { get; set; }
        public string? Status { get; set; }
        public bool? IsActive { get; set; } = true;
        public bool? IsDeleted { get; set; } = false;
        public MainCurriculumFrameworkFileDto() { }

        public MainCurriculumFrameworkFileDto(MainCurriculumFrameworkFile entity) : base(entity) { }

        public override void InitializateData() { }

        protected override void ExtraMapFromEntity(MainCurriculumFrameworkFile entity) { }

        protected override void ExtraMapToEntity(MainCurriculumFrameworkFile entity) { }
    }
}
