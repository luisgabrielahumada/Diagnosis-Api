using Application.Dto;
using Application.Dto.Common;
using Domain.Entities;
using Infrastructure.Dto.Filters;
using Shared.MapperModel;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Dto
{
    public class CampusDto : MapperModel<CampusDto, Campus, int>
    {
        public Guid? Id { get; set; }
        [Required(ErrorMessage = "El nombre es requerido")]
        public string Name { get; set; }
        public string? Code { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public bool? IsActive { get; set; } = true;
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool? IsDeleted { get; set; } = false;
        public List<LookupDto> Reviewers { get; set; } = new();
        public List<LookupDto> Teachers { get; set; } = new();
        public List<LookupDto> Languages { get; set; } = new();
        public List<LookupDto> Subjects { get; set; } = new();
        public List<LookupDto> AcademicAreas { get; set; } = new();
        public List<LookupDto> Grades { get; set; } = new();
        public List<LookupDto> Courses { get; set; } = new();
        public List<LookupDto> AcademicPeriods { get; set; } = new();

        public CampusDto() { }

        public CampusDto(Campus entity) : base(entity)
        {

        }

        public override void InitializateData() { }

        protected override void ExtraMapFromEntity(Campus entity)
        {
            //if (entity.Users is ICollection<User>)
            //   this.Reviewrs = entity.Users.Select(t => new UserDto(t)).ToList();

        }

        protected override void ExtraMapToEntity(Campus entity) { }
    }
}
