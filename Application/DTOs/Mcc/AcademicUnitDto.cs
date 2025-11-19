using Application.Dto;
using Domain.Entities;
using Shared.MapperModel;

namespace Application.Dto.Mcc
{
    public class AcademicUnitDto : MapperModel<AcademicUnitDto, AcademicUnit, int>
    {
        public Guid? Id { get; set; }
        public Guid? AcademicAreaId { get; set; }
        public int? Priority { get; set; }
        public string? Name { get; set; }
        public string? NameEn { get; set; }
        public string? Code { get; set; }
        public string? Description { get; set; }
        public string? DescriptionEn { get; set; }
        public Guid? GradeId { get; set; }
        public Guid? AcademicPeriodId { get; set; }
        public Guid? LanguageId { get; set; }
        public Guid? SubjectId { get; set; }
        public int EstimatedHours { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool? IsDeleted { get; set; } = false;
        public bool? IsActive { get; set; }
        //public CompetenciesDto? Competency { get; set; }
        public AcademicAreaDto? AcademicArea { get; set; }
        public GradeDto? Grade { get; set; }
        public AcademicPeriodDto? AcademicPeriod { get; set; }
        public LanguageDto? Language { get; set; }
        public SubjectDto? Subject { get; set; }
        public List<AcademicObjectiveDto>? AcademicObjectives { get; set; }
        public List<AcademicEssentialKnowledgeDto>? AcademicEssentialKnowledges { get; set; }

        public AcademicUnitDto() { }

        public AcademicUnitDto(AcademicUnit entity) : base(entity) { }

        public override void InitializateData() { }

        protected override void ExtraMapFromEntity(AcademicUnit entity)
        {

            if (entity.Language is Language)
                Language = new LanguageDto(entity.Language);
            if (entity.AcademicArea is AcademicArea)
                AcademicArea = new AcademicAreaDto(entity.AcademicArea);
            if (entity.Grade is Grade)
                Grade = new GradeDto(entity.Grade);
            if (entity.AcademicPeriod is AcademicPeriod)
                AcademicPeriod = new AcademicPeriodDto(entity.AcademicPeriod);
        }

        protected override void ExtraMapToEntity(AcademicUnit entity) { }
    }
}
