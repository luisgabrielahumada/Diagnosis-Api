using Domain.Enums;
using Shared.Response;
using Application.Dto.Common;

namespace Infrastructure.Services
{
    public interface IEnumService
    {
        ServiceResponse<List<LookupDto>> ToEnumList<T>() where T : Enum;
    }
    public class EnumService : IEnumService
    {
        public ServiceResponse<List<LookupDto>> ToEnumList<T>() where T : Enum
        {
            var sr = new ServiceResponse<List<LookupDto>>();
            try
            {
                var items = Enum.GetValues(typeof(T))
                      .Cast<T>()
                      .Select(e => new LookupDto
                      {
                          Value = Convert.ToInt32(e),
                          Index = e.ToString(),
                          Label = GetTranslation(e)
                      })
                     .ToList();
                sr.Data = items;
            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }
            return sr;

        }

        private string GetTranslation<T>(T enumValue) where T : Enum
        {
            return enumValue switch
            {
                // ReviewStatus
                ReviewStatus.Approved => "Aprobado / Approved",
                ReviewStatus.Rejected => "Rechazado / Rejected",
                ReviewStatus.RequiresChanges => "Requiere cambios / Requires Changes",

                // UserRole
                UserRole.Pending => "Por Asignar / Pending",
                UserRole.Admin => "Administrador / Admin",
                UserRole.Coordinator => "Coordinador / Coordinator",
                UserRole.Planning_Advisor => "Asesor de planificación / Planning Advisor",
                UserRole.Teacher => "Docente / Teacher",
                UserRole.Audit => "Auditor / Audit",

                // NotificationType
                NotificationType.Info => "Información / Info",
                NotificationType.Warning => "Advertencia / Warning",
                NotificationType.Error => "Error / Error",
                NotificationType.Success => "Éxito / Success",
                NotificationType.PlanningSubmitted => "Planificación enviada / Planning Submitted",
                NotificationType.PlanningApproved => "Planificación aprobada / Planning Approved",
                NotificationType.PlanningRejected => "Planificación rechazada / Planning Rejected",
                NotificationType.ReviewAssigned => "Revisión asignada / Review Assigned",
                NotificationType.Reminder => "Recordatorio / Reminder",

                // PeriodType
                PeriodType.Trimester => "Trimestre / Trimester",
                PeriodType.Semester => "Semestre / Semester",
                PeriodType.Quarter => "Cuatrimestre / Quarter",
                PeriodType.Custom => "Personalizado / Custom",

                // PlanningStatus
                PlanningStatus.Draft => "Borrador / Draft",
                PlanningStatus.Submitted => "Enviado / Submitted",
                PlanningStatus.InReview => "En revisión / In Review",
                PlanningStatus.Approved => "Aprobado / Approved",
                PlanningStatus.Rejected => "Rechazado / Rejected",
                PlanningStatus.RequiresChanges => "Requiere cambios / Requires Changes",

                // CompetencyType
                CompetencyType.Specific => "Específica / Specific",
                CompetencyType.General => "General / General",
                CompetencyType.Citizenship => "Ciudadana / Citizenship",
                CompetencyType.Labor => "Laboral / Labor",

                // Fallback
                _ => enumValue.ToString()
            };
        }


    }
}
