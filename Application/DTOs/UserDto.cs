using Domain.Entities;
using Shared.MapperModel;

namespace Infrastructure.Dto
{
    public class UserDto : MapperModel<UserDto, User, int>
    {
        public Guid? Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Login { get; set; }
        public string? PasswordHash { get; set; }
        public Guid? RoleId { get; set; }
        public string? RoleCode { get; set; }
        public string? Phone { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public DateTime? CreatedAt { get; set; }
        public Guid? CampusId { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsDeleted { get; set; } = false;
        public RoleDto? Role { get; set; }

        public UserDto() { }

        public UserDto(User entity) : base(entity)
        {
          
        }

        public override void InitializateData() { }

        protected override void ExtraMapFromEntity(User entity)
        {
            if (entity.Role is Role role)
                this.Role = new RoleDto(role);

        }

        protected override void ExtraMapToEntity(User entity) { }
    }
}
