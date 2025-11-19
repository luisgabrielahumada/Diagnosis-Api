using Application.Dto.Mcc;
using Domain.Entities;
using Shared.MapperModel;
using Shared.Pagination;

namespace Application.Dto.Campus
{
    public class ProcessFormDto : MapperModel<ProcessFormDto, ProcessForm, int>
    {
        public Guid Id { get; set; }
        public Guid? StudentId { get; set; }
        public StudentDto? Student { get; set; }
        public Guid? CustomerId { get; set; }
        public ParentDto? Customer { get; set; }
        public Guid? TeacherId { get; set; }
        public TeacherDto? Teacher { get; set; }
        public Guid? TeachingAssignmentId { get; set; }
        public TeachingAssignmentDto? TeachingAssignment { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
        public int? Version { get; set; } = 1;
        public DateTime? InterviewDate { get; set; }
        public string? SendMethod { get; set; }
        public string? AccessToken { get; set; }
        public string? EmailSent { get; set; }
        public string? Type { get; set; }
        public string? Url { get; set; }
        public string? Qr { get; set; }
        public List<ProcessFormReviewDto>? Reviews { get; set; } = new();
        public ProcessFormDto() { }

        public ProcessFormDto(ProcessForm entity) : base(entity) { }

        public override void InitializateData() { }

        protected override void ExtraMapFromEntity(ProcessForm entity)
        {
            this.Status = entity.Status?.ToString().ToLower();
        }

        protected override void ExtraMapToEntity(ProcessForm entity) { }
    }

    public class CreateProcessFormDto : MapperModel<CreateProcessFormDto, ProcessForm, int>
    {
        public Guid StudentId { get; set; }
        public Guid CustomerId { get; set; }
        public Guid TeacherId { get; set; }
        public Guid TeachingAssignmentId { get; set; }
        public DateTime InterviewDate { get; set; }
        public string SendMethod { get; set; }
        public string EmailSent { get; set; }
        public CreateProcessFormDto() { }

        public CreateProcessFormDto(ProcessForm entity) : base(entity) { }

        public override void InitializateData() { }

        protected override void ExtraMapFromEntity(ProcessForm entity) { }

        protected override void ExtraMapToEntity(ProcessForm entity) { }
    }

    public class ProcessFormInboxDto
    {
        public Dictionary<string, int>? Status { get; set; }
        public PaginatedList<ProcessFormDto> List { get; set; }
    }
}