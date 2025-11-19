using Domain.Entities;
using Shared.Extensions;
using Shared.MapperModel;
using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Dto
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
        // public List<CycleKnowledgeDto>? CycleKnowledges { get; set; }
        public List<PlanningPerformanceDto>? CyclePerformances { get; set; }

        public PlanningCycleDto() { }

        public PlanningCycleDto(PlanningCycle entity) : base(entity)
        {
            if (entity.Planning is Planning)
                this.Planning = new PlanningDto(entity.Planning);
            if (entity.CycleObjectives is Planning)
                this.CycleObjectives = MappingHelper.MapEntityListToMapperModelList<CycleObjective, CycleObjectiveDto>(entity.CycleObjectives);
            if (entity.CyclePerformances is List<PlanningCyclePerformance>)
            {
                this.CyclePerformances = entity
                                            .CyclePerformances
                                            //.Where(x=>x.PlanningPerformance is PlanningCyclePerformance)
                                            .Select(t => new PlanningPerformanceDto(t.PlanningPerformance)).ToList();


            }
            if (entity.CycleObjectives is List<CycleObjective>)
            {
                this.CycleObjectives = entity
                                            .CycleObjectives
                                            .Select(t => new CycleObjectiveDto(t)).ToList();

                this.CycleObjectives = this.CycleObjectives?.Select(t =>
                {
                    var obj = entity.CycleObjectives.Where(m => m.Id == t.Id).FirstOrDefault();
                    t.CycleKnowledges = obj.CycleKnowledges.Select(k => new CycleKnowledgeDto(k)).ToList();
                    return t;
                }).ToList();
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
