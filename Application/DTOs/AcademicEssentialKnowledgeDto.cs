using Domain.Entities;
using Shared.MapperModel;

namespace Infrastructure.Dto
{
    public class AcademicEssentialKnowledgeDto : MapperModel<AcademicEssentialKnowledgeDto, AcademicEssentialKnowledge, int>
    {
        public Guid? Id { get; set; }
        public Guid? AcademicObjectiveId { get; set; }
        public string? Code { get; set; }
        public string? Description { get; set; }
        public string? DescriptionEn { get; set; }
        public int? DisplayOrder { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool? IsDeleted { get; set; } = false;
        public bool? IsActive { get; set; }
        public AcademicObjectiveDto? AcademicObjective { get; set; }
        public AcademicEssentialKnowledgeDto() { }

        public AcademicEssentialKnowledgeDto(AcademicEssentialKnowledge entity) : base(entity) { }

        public override void InitializateData() { }

        protected override void ExtraMapFromEntity(AcademicEssentialKnowledge entity)
        {
            //if (entity.Language is Language)
            //    this.Language = new LanguageDto(entity.Language);
            if (entity.AcademicObjective is AcademicObjective)
                this.AcademicObjective = new AcademicObjectiveDto(entity.AcademicObjective);
            //if (entity.AcademicArea is AcademicArea)
            //    this.AcademicArea = new AcademicAreaDto(entity.AcademicArea);
            //if (entity.Grade is Grade)
            //    this.Grade = new GradeDto(entity.Grade);
            //if (entity.AcademicPeriod is AcademicPeriod)
            //    this.AcademicPeriod = new AcademicPeriodDto(entity.AcademicPeriod);
        }

        protected override void ExtraMapToEntity(AcademicEssentialKnowledge entity) { }
    }
}
