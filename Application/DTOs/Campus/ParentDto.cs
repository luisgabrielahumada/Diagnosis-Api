using Application.Dto.Security;
using Domain.Entities;
using Shared.MapperModel;

namespace Application.Dto.Campus
{
    public class ParentDto : MapperModel<ParentDto, Parent, int>
    {
        public Guid Id { get; set; }

        public UserDto User { get; set; } = default!;

        public ParentDto() { }

        public ParentDto(Parent entity) : base(entity) { }

        public override void InitializateData() { }

        protected override void ExtraMapFromEntity(Parent entity) { }

        protected override void ExtraMapToEntity(Parent entity) { }
    }
}
