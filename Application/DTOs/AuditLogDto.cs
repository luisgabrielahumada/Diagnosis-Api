using Domain.Entities;
using Shared.Extensions;
using Shared.MapperModel;
using System.Text.Json;

namespace Infrastructure.Dto
{
    public class AuditLogDto : MapperModel<AuditLogDto, AuditLog, int>
    {
        public Guid? Id { get; set; }
        public string TableName { get; set; } = default!;

        public string ActionType { get; set; } = default!; // "Insert", "Update", "Delete"

        public Guid RecordId { get; set; } = default!;

        public string UserName { get; set; } = default!;

        public DateTime Timestamp { get; set; }

        public Dictionary<string, object?>? OldValues { get; set; }

        public Dictionary<string, object?>? NewValues { get; set; }

        public AuditLogDto() { }

        public AuditLogDto(AuditLog entity) : base(entity) { }

        public override void InitializateData() { }

        protected override void ExtraMapFromEntity(AuditLog entity)
        {

            //if (entity.OldValues.IsNotNullOrWhiteSpace())
            //    OldValues = JsonSerializer.Deserialize<Dictionary<string, object?>>(entity.OldValues);
            //if (entity.NewValues.IsNotNullOrWhiteSpace())
            //    NewValues = JsonSerializer.Deserialize<Dictionary<string, object?>>(entity.NewValues);
        }

        protected override void ExtraMapToEntity(AuditLog entity) { }
    }
}
