using DocumentFormat.OpenXml.Spreadsheet;
using Domain.Entities;
using Infrastructure.Dto;
using Shared.MapperModel;
using System.ComponentModel.DataAnnotations;

namespace Application.Dto
{
    public class AcademicAreaDto : MapperModel<AcademicAreaDto, AcademicArea, int>
    {
        public Guid? Id { get; set; }
        [Required(ErrorMessage = "El nombre es requerido")]
        public string Name { get; set; }
        public string? Code { get; set; }
        public string? Description { get; set; }
        public string? Color { get; set; }
        public int? DisplayOrder { get; set; } = 0;
        public bool? IsActive { get; set; } = true;
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool? IsDeleted { get; set; } = false;
        public List<SubjectDto>? Subjects { get; set; }
        public AcademicAreaDto() { }

        public AcademicAreaDto(AcademicArea entity) : base(entity) { }

        public override void InitializateData() { }

        protected override void ExtraMapFromEntity(AcademicArea entity) { }

        protected override void ExtraMapToEntity(AcademicArea entity) { }
    }
}
