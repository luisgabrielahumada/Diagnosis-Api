using Application.Dtos;
using Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Shared.Response;
using Web.Api.Controllers;
using static Shared.Constants;

namespace Application.Tests.Controllers
{
    public class DiagnosisControllerTests
    {
        [Fact]
        public async Task Detect_ShouldReturn200_WhenZombie()
        {
            var dto = new PatientDiagnosisRequestDto
            {
                PatientId = Guid.Parse("52BFF1AA-CA68-49A9-AA20-000B8050B581"),
                GeneticCode = new List<string>
                {
                    "PLAGGP",
                    "APGLGP",
                    "LLALGL",
                    "APLAPL",
                    "PPPPLA",
                    "LAPLGG"
                }
            };

            var mock = new Mock<IDiagnosisService>();
            mock.Setup(x => x.CreateDiagnosisAsync(DiagnosisType.Zombie, dto))
                .ReturnsAsync(new ServiceResponse<PatientDiagnosisResponseDto>
                {
                    Errors = new List<ServiceError>(),
                    Data = new PatientDiagnosisResponseDto { Infected = true }
                });

            var controller = new DiagnosisController(mock.Object);

            IActionResult result = await controller.CreateDiagnosis(dto, DiagnosisType.Zombie);
            var objectResult = result as ObjectResult;
            int? status = objectResult?.StatusCode;
            Assert.Equal(StatusCodes.Status200OK, status);
        }

        [Fact]
        public async Task Detect_ShouldReturn403_WhenHuman()
        {
            var dto = new PatientDiagnosisRequestDto
            {
                PatientId = Guid.Parse("52BFF1AA-CA68-49A9-AA20-000B8050B581"),
                GeneticCode = new List<string>
                {
                    "PLAGGP",
                    "APGLGP",
                    "LLALGL",
                    "APLAPL",
                    "GAPLAG",
                    "LAPLGG"
                }
            };

            var mock = new Mock<IDiagnosisService>();
            mock.Setup(x => x.CreateDiagnosisAsync(DiagnosisType.Zombie, dto))
                .ReturnsAsync(new ServiceResponse<PatientDiagnosisResponseDto>
                {
                    Errors = new List<ServiceError>(),
                    Data = new PatientDiagnosisResponseDto { Infected = false }
                });

            var controller = new DiagnosisController(mock.Object);

            var result = await controller.CreateDiagnosis(dto, DiagnosisType.Zombie);
            var objectResult = result as ObjectResult;
            int? status = objectResult?.StatusCode;
            Assert.Equal(StatusCodes.Status403Forbidden, status);
        }
    }
}
