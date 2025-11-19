namespace Application.Dto.Campus
{
    public class CampusRoleDto
    {
        public Guid? AcademicAreaId { get; set; }
        public Guid CampusId { get; set; }
        public Guid[]? Subjects { get; set; }
        public Guid[]? AcademicAreas { get; set; }
    }
}
