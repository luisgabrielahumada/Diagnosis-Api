using Application.Dto.Security;
using Domain.Entities;
using Shared.MapperModel;

namespace Application.Dto.Campus
{
    public class StudentDto : MapperModel<StudentDto, Student, int>
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public DateTime? DateOfBirth { get; set; }

        // Optional: if students log in
        public Guid? UserId { get; set; }
        public ICollection<ParentStudentDto> ParentStudents { get; set; } = new List<ParentStudentDto>();

        public UserDto? User { get; set; }

        public StudentDto() { }

        public StudentDto(Student entity) : base(entity) { }

        public override void InitializateData() { }

        protected override void ExtraMapFromEntity(Student entity) { }

        protected override void ExtraMapToEntity(Student entity) { }
    }
}
