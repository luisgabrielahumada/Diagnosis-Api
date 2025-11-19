using Domain.Entities;
using Domain.Enums;
using Shared.MapperModel;
using System.Drawing;

namespace Infrastructure.Dto
{
    public class RoleDto : MapperModel<RoleDto, Role, int>
    {
        public Guid? Id { get; set; }
        public string? Name { get; set; }
        public UserRole? Code { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsDeleted { get; set; } = false;
        public bool? IsEnabledSetting { get; set; }

        public RoleDto() { }

        public RoleDto(Role entity) : base(entity) { }

        public override void InitializateData() { }

        protected override void ExtraMapFromEntity(Role entity) {
           this.Code = (UserRole)Enum.Parse(typeof(UserRole), entity.Code);
        }

        protected override void ExtraMapToEntity(Role entity) { }
    }
}
