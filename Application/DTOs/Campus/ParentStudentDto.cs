using Application.Dto.Security;
using Domain.Entities;
using Domain.Enums;
using Shared.MapperModel;

namespace Application.Dto.Campus
{
    public class ParentStudentDto : MapperModel<ParentStudentDto, ParentStudent, int>
    {
        public Guid Id { get; set; }

        public Guid ParentId { get; set; }
        public ParentDto Parent { get; set; } = default!;

        public Guid StudentId { get; set; }
        public StudentDto Student { get; set; } = default!;

        public ParentRelationshipType Relationship { get; set; }
        public bool IsPrimaryContact { get; set; } = false;

        public ParentStudentDto() { }

        public ParentStudentDto(ParentStudent entity) : base(entity) { }

        public override void InitializateData() { }

        protected override void ExtraMapFromEntity(ParentStudent entity) { }

        protected override void ExtraMapToEntity(ParentStudent entity) { }
    }
}
