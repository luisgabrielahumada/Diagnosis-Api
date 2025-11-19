using Domain.Entities;
using Shared.Extensions;
using Shared.MapperModel;
using System.ComponentModel.DataAnnotations;

namespace Application.Dto.Mcc
{
    public class AcademicPeriodDto : MapperModel<AcademicPeriodDto, AcademicPeriod, int>
    {
        public Guid? Id { get; set; }
        [Required(ErrorMessage = "El nombre es requerido")]
        public string Name { get; set; }
        [Required(ErrorMessage = "El nombre es requerido")]
        public string Alias
        {
            get
            {
                return Name.PeriodYear(StartDate.Value, EndDate.Value);
            }
        }
        public int? Type { get; set; }
        public int? Year
        {
            get
            {
                return StartDate.Value.Year;
            }
        }
        [Required(ErrorMessage = "La fecha de inicio es requerida")]
        public DateTime? StartDate { get; set; }
        [Required(ErrorMessage = "La fecha de fin es requerida")]
        public DateTime? EndDate { get; set; }
        public bool? IsActive { get; set; } = true;
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool? IsDeleted { get; set; } = false;

        public AcademicPeriodDto() { }

        public AcademicPeriodDto(AcademicPeriod entity) : base(entity) { }

        public override void InitializateData() { }

        protected override void ExtraMapFromEntity(AcademicPeriod entity) { }

        protected override void ExtraMapToEntity(AcademicPeriod entity) { }
    }
}
