using Application.Dto.Mcc;
using Domain.Entities;
using Shared.MapperModel;

namespace Application.Dto.Campus
{
    public class CampusGradeDto : MapperModel<CampusGradeDto, CampusGrade, int>
    {
        public Guid Id { get; set; }
        public Guid CampusId { get; set; }
        public CampusDto Campus { get; set; }

        public Guid GradeId { get; set; }
        public GradeDto Grade { get; set; }

        public Guid CourseId { get; set; }
        public CourseDto Course { get; set; }
        public CampusGradeDto() { }

        public CampusGradeDto(CampusGrade entity) : base(entity)
        {

        }

        public override void InitializateData() { }

        protected override void ExtraMapFromEntity(CampusGrade entity)
        {
            //if (entity.Users is ICollection<User>)
            //   this.Reviewrs = entity.Users.Select(t => new UserDto(t)).ToList();

        }

        protected override void ExtraMapToEntity(CampusGrade entity) { }
    }
}
