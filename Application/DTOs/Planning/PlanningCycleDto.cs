using Domain.Entities;
using Shared.MapperModel;
using System.ComponentModel.DataAnnotations;

namespace Application.Dto.Planning
{
    public class PlanningCycleDto : MapperModel<PlanningCycleDto, PlanningCycle, int>
    {
        public Guid? Id { get; set; }
        [Required]
        public Guid? PlanningId { get; set; }
        //public Guid? PlanningPerformanceId { get; set; }
        public DateTime? StartingDate { get; set; }
        public DateTime? FinalDate { get; set; }
        public int? Session { get; set; }
        public string? Activity { get; set; }
        public string? ActivityEn { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? Resources { get; set; }
        public string? ResourcesEn { get; set; }
        public string? Observations { get; set; }
        public string? ObservationsEn { get; set; }
        public Guid? UserId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? Order { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool? IsDeleted { get; set; } = false;
        public bool? IsActive { get; set; } = true;
        public bool? IsOwner { get; set; }
        public int IsApproved { get; set; }
        public string? LinkingQuestions { get; set; }
        public PlanningDto? Planning { get; set; }
        public List<CycleObjectiveDto>? CycleObjectives { get; set; }
        public List<CycleKnowledgeDto>? CycleKnowledges { get; set; }
        public List<PlanningPerformanceDto>? CyclePerformances { get; set; }

        public PlanningCycleDto() { }

        public PlanningCycleDto(PlanningCycle entity) : base(entity)
        {
            if (entity.Planning is Domain.Entities.Planning)
                Planning = new PlanningDto(entity.Planning);
            if (entity.CycleObjectives is Domain.Entities.Planning)
                CycleObjectives = MappingHelper.MapEntityListToMapperModelList<CycleObjective, CycleObjectiveDto>(entity.CycleObjectives);
            if (entity.CyclePerformances is List<PlanningCyclePerformance>)
            {
                CyclePerformances = entity
                                            .CyclePerformances
                                            .Select(t => new PlanningPerformanceDto(t.PlanningPerformance)).ToList();
            }
            //if (entity.CycleKnowledges is Planning)
            //    this.CycleKnowledges = MappingHelper.MapEntityListToMapperModelList<CycleKnowledge, CycleKnowledgeDto>(entity.CycleKnowledges);
            //if (entity.PlanningPerformance is PlanningPerformance)
            //    this.PlanningPerformance = new PlanningPerformanceDto(entity.PlanningPerformance);
        }

        public override void InitializateData() { }

        protected override void ExtraMapFromEntity(PlanningCycle entity)
        {

        }

        protected override void ExtraMapToEntity(PlanningCycle entity) { }
    }
}
