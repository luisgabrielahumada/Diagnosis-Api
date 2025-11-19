using Application.Dto.Security;
using Domain.Entities;
using Shared.MapperModel;

namespace Application.Dto.Campus
{
    public class TeacherCampusDto : MapperModel<TeacherCampusDto, TeacherCampus, int>
    {
        public Guid Id { get; set; }
        public TeacherDto Teacher { get; set; } = default!;
        public CampusDto Campus { get; set; } = default!;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public TeacherCampusDto() { }
        public TeacherCampusDto(TeacherCampus entity) : base(entity) { }
        public override void InitializateData() { }
        protected override void ExtraMapFromEntity(TeacherCampus entity) { }
        protected override void ExtraMapToEntity(TeacherCampus entity) { }
    }
}
