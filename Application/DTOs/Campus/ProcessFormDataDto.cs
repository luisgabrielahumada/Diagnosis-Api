using Domain.Entities;
using Shared.MapperModel;

namespace Application.Dto.Campus
{
    public class ProcessFormDataDto : MapperModel<ProcessFormDataDto, ProcessFormData, int>
    {
        public Guid Id { get; set; }
        public Guid ProessFormId { get; set; }
        public ProcessFormDto? ProessForm { get; set; }
        public FamilyInfoDto? FamilyInfo { get; set; } = new();
        public List<PlanDto>? Plan { get; set; } = new();
        public List<PrioritiesDto>? Priorities { get; set; } = new();
        public List<LearningUnitDto> Learning { get; set; } =  new();
        public string? ActionPlan { get; set; }
        public string? ActionPlanHouse { get; set; }
        public string? ActionPlanSchool { get; set; }
        public string? Observation { get; set; }
        public SignatureParentDto? Signature { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public SignatureTeacherDto? SignatureTeacher { get; set; }
        public SignatureCoordinatorDto? SignatureCoordinator { get; set; }

        public ProcessFormDataDto() { }

        public ProcessFormDataDto(ProcessFormData entity) : base(entity) { }

        public override void InitializateData() { }

        protected override void ExtraMapFromEntity(ProcessFormData entity) { }

        protected override void ExtraMapToEntity(ProcessFormData entity) { }
    }

    public class FamilyInfoDto
    {
        public string? Father { get; set; }
        public string? FatherProfession { get; set; }
        public string? Mother { get; set; }
        public string? MotherProfession { get; set; }
        public string? FamilyStatus { get; set; }
        public string? MedidalObservation { get; set; }
        public string? Owner { get; set; }
    }

    public class PlanDto
    {
        public string? Progress { get; set; }
        public string? ToImprovement { get; set; }
    }

    public class PrioritiesDto
    {
        public string? Dimension { get; set; }
        public string? Constituent { get; set; }
        public string? Title { get; set; }
        public string? PositiveAspects { get; set; }
        public string? ToImprovement { get; set; }
    }

    public class LearningUnitDto
    {
        public string? Category1 { get; set; }
        public string? Category2 { get; set; }
        public string? Category3 { get; set; }
        public string? CategoryLove { get; set; }
    }

    public class SignatureParentDto
    {
        public string? Parent1 { get; set; }
        public string? Parent2 { get; set; }
    }

    public class SignatureTeacherDto
    {
        public string? Teacher { get; set; }
    }

    public class SignatureCoordinatorDto
    {
        public string? Coordinator { get; set; }
    }
}