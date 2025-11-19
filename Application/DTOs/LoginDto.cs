using Shared.MapperModel;
using Domain.Entities;

namespace Infrastructure.Dto
{
    public class LoginDto : MapperModel<LoginDto, User, int>
    {
        public string Login { get; set; }
        public string Password { get; set; }

        public LoginDto() { }

        public LoginDto(User entity) : base(entity) { }

        public override void InitializateData() { }

        protected override void ExtraMapFromEntity(User entity) { }

        protected override void ExtraMapToEntity(User entity) { }
    }
}
