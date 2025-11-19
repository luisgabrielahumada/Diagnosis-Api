namespace Application.Dto
{
    public class PlanningCloneLogDto
    {
        public Guid[] GradeIds { get; set; }
        public Guid[] CourseIds { get; set; }
        public Guid AcademicPeriodId { get; set; }
    }
}
