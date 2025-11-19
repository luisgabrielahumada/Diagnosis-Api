using Infrastructure.Dto;

namespace Application.Dto.DataList
{
    public class CampusWithAreasDto
    {
        public Guid CampusId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public IList<AcademicAreaDto> Areas { get; set; }
        public IList<UserDto> Teachers { get; set; }
       // public IList<SubjectDto> Subjects { get; set; }
    }
}
