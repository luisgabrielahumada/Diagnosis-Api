using Domain.Entities;
using Shared.MapperModel;
using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Dto
{
    public class GradeDto : MapperModel<GradeDto, Grade, int>
    {
        public Guid? Id { get; set; }
        [Required(ErrorMessage = "El nombre es requerido")]
        public string Name { get; set; }
        public string? Code { get; set; }
        public int? Level { get; set; }
        public int? Schedule { get; set; } = 0;
        public int? Student { get; set; } = 0;
        public string? Section { get; set; }
        public int? Capacity { get; set; } = 0;
        public int? DisplayOrder { get; set; } = 0;
        public string? Description { get; set; }
        public bool? IsActive { get; set; } = true;
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool? IsDeleted { get; set; } = false;

        public GradeDto() { }

        public GradeDto(Grade entity) : base(entity) { }

        public override void InitializateData() { }

        protected override void ExtraMapFromEntity(Grade entity) { }

        protected override void ExtraMapToEntity(Grade entity) { }
    }
}
