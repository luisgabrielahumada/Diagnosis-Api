using Application.Dto;
using Application.Dto.Filters;
using Application.Dto.Filters.Curricunlum;
using Application.Dto.Mcc;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Shared.Configuration;
using Shared.Extensions;
using Shared.MapperModel;
using Shared.Pagination;
using Shared.Response;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using static Shared.Constants;
namespace Infrastructure.Services
{
    public interface ICurriculumFrameworkService
    {
        Task<ServiceResponse<MainCurriculumFrameworkInboxDto>> GetAllAsync(int pageIndex, int pageSize, MainCurriculumFrameworkInfoDto filter = null);
        Task<ServiceResponse<MainCurriculumFrameworkDto>> GetByIdAsync(Guid id, MainCurriculumFrameworkInfoDto filter);
        Task<ServiceResponse<MainCurriculumFrameworkFileDto>> GenerateAsync(Guid id, MainCurriculumFrameworkInfoDto filter);
        Task<ServiceResponse> ImportAsync(Guid id, ImportMccRequest file);
        Task<ServiceResponse<MainCurriculumFrameworkDto>> SaveAsync(Guid id, MainCurriculumFrameworkDto filter);
    }
    public class ExportRow
    {
        public Guid? Id { get; set; }
        public string Descripcion { get; set; } = "";
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public Guid? PeriodId { get; set; }
        public string? Period { get; set; }
    }
    public class ObjectiveExportRow : ExportRow
    {
        public Guid? UnitId { get; set; }      // EDITABLE (requerido para nuevos)
        public string Unit { get; set; } = ""; // Solo lectura (referencia)
    }
    public class KnowledgeExportRow : ExportRow
    {
        public Guid? ObjectiveId { get; set; } // EDITABLE (requerido para nuevos)
        public string Objective { get; set; } = "";
    }

    public class BasicImportRow
    {
        public Guid? Id { get; set; }
        public string Descripcion { get; set; } = "";
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public Guid? PeriodId { get; set; }
        public string? PeriodName { get; set; }
    }

    public class ObjectiveImportRow : BasicImportRow
    {
        public Guid? UnitId { get; set; }
        public string? Unit { get; set; }
    }

    public class KnowledgeImportRow : BasicImportRow
    {
        public Guid? ObjectiveId { get; set; }
        public string? Objective { get; set; }
        public string? Unit { get; set; }
    }

    public class CurriculumFrameworkService : ICurriculumFrameworkService
    {

        private const string META_SHEET = "_Meta";
        private const string SIGN_NAME = "SYS_SIG";
        private const string VER_NAME = "SYS_VER";
        private const int SIGNATURE_VERSION = 1;
        private static readonly string[] TrackedSheets = new[]
        {
            "Competence",
            "AcademicPerformance",
            "AcademicUnit",
            "AcademicObjective",
            "AcademicEssentialKnowledge"
        };

