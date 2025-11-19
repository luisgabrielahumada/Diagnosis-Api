using Application.Dto;
using Domain.Entities;
using Shared.MapperModel;

namespace Application.Dto.Mcc
{
    public class AcademicMethodologyDto : MapperModel<AcademicMethodologyDto, AcademicMethodology, int>
    {
        public Guid? Id { get; set; }
        public Guid? AcademicUnitId { get; set; }
        public Guid? AcademicAreaId { get; set; }
        public string? Code { get; set; }
        public string? Description { get; set; }
        public string? DescriptionEn { get; set; }
        public Guid? GradeId { get; set; }
        public Guid? AcademicPeriodId { get; set; }
        public int? DisplayOrder { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool? IsDeleted { get; set; } = false;
        public bool? IsActive { get; set; }
        //public AcademicUnitDto? AcademicUnit { get; set; }
        public AcademicAreaDto? AcademicArea { get; set; }
        public GradeDto? Grade { get; set; }
        public AcademicPeriodDto? AcademicPeriod { get; set; }
        public LanguageDto? Language { get; set; }

        public AcademicMethodologyDto() { }

        public AcademicMethodologyDto(AcademicMethodology entity) : base(entity) { }

        public override void InitializateData() { }

        protected override void ExtraMapFromEntity(AcademicMethodology entity)
        {
            if (entity.Language is Language)
                Language = new LanguageDto(entity.Language);
            //if (entity.AcademicArea is AcademicArea)
            //    this.AcademicArea = new AcademicAreaDto(entity.AcademicArea);
            //if (entity.Grade is Grades)
            //    this.Grade = new GradeDto(entity.Grade);
            //if (entity.AcademicPeriod is AcademicPeriod)
            //    this.AcademicPeriod = new AcademicPeriodDto(entity.AcademicPeriod);
        }

        protected override void ExtraMapToEntity(AcademicMethodology entity) { }
    }
}
