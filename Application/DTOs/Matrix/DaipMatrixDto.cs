using Domain.Entities;
using Shared.MapperModel;

namespace Application.Dto.Matrix
{
    public class DaipMatrixDto : MapperModel<TopicCategoryDto, DaipMatrix, int>
    {
        public Guid Id { get; set; }
        public string Stage { get; set; }
        public string Grade { get; set; }
        public string Title { get; set; }
        public string SourceUrl { get; set; }
        public int Order { get; set; }
        public int StartRange { get; set; }
        public int EndRange { get; set; }
        public DaipMatrixPayloadDto Items { get; set; } = new();
        public DaipMatrixDto() { }

        public DaipMatrixDto(DaipMatrix entity) : base(entity) { }

        public override void InitializateData() { }

        protected override void ExtraMapFromEntity(DaipMatrix entity) { }

        protected override void ExtraMapToEntity(DaipMatrix entity) { }
    }
    public class DaipMatrixPayloadDto
    {
        public List<string> Pillars { get; set; }
        public List<ItemDto> Items { get; set; }
       
    }
    public class ItemDto
    {
        public string Dimension { get; set; } = string.Empty;

        public string Pillar { get; set; } = string.Empty;

        public string Text { get; set; } = string.Empty;
    }
}
