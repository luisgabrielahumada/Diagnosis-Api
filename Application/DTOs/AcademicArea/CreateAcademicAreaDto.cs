using Application.Dto;
using Domain.Entities;
using Shared.MapperModel;
using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Dto
{
    public class CreateAcademicAreaDto : MapperModel<AcademicAreaDto, AcademicArea, int>
    {
        public Guid? Id { get; set; }
        [Required(ErrorMessage = "El nombre es requerido")]
        public string Name { get; set; }
        public string? Code { get; set; }
        public string? Description { get; set; }
        public string? Color { get; set; }
        public int? DisplayOrder { get; set; } = 0;
        public bool? IsActive { get; set; } = true;
        public bool? IsDeleted { get; set; } = false;
        public CreateAcademicAreaDto() { }

        public CreateAcademicAreaDto(AcademicArea entity) : base(entity) { }

        public override void InitializateData() { }

        protected override void ExtraMapFromEntity(AcademicArea entity)
        {

        }

        protected override void ExtraMapToEntity(AcademicArea entity) { }
    }
}
