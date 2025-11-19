using Domain.Entities;
using Shared.MapperModel;

namespace Application.Dto.Campus
{
    public class ProcessFormReviewDto : MapperModel<ProcessFormReviewDto, ProcessFormReview, int>
    {
        public Guid Id { get; set; }
        public Guid ProcessFormId { get; set; }
        public ProcessFormDto ProcessForm { get; set; }
        public string? Type { get; set; }
        public string? Observations { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;

        public ProcessFormReviewDto() { }

        public ProcessFormReviewDto(ProcessFormReview entity) : base(entity) { }

        public override void InitializateData() { }

        protected override void ExtraMapFromEntity(ProcessFormReview entity) { }

        protected override void ExtraMapToEntity(ProcessFormReview entity) { }
    }

    public class CreateProcessFormReviewDto 
    {
        public string? Observations { get; set; }
    }
}