using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Shared.Response;

public class ConfigurationCampusRepository : IConfigurationsRepository
{
    private readonly ApplicationDbContext _ctx;

    public ConfigurationCampusRepository(ApplicationDbContext ctx)
    {
        _ctx = ctx;
    }

    public async Task<ServiceResponse<List<User>>> GetConfigurationAsync(
    Guid campusId,
    Guid subjectId,
    Guid academicAreaId,
    User currentUser)
    {
        var sr = new ServiceResponse<List<User>>();

        try
        {
            var myUserId = currentUser.Id;
            var roleCode = currentUser.Role.Code;
            var isAdmin = roleCode == UserRole.Admin.ToString();
            var isAdvisor = roleCode == UserRole.Planning_Advisor.ToString();

            // 1) Traigo los docentes
            var teachers = await _ctx.Set<CampusSubjectTeacher>()
                .AsNoTracking()
                .Where(st => st.CampusId == campusId && st.SubjectId == subjectId)
                .Select(st => st.Teacher)
                .ToListAsync();

            // 2) Traigo los revisores del área
            var reviewers = await _ctx.Set<CampusAcademicAreaReviewer>()
                .AsNoTracking()
                .Where(ar => ar.CampusId == campusId && ar.AcademicAreaId == academicAreaId)
                .Select(ar => ar.Reviewer)
                .ToListAsync();

            // 3) Compruebo si yo soy revisor de ese campus+área
            var amIReviewer = await _ctx.Set<CampusAcademicAreaReviewer>()
                .AsNoTracking()
                .AnyAsync(ar =>
                    ar.CampusId == campusId &&
                    ar.AcademicAreaId == academicAreaId &&
                    ar.ReviewerId == myUserId);

            // 4) Armo la lista de salida según rol
            IEnumerable<User> candidates;
            if (isAdmin)
            {
                // Admin: todos los docentes + todos los revisores
                candidates = teachers.Concat(reviewers);
            }
            else if (isAdvisor && amIReviewer)
            {
                // Advisor (y soy revisor): solo docentes de esa asignatura + me incluyo a mí
                candidates = teachers.Append(currentUser);
            }
            else
            {
                // Resto de casos (docente normal): solo yo
                candidates = new[] { currentUser };
            }

            // 5) Elimino duplicados y devuelvo
            sr.Data = candidates
                .GroupBy(u => u.Id)
                .Select(g => g.First())
                .ToList();
        }
        catch (Exception ex)
        {
            sr.AddError(ex);
        }

        return sr;
    }


}
