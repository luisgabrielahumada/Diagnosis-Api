using Domain.Entities;
using Shared.MapperModel;

namespace Infrastructure.Dto
{
    public class AcademicObjectiveDto : MapperModel<AcademicObjectiveDto, AcademicObjective, int>
    {
        public Guid? Id { get; set; }
        public Guid? AcademicUnitId { get; set; }
        public string? Code { get; set; }
        public string? Description { get; set; }
        public string? DescriptionEn { get; set; }
        public int?   DisplayOrder { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool? IsDeleted { get; set; } = false;
        public bool? IsActive { get; set; } = true;
        public AcademicUnitDto? AcademicUnit { get; set; }
        public AcademicObjectiveDto() { }

        public AcademicObjectiveDto(AcademicObjective entity) : base(entity) { }

        public override void InitializateData() { }

        protected override void ExtraMapFromEntity(AcademicObjective entity)
        {

            //if (entity.Language is Language)
            //    this.Language = new LanguageDto(entity.Language);
            if (entity.AcademicUnit is AcademicUnit)
                this.AcademicUnit = new AcademicUnitDto(entity.AcademicUnit);
            //if (entity.AcademicArea is AcademicArea)
            //    this.AcademicArea = new AcademicAreaDto(entity.AcademicArea);
            //if (entity.Grade is Grade)
            //    this.Grade = new GradeDto(entity.Grade);
            //if (entity.AcademicPeriod is AcademicPeriod)
            //    this.AcademicPeriod = new AcademicPeriodDto(entity.AcademicPeriod);
        }

        protected override void ExtraMapToEntity(AcademicObjective entity) { }
    }
}
