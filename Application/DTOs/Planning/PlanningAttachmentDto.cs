using Domain.Entities;
using Shared.MapperModel;

namespace Application.Dto.Planning
{
    public class PlanningAttachmentDto : MapperModel<PlanningAttachmentDto, PlanningAttachment, int>
    {
        public Guid? Id { get; set; }
        public string FileName { get; set; }
        public string OriginalFileName { get; set; }
        public string FilePath { get; set; }
        public long? FileSize { get; set; }
        public string ContentType { get; set; }
        public Guid? PlanningId { get; set; }
        public Guid? UploadedById { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool? IsDeleted { get; set; } = false;
        public PlanningDto? Planning { get; set; }


        public PlanningAttachmentDto() { }

        public PlanningAttachmentDto(PlanningAttachment entity) : base(entity) { }

        public override void InitializateData() { }

        protected override void ExtraMapFromEntity(PlanningAttachment entity) { }

        protected override void ExtraMapToEntity(PlanningAttachment entity) { }
    }
}
