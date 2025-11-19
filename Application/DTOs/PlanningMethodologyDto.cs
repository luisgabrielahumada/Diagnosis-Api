using Domain.Entities;
using Shared.MapperModel;

namespace Infrastructure.Dto
{
    public class PlanningMethodologyDto : MapperModel<PlanningMethodologyDto, PlanningMethodology, int>
    {
        public Guid? Id { get; set; }
        public Guid? PlanningId { get; set; }
        public string? Name { get; set; }
        public string? NameEn { get; set; }
        public string? Code { get; set; }
        public string Description { get; set; }
        public string? DescriptionEn { get; set; }
        public Guid? UserId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool? IsDeleted { get; set; } = false;
        public bool? IsOwner { get; set; }
        public bool? IsActive { get; set; }
        public PlanningDto? Planning { get; set; }


        public PlanningMethodologyDto() { }

        public PlanningMethodologyDto(PlanningMethodology entity) : base(entity) { }

        public override void InitializateData() { }

        protected override void ExtraMapFromEntity(PlanningMethodology entity) { }

        protected override void ExtraMapToEntity(PlanningMethodology entity) { }
    }
}
