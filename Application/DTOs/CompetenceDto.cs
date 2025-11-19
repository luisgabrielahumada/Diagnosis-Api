using Application.Dto;
using Domain.Entities;
using Shared.MapperModel;

namespace Infrastructure.Dto
{
    public class CompetenceDto : MapperModel<CompetenceDto, Competence, int>
    {
        public Guid? Id { get; set; }
        public Guid? AcademicAreaId { get; set; }
        public string? Name { get; set; }
        public string? NameEn { get; set; }
        public string? Code { get; set; }
        public string Description { get; set; }
        public string? DescriptionEn { get; set; }
        public Guid? GradeId { get; set; }
        public Guid? AcademicPeriodId { get; set; }
        public Guid? LanguageId { get; set; }
        public Guid? SubjectId { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool? IsDeleted { get; set; } = false;
        public AcademicAreaDto? AcademicArea { get; set; }
        public GradeDto? Grade { get; set; }
        public AcademicPeriodDto? AcademicPeriod { get; set; }
        public LanguageDto? Language { get; set; }
        public SubjectDto? Subject { get; set; }

        public CompetenceDto() { }

        public CompetenceDto(Competence entity) : base(entity) { }

        public override void InitializateData() { }

        protected override void ExtraMapFromEntity(Competence entity)
        {
            if (entity.Language is Language)
                this.Language = new LanguageDto(entity.Language);
            if (entity.AcademicArea is AcademicArea)
                this.AcademicArea = new AcademicAreaDto(entity.AcademicArea);
            if (entity.Grade is Grade)
                this.Grade = new GradeDto(entity.Grade);
            if (entity.AcademicPeriod is AcademicPeriod)
                this.AcademicPeriod = new AcademicPeriodDto(entity.AcademicPeriod);
            if (entity.Subject is Subject)
                this.Subject = new SubjectDto(entity.Subject);
        }

        protected override void ExtraMapToEntity(Competence entity) { }
    }
}
