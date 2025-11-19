using Application.Dto.Mcc;
using Domain.Entities;
using Shared.MapperModel;

namespace Application.Dto.Campus
{
    public class TeachingAssignmentDto : MapperModel<TeachingAssignmentDto, TeachingAssignment, int>
    {
        public Guid Id { get; set; }
        public Guid? CampusGradeId { get; set; }
        public TeacherDto? Teacher { get; set; } = default!;
        public GradeDto? Grade { get; set; } = default!;
        public CourseDto? Course { get; set; } = default!;
        public AcademicPeriodDto? AcademicPeriod { get; set; } = default!;
        public string? TeacherType { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public TeachingAssignmentDto() { }

        public TeachingAssignmentDto(TeachingAssignment entity) : base(entity) { }

        public override void InitializateData() { }

        protected override void ExtraMapFromEntity(TeachingAssignment entity) { }

        protected override void ExtraMapToEntity(TeachingAssignment entity) { }
    }
}
