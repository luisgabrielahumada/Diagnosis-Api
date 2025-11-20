using static Shared.Constants;

namespace Application.DTOs
{
    public class StatsResponseDto
    {
        public string DiagnosisType { get; set; }
        public int CountInfected { get; set; }
        public int CountNotInfected { get; set; }
        public double Ratio { get; set; }
    }
}