        private static readonly Dictionary<string, string[]> ExpectedHeaders = new()
        {
            ["Competence"] = new[] { "Id", "Descripcion", "IsActive", "IsDeleted" },
            ["AcademicPerformance"] = new[] { "Id", "Descripcion", "IsActive", "IsDeleted" },
            ["AcademicUnit"] = new[] { "Id", "Descripcion", "IsActive", "IsDeleted" },
            ["AcademicObjective"] = new[] { "Id", "Descripcion", "IsActive", "IsDeleted", "UnitId", "Unit" },
            ["AcademicEssentialKnowledge"] = new[] { "Id", "Descripcion", "IsActive", "IsDeleted", "ObjectiveId", "Objective", "Unit" },
        };
        private readonly ICompetencyService _competence;
        private readonly IAcademicPerformanceService _academicPerformance;
        private readonly IAcademicUnitService _academicUnit;
        private readonly IAcademicObjectiveService _academicObjective;
        private readonly IAcademicEssentialKnowledgeService _academicEssentialKnowledge;
        private readonly IReadRepository<MainCurriculumFramework> _read;
        private readonly IReadRepository<AcademicArea> _readAcademicArea;
        private readonly IReadRepository<Subject> _readSubject;
        private readonly IReadRepository<Language> _readLanguage;
        private readonly IReadRepository<Grade> _readGrade;
        private readonly IReadRepository<AcademicPeriod> _readAcademicPeriod;
        private readonly IWriteRepository<MainCurriculumFrameworkFile> _writeMainCurriculumFrameworkFile;
        private readonly IWriteRepository<MainCurriculumFramework> _write;
        private readonly IIdentityUserService _identityUser;
        private readonly ILoggingService _log;
        public CurriculumFrameworkService(
            ICompetencyService competence,
            IAcademicPerformanceService academicPerformance,
            IAcademicUnitService academicUnit,
            IAcademicObjectiveService academicObjective,
            IAcademicEssentialKnowledgeService academicEssentialKnowledge,
            IReadRepository<AcademicArea> readAcademicArea,
            IReadRepository<Subject> readSubject,
            IReadRepository<Language> readLanguage,
            IReadRepository<Grade> readGrade,
            IReadRepository<AcademicPeriod> readAcademicPeriod,
            IIdentityUserService identityUser,
            IReadRepository<MainCurriculumFramework> read,
            IWriteRepository<MainCurriculumFramework> write,
            IWriteRepository<MainCurriculumFrameworkFile> writeMainCurriculumFrameworkFile,
            ILoggingService log)
        {
            _competence = competence;
            _readAcademicArea = readAcademicArea;
            _readSubject = readSubject;
            _readLanguage = readLanguage;
            _readGrade = readGrade;
            _readAcademicPeriod = readAcademicPeriod;
            _identityUser = identityUser;
            _academicPerformance = academicPerformance;
            _academicUnit = academicUnit;
            _academicObjective = academicObjective;
            _academicEssentialKnowledge = academicEssentialKnowledge;
            _writeMainCurriculumFrameworkFile = writeMainCurriculumFrameworkFile;
            _read = read;
            _write = write;
            _log = log;
        }
        public async Task<ServiceResponse<MainCurriculumFrameworkInboxDto>> GetAllAsync(int pageIndex, int pageSize, MainCurriculumFrameworkInfoDto filter = null)
        {
            var sr = new ServiceResponse<MainCurriculumFrameworkInboxDto>();
            try
            {
                var data = new PaginatedList<MainCurriculumFrameworkDto>();
                var parameters = new PagerParameters(pageIndex, pageSize);
                parameters.SortDirection = "DESC";
                parameters.SortField = "CreatedAt";
                var includes = new[]
                    {
                        nameof(MainCurriculumFramework.AcademicArea),
                        nameof(MainCurriculumFramework.AcademicPeriod),
                        nameof(MainCurriculumFramework.Grade),
                        nameof(MainCurriculumFramework.Subject),
                        nameof(MainCurriculumFramework.Language)
                    };

                Expression<Func<MainCurriculumFramework, bool>> f = x =>
                         (x.Status == filter.Status) &&
                         (!filter.IsActive.HasValue || x.IsActive == filter.IsActive.Value) &&
                         (!filter.IsDeleted.HasValue || x.IsDeleted == filter.IsDeleted.Value) &&
                         (!filter.SubjectId.HasValue || x.SubjectId == filter.SubjectId.Value) &&
                         (!filter.GradeId.HasValue || x.GradeId == filter.GradeId.Value) &&
                         (!filter.AcademicPeriodId.HasValue || x.AcademicPeriodId == filter.AcademicPeriodId.Value) &&
                         (!filter.AcademicAreaId.HasValue || x.AcademicAreaId == filter.AcademicAreaId.Value) &&
                         (!filter.LanguageId.HasValue || x.LanguageId == filter.LanguageId.Value);

                var itemsDB = await _read
                                   .GetPaginationAsync<object>(parameters, includes, f, null);
                if (!itemsDB.Status)
                {
                    sr.AddErrors(itemsDB.Errors);
                    return sr;
                }

                data = new PaginatedList<MainCurriculumFrameworkDto>
                {
                    Count = itemsDB.Data.Count,
                    List = itemsDB.Data?.List
                            .Select(t => new MainCurriculumFrameworkDto
                            {
                                Id = t.Id, // <- cámbialo a t.Id si existe.
                                AcademicArea = t.AcademicArea?.Name ?? string.Empty,
                                AcademicAreaId = t.AcademicArea?.Id ?? null,
                                Color = t.AcademicArea?.Color ?? string.Empty,

                                Subject = t.Subject?.Name ?? string.Empty,
                                SubjectId = t.Subject?.Id ?? null,
                                Course = t.Subject?.Alias ?? string.Empty,

                                Language = t.Language?.Name ?? string.Empty,
                                LanguageId = t.Language?.Id ?? null,

                                Grade = t.Grade?.Name ?? string.Empty,
                                GradeId = t.Grade?.Id ?? null,

                                AcademicPeriod = t.AcademicPeriod?.Name ?? string.Empty,
                                AcademicPeriodId = t.AcademicPeriod?.Id ?? null,

                                CreatedAt = t.CreatedAt.ToString(),
                                UpdatedAt = t.UpdatedAt?.ToString()
                            })
                            .ToList() ?? new List<MainCurriculumFrameworkDto>()
                };
                //Expression<Func<MainCurriculumFramework, bool>> predicateCount = x => x.Status == filter.Status;

                var curriculums = await _read.GetAllAsync(
                        includes: includes,
                        predicate: null
                    );

                if (!curriculums.Status)
                {
                    sr.AddErrors(curriculums.Errors);
                    return sr;
                }
                var allStatus = Enum.GetNames(typeof(CurriculumStatus));
                var statusWithQuantity = curriculums
                    .Data
                    //.Where(t => t.IsDeleted.Value == filter.IsDeleted)
                    .GroupBy(p => p.Status.ToLower()) // Normaliza el casing
                    .ToDictionary(g => g.Key, g => g.Count());
                var status = allStatus.ToDictionary(
                    s => s.ToLower(),
                    s => statusWithQuantity.ContainsKey(s.ToLower()) ? statusWithQuantity[s.ToLower()] : 0
                );

                sr.Data = new MainCurriculumFrameworkInboxDto
                {
                    Curriculum = data,
                    Status = status
                };

            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }
            return sr;
        }
        public async Task<ServiceResponse<MainCurriculumFrameworkDto>> GetByIdAsync(Guid id, MainCurriculumFrameworkInfoDto filter)
        {
            var sr = new ServiceResponse<MainCurriculumFrameworkDto>();
            try
            {
                var includes = new[]
                {
                    nameof(MainCurriculumFramework.AcademicArea),
                    nameof(MainCurriculumFramework.Subject),
                    nameof(MainCurriculumFramework.Language),
                    nameof(MainCurriculumFramework.Grade),
                    nameof(MainCurriculumFramework.AcademicPeriod),
                    nameof(MainCurriculumFramework.MainCurriculumFrameworkFile)
                };
                var itemDB = await _read.GetByIdAsync(id, includes);
                if (!itemDB.Status)
                {
                    sr.AddErrors(itemDB.Errors);
                    return sr;
                }

                var competencies = await _competence.GetAllAsync(1, int.MaxValue, new CompetenceInfoDto
                {
                    AcademicAreaId = filter.AcademicAreaId,
                    AcademicPeriodId = filter.AcademicPeriodId,
                    GradeId = filter.GradeId,
                    LanguageId = filter.LanguageId,
                    SubjectId = filter.SubjectId,
                    IsActive = true,
                    IsDeleted = false
                });

                if (!competencies.Status)
                {
                    sr.AddErrors(competencies.Errors);
                    competencies = new ServiceResponse<PaginatedList<CompetenceDto>> { Data = new PaginatedList<CompetenceDto> { Count = 0, List = new List<CompetenceDto>() } };
                }

                var academicPerformances = await _academicPerformance.GetAllAsync(1, int.MaxValue, new AcademicPerformanceInfoDto
                {
                    AcademicAreaId = filter.AcademicAreaId,
                    AcademicPeriodId = filter.AcademicPeriodId,
                    GradeId = filter.GradeId,
                    LanguageId = filter.LanguageId,
                    SubjectId = filter.SubjectId,
                    IsActive = true,
                    IsDeleted = false
                });

                if (!academicPerformances.Status)
                {
                    sr.AddErrors(academicPerformances.Errors);
                    academicPerformances = new ServiceResponse<PaginatedList<AcademicPerformanceDto>> { Data = new PaginatedList<AcademicPerformanceDto> { Count = 0, List = new List<AcademicPerformanceDto>() } };
                }

                var academicUnits = await _academicUnit.GetAllAsync(1, int.MaxValue, new AcademicUnitInfoDto
                {
                    AcademicAreaId = filter.AcademicAreaId,
                    AcademicPeriodId = filter.AcademicPeriodId,
                    GradeId = filter.GradeId,
                    LanguageId = filter.LanguageId,
                    SubjectId = filter.SubjectId,
                    IsActive = true,
                    IsDeleted = false
                });

                if (!academicUnits.Status)
                {
                    sr.AddErrors(academicUnits.Errors);
                    academicUnits = new ServiceResponse<PaginatedList<AcademicUnitDto>> { Data = new PaginatedList<AcademicUnitDto> { Count = 0, List = new List<AcademicUnitDto>() } };
                }



                var data = new MainCurriculumFrameworkDto
                {
                    Id = id,
                    AcademicArea = itemDB.Data.AcademicArea.Name,
                    AcademicAreaId = filter.AcademicAreaId.Value,
                    Color = itemDB.Data.AcademicArea.Color,
                    Subject = itemDB.Data.Subject.Name,
                    SubjectId = filter.SubjectId.Value,
                    Language = itemDB.Data.Language.Name,
                    LanguageId = filter.LanguageId.Value,
                    Grade = itemDB.Data.Grade.Name,
                    GradeId = filter.GradeId.Value,
                    Course = itemDB.Data.Subject.Alias,
                    AcademicPeriod = itemDB.Data.AcademicPeriod.Name,
                    AcademicPeriodId = filter.AcademicPeriodId.Value,
                    Status = itemDB.Data.Status.ToString(),
                    CreatedAt = itemDB.Data.CreatedAt.ToString(),
                    IsActive = itemDB.Data.IsActive,
                    IsDeleted = itemDB.Data.IsDeleted,
                    UpdatedAt = itemDB.Data.UpdatedAt == null ? string.Empty : itemDB.Data.UpdatedAt?.ToString(),
                    Competencies = competencies.Data.List.ToList(),
                    AcademicPerformances = academicPerformances.Data.List.ToList(),
                    AcademicUnits = academicUnits.Data.List.ToList(),
                    MainCurriculumFrameworkFile = MappingHelper.MapEntityListToMapperModelList<MainCurriculumFrameworkFile, MainCurriculumFrameworkFileDto>(itemDB.Data.MainCurriculumFrameworkFile ?? new List<MainCurriculumFrameworkFile>())
                };

                sr.Data = data;

            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }
            return sr;
        }
        public async Task<ServiceResponse> ImportAsync(Guid id, ImportMccRequest req)
        {
            var sr = new ServiceResponse();
            if (req.File == null || req.File.Length == 0)
            {
                sr.AddError("Error", "El archivo está vacío.");
                return sr;
            }
            var includes = new[]
            {
                nameof(MainCurriculumFramework.MainCurriculumFrameworkFile)
            };
            var resp = await _read.GetByIdAsync(id, includes);
            if (!resp.Status)
            {
                sr.AddErrors(resp.Errors);
                return sr;
            }

            if (resp.Data.LanguageId != req.LanguageId
                || resp.Data.GradeId != req.GradeId
                || resp.Data.SubjectId != req.SubjectId
                || resp.Data.AcademicAreaId != req.AcademicAreaId
                || resp.Data.AcademicPeriodId != req.AcademicPeriodId)
            {
                sr.AddError("Error", "El curriculum no corresponde a los datos de Lenguage, Grado, Asignatura, Area, Periodo que ustede desea cargar.");
                return sr;
            }

            using var ms = new MemoryStream();
            await req.File.CopyToAsync(ms);
            ms.Position = 0;

            using var wb = new XLWorkbook(ms);
            if (!TryValidateSystemSignature(wb, SecuritySettings.ExportSignatureSecret, out var why))
            {
                sr.AddError("ValidateError", $"El archivo no es válido o fue alterado: {why}. " +
                            "Por favor, expórtalo nuevamente desde el sistema.");
                return sr;
            }

            // Parse sheets -> rows
            var compRows = ParseBasicSheet(wb, "Competence");
            var perfRows = ParseBasicSheet(wb, "AcademicPerformance");
            var unitRows = ParseBasicSheet(wb, "AcademicUnit");
            var objRows = ParseObjectivesSheet(wb, "AcademicObjective");
            var knowRows = ParseKnowledgesSheet(wb, "AcademicEssentialKnowledge");

            var mccFile = resp
                .Data
                .MainCurriculumFrameworkFile?
                .Where(t => t.FileName.Equals(req.File.FileName, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();

            if (mccFile is not MainCurriculumFrameworkFile)
            {
                sr.AddError("El archivo cargado es invalido");
                return sr;
            }
            mccFile.ImportCount += 1;
            await _writeMainCurriculumFrameworkFile.UpdateAsync(_identityUser.UserEmail, mccFile);

            try
            {
                foreach (var r in compRows)
                {
                    try
                    {
                        var entity = new CompetenceDto
                        {
                            Id = r.Id,
                            Description = r.Descripcion,
                            IsActive = r.IsActive,
                            IsDeleted = r.IsDeleted,
                            AcademicAreaId = req.AcademicAreaId,
                            LanguageId = req.LanguageId,
                            GradeId = req.GradeId,
                            SubjectId = req.SubjectId,
                            AcademicPeriodId = r.PeriodId ?? req.AcademicPeriodId
                        };

                        var result = await _competence.SaveAsync(r.Id, entity);
                        if (!result.Status)
                        {
                            sr.AddError(result.Errors.FirstOrDefault().ErrorMessage);
                        }
                    }
                    catch (Exception ex)
                    {
                        sr.AddError(ex);
                    }
                }

                foreach (var r in perfRows)
                {
                    try
                    {
                        var entity = new AcademicPerformanceDto
                        {
                            Id = r.Id,
                            Description = r.Descripcion,
                            IsActive = r.IsActive,
                            IsDeleted = r.IsDeleted,
                            AcademicAreaId = req.AcademicAreaId,
                            LanguageId = req.LanguageId,
                            GradeId = req.GradeId,
                            SubjectId = req.SubjectId,
                            AcademicPeriodId = r.PeriodId ?? req.AcademicPeriodId
                        };

                        var result = await _academicPerformance.SaveAsync(r.Id, entity);
                        if (!result.Status)
                        {
                            sr.AddError(result.Errors.FirstOrDefault().ErrorMessage);
                        }
                    }
                    catch (Exception ex)
                    {
                        sr.AddError(ex);
                    }
                }

                foreach (var r in unitRows)
                {
                    try
                    {
                        var entity = new AcademicUnitDto
                        {
                            Id = r.Id,
                            Description = r.Descripcion,
                            IsActive = r.IsActive,
                            IsDeleted = r.IsDeleted,
                            AcademicAreaId = req.AcademicAreaId,
                            LanguageId = req.LanguageId,
                            GradeId = req.GradeId,
                            SubjectId = req.SubjectId,
                            AcademicPeriodId = r.PeriodId ?? req.AcademicPeriodId
                        };

                        var result = await _academicUnit.SaveAsync(r.Id, entity);
                        if (!result.Status)
                        {
                            sr.AddError(result.Errors.FirstOrDefault().ErrorMessage);
                        }
                        if (result.Data is Guid)
                        {
                            objRows = objRows
                                   .Select(o =>
                                       {
                                           if (string.Equals(o.Unit?.Trim(), r.Descripcion, StringComparison.OrdinalIgnoreCase))
                                               o.UnitId = (Guid)result.Data;
                                           return o;
                                       })
                                   .ToList();
                        }

                    }
                    catch (Exception ex)
                    {
                        sr.AddError(ex);
                    }
                }

                foreach (var r in objRows.Where(t => t.UnitId is Guid))
                {
                    try
                    {
                        var entity = new AcademicObjectiveDto
                        {
                            Id = r.Id,
                            Description = r.Descripcion,
                            IsActive = r.IsActive,
                            IsDeleted = r.IsDeleted,
                            AcademicUnitId = r.UnitId, // Editable
                        };

                        var result = await _academicObjective.SaveAsync(r.Id, entity);
                        if (!result.Status)
                        {
                            sr.AddError(result.Errors.FirstOrDefault().ErrorMessage);
                        }

                        if (result.Data is Guid)
                        {

                            knowRows = knowRows
                                   .Select(o =>
                                   {
                                       if (string.Equals(o.Objective?.Trim(), r.Descripcion, StringComparison.OrdinalIgnoreCase))
                                           o.ObjectiveId = (Guid)result.Data;
                                       return o;
                                   })
                                   .ToList();
                        }
                    }
                    catch (Exception ex)
                    {
                        sr.AddError(ex);
                    }
                }

                foreach (var r in knowRows.Where(t => t.ObjectiveId is Guid))
                {
                    try
                    {
                        var entity = new AcademicEssentialKnowledgeDto
                        {
                            Id = r.Id,
                            Description = r.Descripcion,
                            IsActive = r.IsActive,
                            IsDeleted = r.IsDeleted,
                            AcademicObjectiveId = r.ObjectiveId // Editable
                        };

                        var result = await _academicEssentialKnowledge.SaveAsync(r.Id, entity);
                        if (!result.Status)
                        {
                            sr.AddError(result.Errors.FirstOrDefault().ErrorMessage);
                        }
                    }
                    catch (Exception ex)
                    {
                        sr.AddError(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }

            return sr;
        }
        private static List<BasicImportRow> ParseBasicSheet(XLWorkbook wb, string sheetName)
        {
            var list = new List<BasicImportRow>();
            var ws = wb.Worksheets.FirstOrDefault(s => s.Name.Equals(sheetName, StringComparison.OrdinalIgnoreCase));
            if (ws == null) return list;

            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
            if (lastRow < 2) return list;

            for (int row = 2; row <= lastRow; row++)
            {
                var idText = ws.Cell(row, 1)?.GetString()?.Trim();
                var descText = ws.Cell(row, 2)?.GetString() ?? "";
                var actText = ws.Cell(row, 3)?.GetString() ?? "";
                var delText = ws.Cell(row, 4)?.GetString() ?? "";
                var periodIdText = ws.Cell(row, 5)?.GetString()?.Trim();
                var periodTxt = ws.Cell(row, 6)?.GetString();

                // fila totalmente vacía ⇒ saltar
                if (descText.IsNullOrWhiteSpace())
                    continue;

                Guid? id = null;
                Guid? periodId = null;
                if (Guid.TryParse(idText, out var g)) id = g;
                if (Guid.TryParse(periodIdText, out var p)) periodId = p;

                list.Add(new BasicImportRow
                {
                    Id = id,
                    Descripcion = Normalize(descText),
                    IsActive = ParseYesNo(actText),
                    IsDeleted = ParseYesNo(delText),
                    PeriodId = periodId,
                    PeriodName = periodTxt
                });
            }
            return list;
        }
        private static List<ObjectiveImportRow> ParseObjectivesSheet(XLWorkbook wb, string sheetName)
        {
            var list = new List<ObjectiveImportRow>();
            var ws = wb.Worksheets.FirstOrDefault(s => s.Name.Equals(sheetName, StringComparison.OrdinalIgnoreCase));
            if (ws == null) return list;

            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
            if (lastRow < 2) return list;

            for (int row = 2; row <= lastRow; row++)
            {
                var idText = ws.Cell(row, 1)?.GetString()?.Trim();
                var descText = ws.Cell(row, 2)?.GetString() ?? "";
                var actText = ws.Cell(row, 3)?.GetString() ?? "";
                var delText = ws.Cell(row, 4)?.GetString() ?? "";
                var unitIdText = ws.Cell(row, 5)?.GetString()?.Trim(); // puede venir de fórmula
                var unitText = ws.Cell(row, 6)?.GetString();

                // fila vacía total ⇒ saltar
                if (descText.IsNullOrWhiteSpace() && unitText.IsNullOrWhiteSpace())
                    continue;

                Guid? id = null;
                if (Guid.TryParse(idText, out var g1)) id = g1;

                Guid? unitId = null;
                if (Guid.TryParse(unitIdText, out var g2)) unitId = g2;

                list.Add(new ObjectiveImportRow
                {
                    Id = id,
                    Descripcion = Normalize(descText),
                    IsActive = ParseYesNo(actText),
                    IsDeleted = ParseYesNo(delText),
                    UnitId = unitId,
                    Unit = unitText
                });
            }
            return list;
        }
        private static List<KnowledgeImportRow> ParseKnowledgesSheet(XLWorkbook wb, string sheetName)
        {
            var list = new List<KnowledgeImportRow>();
            var ws = wb.Worksheets.FirstOrDefault(s => s.Name.Equals(sheetName, StringComparison.OrdinalIgnoreCase));
            if (ws == null) return list;

            var lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;
            if (lastRow < 2) return list;

            for (int row = 2; row <= lastRow; row++)
            {
                var idText = ws.Cell(row, 1)?.GetString()?.Trim();
                var descText = ws.Cell(row, 2)?.GetString() ?? "";
                var actText = ws.Cell(row, 3)?.GetString() ?? "";
                var delText = ws.Cell(row, 4)?.GetString() ?? "";

                var objIdTxt = ws.Cell(row, 5)?.GetString()?.Trim(); // fórmula
                var objText = ws.Cell(row, 6)?.GetString();
                var unitText = ws.Cell(row, 7)?.GetString();

                if (descText.IsNullOrWhiteSpace() && objText.IsNullOrWhiteSpace())
                    continue;

                Guid? id = null;
                if (Guid.TryParse(idText, out var g1)) id = g1;

                Guid? objectiveId = null;
                if (Guid.TryParse(objIdTxt, out var g2)) objectiveId = g2;

                list.Add(new KnowledgeImportRow
                {
                    Id = id,
                    Descripcion = Normalize(descText),
                    IsActive = ParseYesNo(actText),
                    IsDeleted = ParseYesNo(delText),
                    ObjectiveId = objectiveId,
                    Objective = objText,
                    //Unit = unitText
                });
            }
            return list;
        }
        public async Task<ServiceResponse<MainCurriculumFrameworkFileDto>> GenerateAsync(Guid id, MainCurriculumFrameworkInfoDto filter)
        {
            var sr = new ServiceResponse<MainCurriculumFrameworkFileDto>();
            try
            {

                var itemsPeriods = await _readAcademicPeriod.GetAllAsync(null, predicate: null, orderBy: x => x.OrderBy(y => y.CreatedAt));
                if (!itemsPeriods.Status)
                {
                    sr.AddErrors(itemsPeriods.Errors);
                    return sr;
                }

                var itemsCompetence = await _competence.GetAllAsync(1, int.MaxValue, new CompetenceInfoDto
                {
                    AcademicAreaId = filter.AcademicAreaId,
                    AcademicPeriodId = filter.AcademicPeriodId,
                    GradeId = filter.GradeId,
                    LanguageId = filter.LanguageId,
                    SubjectId = filter.SubjectId
                });
                if (!itemsCompetence.Status)
                {
                    sr.AddErrors(itemsCompetence.Errors);
                    return sr;
                }

                var itemsAcademicPerformance = await _academicPerformance.GetAllAsync(1, int.MaxValue, new AcademicPerformanceInfoDto
                {
                    AcademicAreaId = filter.AcademicAreaId,
                    AcademicPeriodId = filter.AcademicPeriodId,
                    GradeId = filter.GradeId,
                    LanguageId = filter.LanguageId,
                    SubjectId = filter.SubjectId
                });
                if (!itemsAcademicPerformance.Status)
                {
                    sr.AddErrors(itemsAcademicPerformance.Errors);
                    return sr;
                }

                var itemsAcademicUnit = await _academicUnit.GetAllAsync(1, int.MaxValue, new AcademicUnitInfoDto
                {
                    AcademicAreaId = filter.AcademicAreaId,
                    AcademicPeriodId = filter.AcademicPeriodId,
                    GradeId = filter.GradeId,
                    LanguageId = filter.LanguageId,
                    SubjectId = filter.SubjectId
                });
                if (!itemsAcademicUnit.Status)
                {
                    sr.AddErrors(itemsAcademicUnit.Errors);
                    return sr;
                }

                var periods = (itemsPeriods.Data ?? new List<AcademicPeriod>())
                                .OrderBy(p => p.IsActive)
                                .Select(p => new ExportRow
                                {
                                    Id = p.Id,
                                    Descripcion = p.Name ?? "",
                                    IsActive = p.IsActive ?? true,
                                    IsDeleted = p.IsDeleted ?? false,
                                })
                                .ToList();

                var units = itemsAcademicUnit.Data.List ?? new List<AcademicUnitDto>();

                var competencies = (itemsCompetence.Data.List ?? new List<CompetenceDto>())
                    .OrderBy(x => x.Id)
                    .Select(x => new ExportRow
                    {
                        Id = x.Id,
                        Descripcion = x.Description ?? "",
                        IsActive = x.IsActive ?? true,
                        IsDeleted = x.IsDeleted ?? false,
                        PeriodId = x.AcademicPeriodId,
                        Period = x.AcademicPeriod?.Name
                    })
                    .ToList();

                var academicPerformance = (itemsAcademicPerformance.Data.List ?? new List<AcademicPerformanceDto>())
                    .OrderBy(x => x.Id)
                    .Select(x => new ExportRow
                    {
                        Id = x.Id,
                        Descripcion = x.Description ?? "",
                        IsActive = x.IsActive ?? true,
                        IsDeleted = x.IsDeleted ?? false,
                        PeriodId = x.AcademicPeriodId,
                        Period = x.AcademicPeriod?.Name
                    })
                    .ToList();

                var academicUnit = units
                    .OrderBy(x => x.Id)
                    .Select(x => new ExportRow
                    {
                        Id = x.Id,
                        Descripcion = x.Description ?? x.Name ?? "",
                        IsActive = x.IsActive ?? true,
                        IsDeleted = x.IsDeleted ?? false,
                        PeriodId = x.AcademicPeriodId,
                        Period = x.AcademicPeriod?.Name
                    })
                    .ToList();

                // 2) Objetivos (relación con Unidad) — UnitId editable
                var objetivos = units
                    .OrderBy(u => u.Id)
                    .SelectMany(u => (u.AcademicObjectives ?? Enumerable.Empty<AcademicObjectiveDto>())
                        .OrderBy(o => o.Id)
                        .Select(o => new ObjectiveExportRow
                        {
                            Id = o.Id,
                            Descripcion = o.Description ?? "",
                            IsActive = o.IsActive ?? true,
                            IsDeleted = o.IsDeleted ?? false,
                            UnitId = u.Id,
                            Unit = u.Description ?? u.Name ?? ""
                        }))
                    .ToList();

                // 3) Conocimientos (relación con Objetivo) — ObjectiveId editable
                var conocimientos = units
                                    .SelectMany(u => (u.AcademicEssentialKnowledges ?? Enumerable.Empty<AcademicEssentialKnowledgeDto>()))
                                    .OrderBy(x => x.Id)
                                    .Select(x => new KnowledgeExportRow
                                    {
                                        Id = x.Id,
                                        Descripcion = x.Description ?? "",
                                        IsActive = x.IsActive ?? true,
                                        IsDeleted = x.IsDeleted ?? false,
                                        ObjectiveId = x.AcademicObjectiveId,
                                        Objective = x.AcademicObjective?.Description ?? ""
                                    })
                                    .ToList();


                // 4) Añadir filas "plantilla" para nuevas altas (deja Id en blanco)
                const int TEMPLATE_ROWS = 5;
                void AppendTemplates<T>(List<T> list, Func<T> factory)
                {
                    for (int i = 0; i < TEMPLATE_ROWS; i++) list.Add(factory());
                }

                AppendTemplates(competencies, () => new ExportRow
                {
                    Descripcion = "",
                    IsActive = true,
                    IsDeleted = false
                });
                AppendTemplates(academicPerformance, () => new ExportRow
                {
                    Descripcion = "",
                    IsActive = true,
                    IsDeleted = false
                });
                AppendTemplates(academicUnit, () => new ExportRow
                {
                    Descripcion = "",
                    IsActive = true,
                    IsDeleted = false
                });
                AppendTemplates(objetivos, () => new ObjectiveExportRow
                {
                    Descripcion = "",
                    IsActive = true,
                    IsDeleted = false,
                    UnitId = null,
                    Unit = ""
                });
                AppendTemplates(conocimientos, () => new KnowledgeExportRow
                {
                    Descripcion = "",
                    IsActive = true,
                    IsDeleted = false,
                    ObjectiveId = null,
                    Objective = ""
                });

                // 5) Excel
                using var wb = new XLWorkbook();
                AddBasicSheet(wb, "AcademicPeriod", periods, includePeriod: false, isHidden: true);
                AddBasicSheet(wb, "Competence", competencies, includePeriod: true);
                AddBasicSheet(wb, "AcademicPerformance", academicPerformance, includePeriod: true);
                AddBasicSheet(wb, "AcademicUnit", academicUnit, includePeriod: true);
                AddObjectivesSheet(wb, "AcademicObjective", objetivos);
                AddKnowledgesSheet(wb, "AcademicEssentialKnowledge", conocimientos);
                AddSystemSignature(wb, SecuritySettings.ExportSignatureSecret);
                using var ms = new MemoryStream();
                wb.SaveAs(ms);
                var fileName = $"export_{DateTime.Now.ToString("yyyyMMddhhmmss")}.xlsx";
                await _writeMainCurriculumFrameworkFile.AddAsync(_identityUser.UserEmail, new MainCurriculumFrameworkFile
                {
                    FileName = fileName,
                    IsActive = true,
                    IsDeleted = false,
                    MainCurriculumFrameworkId = id
                });
                sr.Data = new MainCurriculumFrameworkFileDto
                {
                    Content = ms.ToArray(),
                    FileName = $"export_{DateTime.Now.ToString("yyyyMMddhhmmss")}.xlsx"
                };
            }
            catch (Exception ex)
            {
                sr.AddError(ex);
                _log.LogErrorAsync("Export", ex.ToString(), userName: _identityUser.UserEmail);
            }
            return sr;
        }
        private static void AddBasicSheet(XLWorkbook wb, string sheetName, List<ExportRow> rows, bool includePeriod = false, bool isHidden = false)
        {
            var ws = wb.Worksheets.Add(SafeSheetName(sheetName));

            ws.Cell(1, 1).Value = "Id";
            ws.Cell(1, 2).Value = "Descripcion";
            ws.Cell(1, 3).Value = "IsActive";
            ws.Cell(1, 4).Value = "IsDeleted";

            if (includePeriod)
            {
                ws.Cell(1, 5).Value = "PeriodId"; // calculada
                ws.Cell(1, 6).Value = "Period";   // editable (dropdown)
                ws.Column(5).Style.NumberFormat.Format = "@"; // GUID como texto
            }

            ws.Column(3).Style.NumberFormat.Format = "@";
            ws.Column(4).Style.NumberFormat.Format = "@";

            for (int i = 0; i < rows.Count; i++)
            {
                var r = rows[i];
                int row = i + 2;
                ws.Cell(row, 1).Value = r.Id?.ToString() ?? "";
                ws.Cell(row, 2).Value = r.Descripcion ?? "";
                ws.Cell(row, 3).Value = r.IsActive ? "TRUE" : "FALSE";
                ws.Cell(row, 4).Value = r.IsDeleted ? "TRUE" : "FALSE";
                if (includePeriod)
                {
                    ws.Cell(row, 5).Value = r.PeriodId?.ToString() ?? "";
                    ws.Cell(row, 6).Value = r.Period ?? "";
                }
            }

            var lastRow = Math.Max(2, rows.Count + 1);
            var lastCol = includePeriod ? 6 : 4;

            var table = ws.Range(1, 1, lastRow, lastCol).CreateTable();
            table.Theme = XLTableTheme.TableStyleMedium9;
            if (sheetName.Equals("AcademicPeriod", StringComparison.OrdinalIgnoreCase))
            {
                table.Name = "AcademicPeriodTbl";

                var idField = table.Fields.First(f => f.Name.Equals("Id", StringComparison.OrdinalIgnoreCase));
                var descField = table.Fields.First(f => f.Name.Equals("Descripcion", StringComparison.OrdinalIgnoreCase));

                var idCol = table.DataRange.Column(idField.Name);
                var descCol = table.DataRange.Column(descField.Name);

                AddOrReplaceNamedRange(wb, "Period_Id", idCol.AsRange());
                AddOrReplaceNamedRange(wb, "Period_Desc", descCol.AsRange());
            }
            // 👉 Si es la hoja de Unidades, nombra tabla y expone rangos
            if (sheetName.Equals("AcademicUnit", StringComparison.OrdinalIgnoreCase))
            {
                table.Name = "AcademicUnitTbl";

                // Busca los fields por nombre
                var idField = table.Fields.First(f => f.Name.Equals("Id", StringComparison.OrdinalIgnoreCase));
                var descField = table.Fields.First(f => f.Name.Equals("Descripcion", StringComparison.OrdinalIgnoreCase));

                // Toma las columnas desde table.DataRange por índice del field
                var idCol = table.DataRange.Column(idField.Name);      // IXLRangeColumn
                var descCol = table.DataRange.Column(descField.Name);    // IXLRangeColumn

                // Crea/actualiza rangos nombrados a nivel libro
                AddOrReplaceNamedRange(wb, "Unit_Id", idCol.AsRange());
                AddOrReplaceNamedRange(wb, "Unit_Desc", descCol.AsRange());
            }

            var header = ws.Range(1, 1, 1, 4);
            header.Style.Font.Bold = true;
            header.Style.Fill.PatternType = XLFillPatternValues.Solid;
            header.Style.Fill.BackgroundColor = XLColor.FromHtml("#000");
            header.Style.Font.FontColor = XLColor.White;
            header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            ws.Column(2).Style.Alignment.WrapText = true;
            ws.Columns(1, lastCol).AdjustToContents();

            // Validación booleana (texto)
            var boolList = GetBoolTextListRange(wb);
            var dvRange = ws.Range(2, 3, lastRow, 4);
            var dv = dvRange.SetDataValidation();
            dv.AllowedValues = XLAllowedValues.List;
            dv.InCellDropdown = true;
            dv.IgnoreBlanks = true;
            dv.List(boolList);
            if (includePeriod)
            {
                var periodDesc = wb.NamedRanges.FirstOrDefault(n => n.Name.Equals("Period_Desc", StringComparison.OrdinalIgnoreCase));
                if (periodDesc != null)
                {
                    var dvPeriod = ws.Range(2, 6, lastRow, 6).SetDataValidation();
                    dvPeriod.AllowedValues = XLAllowedValues.List;
                    dvPeriod.InCellDropdown = true;
                    dvPeriod.IgnoreBlanks = true;
                    dvPeriod.List(periodDesc.Ranges.First());

                    for (int row = 2; row <= lastRow; row++)
                    {
                        // PeriodId = INDEX(Period_Id, MATCH(TRIM(Period), Period_Desc, 0))
                        ws.Cell(row, 5).FormulaA1 =
                            $"IF(F{row}=\"\",\"\",IFERROR(INDEX(Period_Id, MATCH(TRIM(F{row}), Period_Desc, 0)), \"\"))";
                    }
                }
            }
            // Protección
            ws.CellsUsed().Style.Protection.Locked = true;
            ws.Column(2).Style.Protection.SetLocked(false);
            ws.Column(3).Style.Protection.SetLocked(false);
            ws.Column(4).Style.Protection.SetLocked(false);
            if (includePeriod)
            {
                ws.Column(5).Style.Protection.SetLocked(false);
                ws.Column(6).Style.Protection.SetLocked(false);
            }
            ws.Protect("proteccion-basica");
            if (isHidden)
                ws.Visibility = XLWorksheetVisibility.VeryHidden;
        }
        private static void AddOrReplaceNamedRange(XLWorkbook wb, string name, IXLRange range)
        {
            var existing = wb.NamedRanges.FirstOrDefault(n => n.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (existing != null) existing.Delete();
            range.AddToNamed(name, XLScope.Workbook);
        }
        private static void AddObjectivesSheet(XLWorkbook wb, string sheetName, List<ObjectiveExportRow> rows)
        {
            var ws = wb.Worksheets.Add(SafeSheetName(sheetName));

            ws.Cell(1, 1).Value = "Id";
            ws.Cell(1, 2).Value = "Descripcion";
            ws.Cell(1, 3).Value = "IsActive";
            ws.Cell(1, 4).Value = "IsDeleted";
            ws.Cell(1, 5).Value = "UnitId";   // fórmula (solo lectura)
            ws.Cell(1, 6).Value = "Unit";     // editable por dropdown

            ws.Column(3).Style.NumberFormat.Format = "@";
            ws.Column(4).Style.NumberFormat.Format = "@";
            ws.Column(5).Style.NumberFormat.Format = "@";

            for (int i = 0; i < rows.Count; i++)
            {
                var r = rows[i];
                int row = i + 2;
                ws.Cell(row, 1).Value = r.Id?.ToString() ?? "";
                ws.Cell(row, 2).Value = r.Descripcion ?? "";
                ws.Cell(row, 3).Value = r.IsActive ? "TRUE" : "FALSE";
                ws.Cell(row, 4).Value = r.IsDeleted ? "TRUE" : "FALSE";
                ws.Cell(row, 6).Value = r.Unit ?? "";
            }

            var lastRow = Math.Max(2, rows.Count + 1);

            var table = ws.Range(1, 1, lastRow, 6).CreateTable();
            table.Theme = XLTableTheme.TableStyleMedium9;
            table.Name = "AcademicObjectiveTbl";

            // Exponer rangos para Conocimientos (por índice, no field.DataRange)
            var fId = table.Fields.First(f => f.Name.Equals("Id", StringComparison.OrdinalIgnoreCase));
            var fDesc = table.Fields.First(f => f.Name.Equals("Descripcion", StringComparison.OrdinalIgnoreCase));
            var fUnit = table.Fields.First(f => f.Name.Equals("Unit", StringComparison.OrdinalIgnoreCase));

            var objIdCol = table.DataRange.Column(fId.Name);
            var objDescCol = table.DataRange.Column(fDesc.Name);
            var objUnitCol = table.DataRange.Column(fUnit.Name);

            AddOrReplaceNamedRange(wb, "Objective_Id", objIdCol.AsRange());
            AddOrReplaceNamedRange(wb, "Objective_Desc", objDescCol.AsRange());
            AddOrReplaceNamedRange(wb, "Objective_UnitName", objUnitCol.AsRange());

            var header = ws.Range(1, 1, 1, 6);
            header.Style.Font.Bold = true;
            header.Style.Fill.PatternType = XLFillPatternValues.Solid;
            header.Style.Fill.BackgroundColor = XLColor.FromHtml("#000");
            header.Style.Font.FontColor = XLColor.White;
            header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            ws.Column(2).Style.Alignment.WrapText = true;
            ws.Columns(1, 6).AdjustToContents();

            // Dropdown de unidades (col 6) desde Unit_Desc
            var unitDesc = wb.NamedRanges.FirstOrDefault(n => n.Name.Equals("Unit_Desc", StringComparison.OrdinalIgnoreCase));
            if (unitDesc != null)
            {
                var dvUnit = ws.Range(2, 6, lastRow, 6).SetDataValidation();
                dvUnit.AllowedValues = XLAllowedValues.List;
                dvUnit.InCellDropdown = true;
                dvUnit.IgnoreBlanks = true;
                dvUnit.List(unitDesc.Ranges.First());

                // UnitId (col 5) = INDEX(Unit_Id, MATCH(Unit, Unit_Desc, 0))
                for (int row = 2; row <= lastRow; row++)
                {
                    var formula = $"IF(F{row}=\"\",\"\",IFERROR(INDEX(Unit_Id, MATCH(TRIM(F{row}), Unit_Desc, 0)), \"\"))";
                    ws.Cell(row, 5).FormulaA1 = formula;
                }
            }

            // Validación booleana
            var boolList = GetBoolTextListRange(wb);
            var dvBool = ws.Range(2, 3, lastRow, 4).SetDataValidation();
            dvBool.AllowedValues = XLAllowedValues.List;
            dvBool.InCellDropdown = true;
            dvBool.IgnoreBlanks = true;
            dvBool.List(boolList);

            // Protección: 5 bloqueada (fórmula), 6 editable
            ws.CellsUsed().Style.Protection.Locked = true;
            ws.Column(2).Style.Protection.SetLocked(false);
            ws.Column(3).Style.Protection.SetLocked(false);
            ws.Column(4).Style.Protection.SetLocked(false);
            ws.Column(6).Style.Protection.SetLocked(false);
            ws.Protect("proteccion-objetivos");
        }
        private static void AddKnowledgesSheet(XLWorkbook wb, string sheetName, List<KnowledgeExportRow> rows)
        {
            var ws = wb.Worksheets.Add(SafeSheetName(sheetName));

            ws.Cell(1, 1).Value = "Id";
            ws.Cell(1, 2).Value = "Descripcion";
            ws.Cell(1, 3).Value = "IsActive";
            ws.Cell(1, 4).Value = "IsDeleted";
            ws.Cell(1, 5).Value = "ObjectiveId"; // fórmula (solo lectura)
            ws.Cell(1, 6).Value = "Objective";   // editable (dropdown por descripción)
            //ws.Cell(1, 7).Value = "Unit";        // fórmula (solo lectura)

            ws.Column(3).Style.NumberFormat.Format = "@";
            ws.Column(4).Style.NumberFormat.Format = "@";
            ws.Column(5).Style.NumberFormat.Format = "@"; // GUID texto

            for (int i = 0; i < rows.Count; i++)
            {
                var r = rows[i];
                int row = i + 2;

                ws.Cell(row, 1).Value = r.Id?.ToString() ?? "";
                ws.Cell(row, 2).Value = r.Descripcion ?? "";
                ws.Cell(row, 3).Value = r.IsActive ? "TRUE" : "FALSE";
                ws.Cell(row, 4).Value = r.IsDeleted ? "TRUE" : "FALSE";
                ws.Cell(row, 6).Value = r.Objective ?? ""; // el usuario elige aquí
                                                           // Unit (7) se calcula por fórmula; no escribir valor
            }

            var lastRow = Math.Max(2, rows.Count + 1);

            var table = ws.Range(1, 1, lastRow, 6).CreateTable();
            table.Theme = XLTableTheme.TableStyleMedium9;

            var header = ws.Range(1, 1, 1, 6);
            header.Style.Font.Bold = true;
            header.Style.Fill.PatternType = XLFillPatternValues.Solid;
            header.Style.Fill.BackgroundColor = XLColor.FromHtml("#000000");
            header.Style.Font.FontColor = XLColor.White;
            header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            ws.Column(2).Style.Alignment.WrapText = true;
            ws.Columns(1, 6).AdjustToContents();

            // ===== Dropdown de Objective (col 6) y fórmulas =====
            var objDesc = wb.NamedRanges.FirstOrDefault(n => n.Name.Equals("Objective_Desc", StringComparison.OrdinalIgnoreCase));
            var objId = wb.NamedRanges.FirstOrDefault(n => n.Name.Equals("Objective_Id", StringComparison.OrdinalIgnoreCase));
            // var objUnit = wb.NamedRanges.FirstOrDefault(n => n.Name.Equals("Objective_UnitName", StringComparison.OrdinalIgnoreCase));

            if (objDesc != null && objId != null)
            {
                // Dropdown por descripción
                var dvObj = ws.Range(2, 6, lastRow, 6).SetDataValidation();
                dvObj.AllowedValues = XLAllowedValues.List;
                dvObj.InCellDropdown = true;
                dvObj.IgnoreBlanks = true;
                dvObj.List(objDesc.Ranges.First());

                // ObjectiveId (col 5) desde descripción
                for (int row = 2; row <= lastRow; row++)
                {
                    var fId = $"IF(F{row}=\"\",\"\",IFERROR(INDEX(Objective_Id, MATCH(TRIM(F{row}), Objective_Desc, 0)), \"\"))";
                    ws.Cell(row, 5).FormulaA1 = fId;

                    //// Unit (col 7) usando ObjectiveId (más robusto si hay descripciones duplicadas)
                    //var fUnit = $"IF(E{row}=\"\",\"\",IFERROR(INDEX(Objective_UnitName, MATCH(E{row}, Objective_Id, 0)), \"\"))";
                    //ws.Cell(row, 7).FormulaA1 = fUnit;
                }
            }

            // Validación booleana
            var boolList = GetBoolTextListRange(wb);
            var dvBool = ws.Range(2, 3, lastRow, 4).SetDataValidation();
            dvBool.AllowedValues = XLAllowedValues.List;
            dvBool.InCellDropdown = true;
            dvBool.IgnoreBlanks = true;
            dvBool.List(boolList);

            // Protección: 5 y 7 bloqueadas (fórmula), 2/3/4/6 editables
            ws.CellsUsed().Style.Protection.Locked = true;
            ws.Column(2).Style.Protection.SetLocked(false);
            ws.Column(3).Style.Protection.SetLocked(false);
            ws.Column(4).Style.Protection.SetLocked(false);
            ws.Column(6).Style.Protection.SetLocked(false);
            ws.Protect("proteccion-conocimientos");
        }
        private static string SafeSheetName(string name)
        {
            // Evita caracteres inválidos y nombres largos
            var invalid = new[] { '\\', '/', '*', '[', ']', ':', '?' };
            foreach (var ch in invalid)
                name = name.Replace(ch, '-');

            if (name.Length > 31) name = name.Substring(0, 31);
            if (string.IsNullOrWhiteSpace(name)) name = "Sheet";
            return name;
        }
        private static string Normalize(string? s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            s = s.Trim();
            // Collapse multiple whitespace to single space
            return Regex.Replace(s, "\\s+", " ");
        }
        private static bool ParseYesNo(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return false;
            var s = raw.Trim().ToLowerInvariant();

            // Spanish/English & typical inputs from Excel validation
            if (s is "sí" or "si" or "true" or "verdadero" or "yes" or "1" or "x") return true;
            if (s is "no" or "false" or "falso" or "0") return false;

            // If Excel stored as boolean instead of text, handle via fallback
            if (bool.TryParse(raw, out var b)) return b;

            // Default conservative: false
            return false;
        }
        private static IXLRange GetBoolTextListRange(XLWorkbook wb)
        {
            var ws = wb.Worksheets.FirstOrDefault(s => s.Name == "_Refs")
                     ?? wb.Worksheets.Add("_Refs");

            // Texto explícito (no booleanos): "TRUE" / "FALSE"
            ws.Cell("A1").SetValue("TRUE");
            ws.Cell("A2").SetValue("FALSE");

            var rng = ws.Range("A1:A2");
            if (!wb.NamedRanges.Any(n => n.Name.Equals("BoolListText", StringComparison.OrdinalIgnoreCase)))
                rng.AddToNamed("BoolListText", XLScope.Workbook);

            ws.Visibility = XLWorksheetVisibility.VeryHidden;
            return rng; // _Refs!A1:A2
        }
        private static string ComputeHmac(string payload, string secret)
        {
            using var h = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var bytes = h.ComputeHash(Encoding.UTF8.GetBytes(payload));
            return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
        }
        private static string BuildSignaturePayload(XLWorkbook wb, string token /* o null si quieres firma estable */)
        {
            var sb = new StringBuilder();
            sb.Append("v=").Append(SIGNATURE_VERSION);

            if (!string.IsNullOrEmpty(token))
                sb.Append("|token=").Append(token);

            foreach (var kv in ExpectedHeaders)
            {
                var sheetName = kv.Key;
                var expected = kv.Value;
                var ws = wb.Worksheets.FirstOrDefault(s =>
                             s.Name.Equals(sheetName, StringComparison.OrdinalIgnoreCase));
                // Si falta hoja, también firma la ausencia (que falle la validación)
                sb.Append("|").Append(sheetName).Append(":");

                for (int i = 0; i < expected.Length; i++)
                {
                    var cellHeader = ws?.Cell(1, i + 1).GetString();
                    sb.Append(Clean(cellHeader)).Append(",");
                }
            }
            return sb.ToString();
        }
        private static void AddSystemSignature(XLWorkbook wb, string secret)
        {
            // Crea/obtiene _Meta
            var ws = wb.Worksheets.FirstOrDefault(s => s.Name == META_SHEET)
                     ?? wb.Worksheets.Add(META_SHEET);

            ws.Cell(1, 1).Value = "Key";
            ws.Cell(1, 2).Value = "Value";

            ws.Cell(2, 1).Value = "SignatureVersion";
            ws.Cell(2, 2).Value = SIGNATURE_VERSION;

            ws.Cell(3, 1).Value = "GeneratedAtUtc";
            ws.Cell(3, 2).Value = DateTime.UtcNow.ToString("o");

            ws.Cell(4, 1).Value = "Token";
            var token = Guid.NewGuid().ToString();
            ws.Cell(4, 2).Value = token;

            ws.Cell(5, 1).Value = "HmacSha256";
            var payload = BuildSignaturePayload(wb, token);
            var hmac = ComputeHmac(payload, secret);
            ws.Cell(5, 2).Value = hmac;

            // Named ranges para validación rápida
            AddOrReplaceNamedRange(wb, SIGN_NAME, ws.Cell(5, 2).AsRange());
            AddOrReplaceNamedRange(wb, VER_NAME, ws.Cell(2, 2).AsRange());

            // Oculta a prueba de curiosos
            ws.Visibility = XLWorksheetVisibility.VeryHidden;
        }
        private static bool TryValidateSystemSignature(XLWorkbook wb, string secret, out string? reason)
        {
            reason = null;

            var ws = wb.Worksheets.FirstOrDefault(s => s.Name == META_SHEET);
            if (ws == null) { reason = "Falta hoja _Meta"; return false; }

            var verStr = ws.Cell(2, 2).GetString().Trim();
            var genAt = ws.Cell(3, 2).GetString().Trim(); // opcional por auditoría
            var token = ws.Cell(4, 2).GetString().Trim();
            var sig = ws.Cell(5, 2).GetString().Trim();

            if (!int.TryParse(verStr, out var ver) || ver != SIGNATURE_VERSION)
            { reason = "Versión de firma inválida"; return false; }

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(sig))
            { reason = "Token/firma ausentes"; return false; }

            var payload = BuildSignaturePayload(wb, token);
            var expected = ComputeHmac(payload, secret);

            if (!string.Equals(expected, sig, StringComparison.OrdinalIgnoreCase))
            { reason = "Firma no coincide (estructura alterada)"; return false; }

            return true;
        }
        private static string Clean(string? s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            var x = s.Replace('\u00A0', ' ')   // NBSP → space
                     .Trim();
            // Normaliza Unicode para evitar diacríticos “raros”
            x = x.Normalize(NormalizationForm.FormKC);
            // Compacta múltiples espacios
            x = System.Text.RegularExpressions.Regex.Replace(x, "\\s+", " ");
            return x;
        }

        public async Task<ServiceResponse<MainCurriculumFrameworkDto>> SaveAsync(Guid id, MainCurriculumFrameworkDto data)
        {
            var sr = new ServiceResponse<MainCurriculumFrameworkDto>();
            try
            {
                if (id == Guid.Empty)
                {
                    var user = data.ToEntity();
                    var resp = await _write.AddAsync(_identityUser.UserEmail, user);
                    sr.Data = new MainCurriculumFrameworkDto(resp);
                }
                else
                {
                    var item = await _read.GetByIdAsync(id);
                    if (!item.Status)
                    {
                        sr.AddErrors(item.Errors);
                        return sr;
                    }
                    item.Data.Status = data.Status ?? item.Data.Status;

                    item.Data.UpdatedAt = DateTime.UtcNow;
                    item.Data.IsDeleted = data.IsDeleted;
                    item.Data.IsActive = data.IsActive;
                    await _write.UpdateAsync(_identityUser.UserEmail, item.Data);
                    sr.Data = data;
                }
            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }
            return sr;
        }
    }
}