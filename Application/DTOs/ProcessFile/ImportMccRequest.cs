using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Application.Dto
{
    public class ImportMccRequest
    {
        [Required]
        public IFormFile File { get; set; } = default!;

        // IDs opcionales (puedes ponerlos Required si lo necesitas)
        public Guid? AcademicAreaId { get; set; }
        public Guid? AcademicPeriodId { get; set; }
        public Guid? GradeId { get; set; }
        public Guid? LanguageId { get; set; }
        public Guid? SubjectId { get; set; }
    }
}
