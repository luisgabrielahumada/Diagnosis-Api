using Domain.Entities;
using Shared.MapperModel;

namespace Infrastructure.Dto
{
    public class CycleKnowledgeDto : MapperModel<CycleKnowledgeDto, CycleKnowledge, int>
    {
        public Guid? Id { get; set; }
        public Guid? KnowledgeId { get; set; }
        //public Guid? PlanningCycleId { get; set; }
        public Guid? CycleObjectiveId { get; set; }
        public string Name { get; set; }
        public string NameEn { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string DescriptionEn { get; set; }
        public Guid? UserId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool? IsDeleted { get; set; } = false;
        public bool? IsActive { get; set; }
        //public PlanningCycleDto? PlanningCycle { get; set; }
        public CycleObjectiveDto? CycleObjective { get; set; }

        public CycleKnowledgeDto() { }

        public CycleKnowledgeDto(CycleKnowledge entity) : base(entity) { }

        public override void InitializateData() { }

        protected override void ExtraMapFromEntity(CycleKnowledge entity) { }

        protected override void ExtraMapToEntity(CycleKnowledge entity) { }
    }
}
