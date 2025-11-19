using Application.Dto;
using Infrastructure.Dto;

namespace Application.Dto.DataList
{
    public class AcademicAreaWithReviewsDto:AcademicAreaDto
    {
        public UserDto Reviewer { get; set; }
        public CampusDto Campus { get; set; }
    }
}
