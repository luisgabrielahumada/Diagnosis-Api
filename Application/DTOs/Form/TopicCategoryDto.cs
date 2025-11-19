using Domain.Entities;
using Domain.Entities.Domain.Entities;
using Shared.MapperModel;

namespace Application.Dto
{
    public class TopicCategoryDto : MapperModel<TopicCategoryDto, TopicCategory, int>
    {
        public Guid? Id { get; set; }
        public string? Description { get; set; }
        public int? Order { get; set; }
        public bool? IsActive { get; set; }
        public List<TopicDto>? Topics { get; set; }
        public TopicCategoryDto() { }

        public TopicCategoryDto(TopicCategory entity) : base(entity) { }

        public override void InitializateData() { }

        protected override void ExtraMapFromEntity(TopicCategory entity) { }

        protected override void ExtraMapToEntity(TopicCategory entity) { }
    }

    public class TopicDto : MapperModel<TopicDto, Topic, int>
    {
        public Guid Id { get; set; }
        public Guid CategoryId { get; set; }
        public string Description { get; set; } = default!;
        public TopicDto() { }

        public TopicDto(Topic entity) : base(entity) { }

        public override void InitializateData() { }

        protected override void ExtraMapFromEntity(Topic entity) { }

        protected override void ExtraMapToEntity(Topic entity) { }
    }
}
