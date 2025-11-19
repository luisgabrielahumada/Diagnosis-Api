using Domain.Entities;
using Shared.MapperModel;
using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Dto
{
    public class LanguageDto : MapperModel<LanguageDto, Language, int>
    {
        public Guid? Id { get; set; }
        [Required(ErrorMessage = "El nombre es requerido")]
        public string Name { get; set; }
        public bool? IsActive { get; set; } = true;
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool? IsDeleted { get; set; } = false;

        public LanguageDto() { }

        public LanguageDto(Language entity) : base(entity) { }

        public override void InitializateData() { }

        protected override void ExtraMapFromEntity(Language entity) { }

        protected override void ExtraMapToEntity(Language entity) { }
    }
}
