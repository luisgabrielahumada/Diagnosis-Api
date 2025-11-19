using Domain.Entities;
using Shared.MapperModel;

namespace Application.Dto.Planning
{
    public class PlanningPerformanceDto : MapperModel<PlanningPerformanceDto, PlanningPerformance, int>
    {
        public Guid? Id { get; set; }
        public Guid? PlanningId { get; set; }
        public string? Name { get; set; }
        public string? NameEn { get; set; }
        public string? Code { get; set; }
        public string? Description { get; set; }
        public string? DescriptionEn { get; set; }
        public Guid? UserId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool? IsDeleted { get; set; } = false;
        public bool? IsActive { get; set; }
        public bool? IsOwner { get; set; }
        public PlanningDto? Planning { get; set; }

        public PlanningPerformanceDto() { }

        public PlanningPerformanceDto(PlanningPerformance entity) : base(entity) { }

        public override void InitializateData() { }

        protected override void ExtraMapFromEntity(PlanningPerformance entity) { }

        protected override void ExtraMapToEntity(PlanningPerformance entity) { }
    }
}
