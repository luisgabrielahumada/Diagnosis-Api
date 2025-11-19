using Application.Dto.Security;
using Domain.Entities;
using Shared.MapperModel;

namespace Application.Dto.Campus
{
    public class TeacherDto : MapperModel<TeacherDto, Teacher, int>
    {
        public Guid Id { get; set; }
        public UserDto User { get; set; } = default!;
        public TeacherDto() { }

        public TeacherDto(Teacher entity) : base(entity) { }

        public override void InitializateData() { }

        protected override void ExtraMapFromEntity(Teacher entity) { }

        protected override void ExtraMapToEntity(Teacher entity) { }
    }
}
