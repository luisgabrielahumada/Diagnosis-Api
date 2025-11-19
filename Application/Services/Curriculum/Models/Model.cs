namespace Application.Services.Curriculum.Models
{
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
    public sealed class ImportItemSummary
    {
        public string Type { get; init; } = "";
        public int Attempted { get; set; }        // rows parsed from sheet (raw)
        public int FilteredOut { get; set; }      // removed by validation/missing links
        public int Processed { get; set; }        // actually attempted to save (after filters)

        // New vs Update intent (based on incoming Id)
        public int IntendedNew { get; set; }      // rows with Id null/empty in the processed set
        public int IntendedUpdate { get; set; }   // rows with Id present in the processed set

        // Results by intent
        public int NewOk { get; set; }
        public int NewFailed { get; set; }
        public int UpdateOk { get; set; }
        public int UpdateFailed { get; set; }

        // Totals (computed)
        public int SavedOk => NewOk + UpdateOk;
        public int Failed => NewFailed + UpdateFailed;
    }

    public sealed class ImportSummary
    {
        public ImportItemSummary Competences { get; init; } = new() { Type = "Competence" };
        public ImportItemSummary Performances { get; init; } = new() { Type = "AcademicPerformance" };
        public ImportItemSummary Units { get; init; } = new() { Type = "AcademicUnit" };
        public ImportItemSummary Objectives { get; init; } = new() { Type = "AcademicObjective" };
        public ImportItemSummary Knowledges { get; init; } = new() { Type = "AcademicEssentialKnowledge" };

        public int TotalSavedOk =>
            Competences.SavedOk + Performances.SavedOk + Units.SavedOk + Objectives.SavedOk + Knowledges.SavedOk;

        public int TotalFailed =>
            Competences.Failed + Performances.Failed + Units.Failed + Objectives.Failed + Knowledges.Failed;
    }
}
