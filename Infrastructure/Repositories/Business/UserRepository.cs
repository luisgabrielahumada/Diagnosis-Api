using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Shared.Response;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _dbContext;

    public UserRepository(ApplicationDbContext ctx)
    {
        _dbContext = ctx;
    }

    public async Task<ServiceResponse<List<User>>> GetUsersByReviewerIdOrAllIfAdmin(bool isAdmin, Guid userId)
    {
        var sr = new ServiceResponse<List<User>>();
        try
        {
            _dbContext.Database.SetCommandTimeout(180);

            var result = await _dbContext.Set<CampusSubjectTeacher>()
                .Join(_dbContext.Set<Subject>(),
                      cst => cst.SubjectId,
                      subject => subject.Id,
                      (cst, subject) => new { cst, subject })
                .Join(_dbContext.Set<AcademicArea>(),
                      cs => cs.subject.AcademicAreaId,
                      academicArea => academicArea.Id,
                      (cs, academicArea) => new { cs, academicArea })
                .Join(_dbContext.Set<CampusAcademicAreaReviewer>(),
                      cs => cs.academicArea.Id,
                      campusReviewer => campusReviewer.AcademicAreaId,
                      (cs, campusReviewer) => new { cs.cs, campusReviewer })
                .Where(c =>
                    isAdmin ||
                    c.campusReviewer.ReviewerId == userId) // Filtro para Admin o ReviewerId
                .Join(_dbContext.Set<User>(),
                      c => c.cs.cst.TeacherId,
                      user => user.Id,
                      (c, user) => new { user.Id, user.Name })
                .Distinct()
                .ToListAsync();

            sr.Data = result.Select(user => new User
            {
                Id = user.Id,
                Name = user.Name
            }).ToList();

            return sr;
        }
        catch (Exception ex)
        {
            sr.AddError(ex);
            return sr;
        }
    }

}
