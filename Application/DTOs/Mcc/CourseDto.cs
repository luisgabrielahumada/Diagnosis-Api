using Domain.Entities;
using Shared.MapperModel;
using System.ComponentModel.DataAnnotations;

namespace Application.Dto.Mcc
{
    public class CourseDto : MapperModel<CourseDto, Course, int>
    {
        public Guid? Id { get; set; }
        [Required(ErrorMessage = "El nombre es requerido")]
        public string Name { get; set; }
        public bool? IsActive { get; set; } = true;
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool? IsDeleted { get; set; } = false;

        public CourseDto() { }

        public CourseDto(Course entity) : base(entity) { }

        public override void InitializateData() { }

        protected override void ExtraMapFromEntity(Course entity) { }

        protected override void ExtraMapToEntity(Course entity) { }
    }
}
