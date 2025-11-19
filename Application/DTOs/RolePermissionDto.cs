using Domain.Entities;
using Shared.MapperModel;


namespace Application.Dto
{
    public class RolePermissionDto : MapperModel<RolePermissionDto, RolePermission, int>
    {
        public int RoleId { get; set; }
        public int MenuId { get; set; }
        public int PermissionId { get; set; }
        public PermissionDto? Permission { get; set; }
        public string? TwoFactorType { get; set; }
        public RolePermissionDto() { }
        public RolePermissionDto(RolePermission entity) : base(entity) { }
        public override void InitializateData() { }
        protected override void ExtraMapFromEntity(RolePermission entity)
        {
            if (entity.Permission is Permission)
            {
                Permission = new PermissionDto(entity.Permission);
            }
        }
        protected override void ExtraMapToEntity(RolePermission entity) { }
    }
}
