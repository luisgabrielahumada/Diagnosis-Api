using Application.Dto;
using Application.Dto.Common;
using Application.Dto.DataList;

namespace Infrastructure.Dto
{
    public class LookupBundleDto
    {
        public IList<AcademicAreaDto> AcademicAreas { get; set; }
        public IList<SubjectDto> Subjects { get; set; }
        public IList<GradeDto> Grades { get; set; }
        public IList<CampusDto> Campuses { get; set; }
        public IList<AcademicPeriodDto> AcademicPeriods { get; set; }
        public IList<AcademicPeriodDto> AllAcademicPeriods { get; set; }
        public IList<LookupDto> CompetencyType { get; set; }
        public IList<LookupDto> NotificationType { get; set; }
        public IList<LookupDto> PeriodType { get; set; }
        public IList<LookupDto> PlanningStatus { get; set; }
        public IList<LookupDto> ReviewStatus { get; set; }
        public IList<LookupDto> UserRole { get; set; }
        public IList<LookupDto> Years { get; set; }
        public IList<LanguageDto> Languages { get; set; }
        public IList<CourseDto> Courses { get; set; }
        public IList<UserDto> Teachers { get; set; }
        public IList<RoleDto> Roles { get; set; }
        public IList<CampusWithAreasDto> CampusesAllAreas { get; set; }
        public IList<ConfigurationSystemDto> ConfigurationSystem { get; set; }

        public IList<LookupDto> CurriculumStatus { get; set; }
    }
}
