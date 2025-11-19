using Shared.MapperModel;
using Domain.Entities;

namespace Infrastructure.Dto
{
    public class SessionDto : MapperModel<SessionDto, User, int>
    {
        public string OldToken { get; set; }

        public SessionDto() { }

        public SessionDto(User entity) : base(entity) { }

        public override void InitializateData() { }

        protected override void ExtraMapFromEntity(User entity) { }

        protected override void ExtraMapToEntity(User entity) { }
    }
}
