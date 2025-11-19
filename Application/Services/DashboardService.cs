using Application.Dto.Common;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Infrastructure.Dto;
using Infrastructure.Dto.Filters;
using Microsoft.Extensions.Caching.Memory;
using Shared;
using Shared.Configuration;
using Shared.Response;
using static Shared.Constants;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Infrastructure.Services
{
    public interface IDashboardService
    {
        //Task<ServiceResponse<DashboardStatsDto>> GetDashboardStatsAsync();
    }

    public class DashboardService : IDashboardService
    {
        //private readonly IPlanningService _readPlanning;
        //private readonly ICampusService _readCampus;
        //private readonly IAcademicAreaService _readAcademicArea;
        //private readonly ISubjectsService _readSubject;
        private readonly IReadRepository<AuditLog> _readAuditLog;
        private readonly IMemoryCache _cache;
        private readonly IIdentityUserService _identityUser;
        private readonly INotificationService _noty;
        private readonly IUserService _readUser;

        public DashboardService(
            //ICampusService readCampus,
            //IAcademicAreaService readAcademicArea,
            //ISubjectsService readSubject,
            IReadRepository<AuditLog> readAuditLog,
            IIdentityUserService identityUser,
            IMemoryCache cache,
            //IPlanningService readPlanning,
            INotificationService notificationService,
            //IUserService readUser
            )
        {
            //_readCampus = readCampus;
            //_readAcademicArea = readAcademicArea;
            //_readSubject = readSubject;
            _readAuditLog = readAuditLog;
            _identityUser = identityUser;
            _cache = cache;
            _noty = notificationService;
         //   _readPlanning = readPlanning;
         //   _readUser = readUser;
        }

        //public async Task<ServiceResponse<DashboardStatsDto>> GetDashboardStatsAsync()
        //{
        //    var sr = new ServiceResponse<DashboardStatsDto>();
        //    try
        //    {
        //        var itemsDB = await _readUser.GetByIdAsync(_identityUser.UserId.Value);
        //        if (!itemsDB.Status)
        //        {
        //            sr.AddErrors(itemsDB.Errors);
        //            return sr;
        //        }

        //        var stats = await _cache.GetOrCreateAsync($"{Constants.KeyCache.DashboardStats}_{itemsDB.Data.Role.Code.Value}", async entry =>
        //        {
        //            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(PerformanceSettings.CacheDuration);
        //            var filter = new PlanningInfoDto();
        //            if (itemsDB.Data.Role.Code.Value != UserRole.Admin)
        //            {
        //                filter.Teacher = _identityUser.UserId;
        //            }
        //            filter.IsActive = true;
        //            filter.IsDeleted = false;
        //            // 1) Total de campus activos
        //            var respPlanning = await _readPlanning.GetAllAsync(1, int.MaxValue, filter,true);
        //            var totalPlanning = respPlanning.Data.Planning.Count > 0
        //                ? respPlanning.Data.Planning.Count
        //                : 0;


        //            var totalReviewers = respPlanning.Status
        //                ? respPlanning.Data.Status.FirstOrDefault(t => t.Key == PlanningStatus.InReview.ToString().ToLower()).Value
        //                : 0;

        //            var totalApproved = respPlanning.Status
        //               ? respPlanning.Data.Status.FirstOrDefault(t => t.Key == PlanningStatus.Approved.ToString().ToLower()).Value
        //               : 0;

        //            var totalDraft = respPlanning.Status
        //           ? respPlanning.Data.Status.FirstOrDefault(t => t.Key == PlanningStatus.Draft.ToString().ToLower()).Value
        //           : 0;



        //            // 3) Notificaciones sin leer del usuario actual
        //            var email = _identityUser.UserEmail;
        //            var respNotifs = await _noty.GetNotificationsAsync(email);
        //            var unreadNotifs = respNotifs.Status
        //                ? respNotifs.Data.Count(t => (!t.IsRead.HasValue || !t.IsRead.Value))
        //                : 0;

        //            var readNotifs = respNotifs.Status
        //               ? respNotifs.Data.Count(t => (t.IsRead.HasValue && t.IsRead.Value))
        //               : 0;


        //            var respCampuses = await _readCampus.GetAllAsync(1, 5, new CampusInfoDto { IsActive = true });
        //            var respAreas = await _readAcademicArea.GetAllAsync(1, 5, new AcademicAreaInfoDto { IsActive = true });
        //            var respSubjects = await _readSubject.GetAllAsync(1, 5, new SubjectInfoDto { IsActive = true });
        //            var respActivity = await _readAuditLog.GetAllAsync();
        //            return new DashboardStatsDto
        //            {
        //                CompletedPlans = totalPlanning | 0,
        //                ReviewersPlans = totalReviewers | 0,
        //                ApprovedPlans = totalApproved | 0,
        //                DraftPlans = totalDraft | 0,
        //                RecentActions = unreadNotifs,
        //                ReadActions = readNotifs,
        //                Campus = respCampuses.Status ?
        //                            respCampuses.Data.List.Select(c => new Item { CreatedAt = c.CreatedAt.Value, Name = c.Name, Total = respCampuses.Data.Count }).ToList()
        //                            : new List<Item>(),
        //                Areas = respAreas.Status ?
        //                            respAreas.Data.List.Select(c => new Item { CreatedAt = c.CreatedAt.Value, Name = c.Name, Total = respAreas.Data.Count }).ToList()
        //                            : new List<Item>(),
        //                Subjects = respSubjects.Status ?
        //                            respSubjects.Data.List.Select(c => new Item { CreatedAt = c.CreatedAt.Value, Name = c.Name, Total = respSubjects.Data.Count }).ToList()
        //                            : new List<Item>(),

        //            };
        //        });

        //        sr.Data = stats;
        //    }
        //    catch (Exception ex)
        //    {
        //        sr.AddError(ex);
        //    }
        //    return sr;
        //}
    }
}
