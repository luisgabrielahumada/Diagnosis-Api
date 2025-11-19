using Domain.Entities;
using Shared.MapperModel;

namespace Infrastructure.Dto
{
    public class CycleReviewDto : MapperModel<CycleReviewDto, CycleReview, int>
    {
        public Guid? Id { get; set; }
        public int? Status { get; set; } = 0;
        public string? UserName { get; set; }
        public string Comments { get; set; }
        public string? InternalNotes { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public Guid? PlanningCycleId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool? IsDeleted { get; set; } = false;
        public bool? IsActive { get; set; }
        public PlanningCycleDto? PlanningCycle { get; set; }


        public CycleReviewDto() { }

        public CycleReviewDto(CycleReview entity) : base(entity) { }

        public override void InitializateData() { }

        protected override void ExtraMapFromEntity(CycleReview entity) { }

        protected override void ExtraMapToEntity(CycleReview entity) { }
    }
}
