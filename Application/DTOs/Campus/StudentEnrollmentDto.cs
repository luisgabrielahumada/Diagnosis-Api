using Application.Dto.Mcc;
using Domain.Entities;
using Shared.MapperModel;

namespace Application.Dto.Campus
{
    public class StudentEnrollmentDto : MapperModel<StudentEnrollmentDto, StudentEnrollment, int>
    {
        public Guid Id { get; set; }

        public Guid StudentId { get; set; }
        public StudentDto Student { get; set; } = default!;

        public Guid CampusId { get; set; }
        public CampusDto Campus { get; set; } = default!;

        public Guid CampusGradeId { get; set; }
        public CampusGradeDto CampusGrade { get; set; } = default!;

        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
        public DateTime? WithdrawnAt { get; set; }

        public StudentEnrollmentDto() { }

        public StudentEnrollmentDto(StudentEnrollment entity) : base(entity) { }

        public override void InitializateData() { }

        protected override void ExtraMapFromEntity(StudentEnrollment entity) { }

        protected override void ExtraMapToEntity(StudentEnrollment entity) { }
    }
}
