using Application.Dto;
using Domain.Entities;
using Shared.MapperModel;
using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Dto
{
    public class SubjectDto : MapperModel<SubjectDto, Subject, int>
    {
        public Guid? Id { get; set; }
        [Required(ErrorMessage = "El área es requerido")]
        public Guid? AcademicAreaId { get; set; }
        [Required(ErrorMessage = "El Campus es requerido")]
        //public Guid? CampusId { get; set; }
        //[Required(ErrorMessage = "El Docente es requerido")]
        //public Guid? TeacherId { get; set; }
        //[Required(ErrorMessage = "El nombre es requerido")]
        public string Name { get; set; }
        public string? Code { get; set; }
        public string? Alias { get; set; }
        public int? WeeklyHours { get; set; } = 0;
        public bool? IsBilingual { get; set; } = false;
        public string? Description { get; set; }
        public AcademicAreaDto? AcademicArea { get; set; }
        //public CampusDto? Campus { get; set; }
        //public UserDto? Teacher { get; set; }
        public bool? IsActive { get; set; } = true;
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool? IsDeleted { get; set; } = false;

        public SubjectDto() { }

        public SubjectDto(Subject entity) : base(entity)
        {
          
        }

        public override void InitializateData() { }

        protected override void ExtraMapFromEntity(Subject entity)
        {
            if(entity.AcademicArea is AcademicArea)
                this.AcademicArea = new AcademicAreaDto(entity.AcademicArea);
        }

        protected override void ExtraMapToEntity(Subject entity) { }
    }
}
