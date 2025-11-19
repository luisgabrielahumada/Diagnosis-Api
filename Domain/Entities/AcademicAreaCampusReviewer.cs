namespace Domain.Entities
{
    public class AcademicAreaCampusReviewer 
    {
        public Guid AcademicAreaId { get; set; }
        public AcademicArea AcademicArea { get; set; }

        public Guid CampusId { get; set; }
        public Campus Campus { get; set; }

        public Guid ReviewerId { get; set; }
        public User Reviewer { get; set; }
    }
}