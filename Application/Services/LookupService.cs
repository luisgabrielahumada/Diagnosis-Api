using Application.Dto;
using Application.Dto.Common;
using Domain.Enums;
using Infrastructure.Dto;
using Infrastructure.Dto.Filters;
using Microsoft.Extensions.Caching.Memory;
using Shared.Configuration;
using Shared.Pagination;
using Shared.Response;
using static Shared.Constants;
namespace Infrastructure.Services
{
    public interface ILookupService
    {
        Task<ServiceResponse<LookupBundleDto>> GetAllLookupsAsync();
    }

    public class LookupService : ILookupService
    {
        private readonly IAcademicAreaService _areaService;
        private readonly ISubjectsService _subjectService;
        private readonly IGradeService _gradeService;
        private readonly ICampusService _campusService;
        private readonly IAcademicPeriodService _periodService;
        private readonly IEnumService _enumService;
        private readonly ICourseService _courseService;
        private readonly ILanguageService _languageService;
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;
        private readonly IMemoryCache _cache;
        private readonly IIdentityUserService _identityUser;
        public LookupService(IAcademicAreaService area, ISubjectsService subject, IGradeService grade,
            ICampusService campus, IAcademicPeriodService period, IMemoryCache cache,
            IEnumService enumService, ILanguageService languageService, ICourseService courseService,
            IUserService userService, IIdentityUserService identityUser,
            IRoleService roleService)
        {
            _areaService = area;
            _subjectService = subject;
            _gradeService = grade;
            _campusService = campus;
            _periodService = period;
            _cache = cache;
            _enumService = enumService;
            _languageService = languageService;
            _courseService = courseService;
            _userService = userService;
            _identityUser = identityUser;
            _roleService = roleService;
        }
        public async Task<ServiceResponse<LookupBundleDto>> GetAllLookupsAsync()
        {
            var sr = new ServiceResponse<LookupBundleDto>();
            try
            {
                var parameters = new PagerParameters(1, int.MaxValue);
                var currentUser = await _userService.GetByIdAsync(_identityUser.UserEmail);
                if (!currentUser.Status)
                {
                    sr.AddErrors(currentUser.Errors);
                    return sr;
                }
                // Precondiciones:
                var myUserId = currentUser.Data.Id;
                var academicAreas = await _cache.GetOrCreateAsync($"{KeyCache.Areas}", async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(PerformanceSettings.CacheDuration);
                    var result = await _areaService.GetAllAsync(1, int.MaxValue, new AcademicAreaInfoDto { IsActive = true, IsDeleted = false });
                    return result?.Data?.List ?? new List<AcademicAreaDto>();
                });

                var subjects = await _cache.GetOrCreateAsync($"{currentUser.Data.Role.Code}_{KeyCache.Subjects}", async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(PerformanceSettings.CacheDuration);
                    var result = await _subjectService.GetAllAsync(1, int.MaxValue, new SubjectInfoDto { IsActive = true, IsDeleted = false });
                    return result?.Data?.List ?? new List<SubjectDto>();
                });

                var grades = await _cache.GetOrCreateAsync(KeyCache.Grades, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(PerformanceSettings.CacheDuration);
                    var result = await _gradeService.GetAllAsync(1, int.MaxValue, new GradeInfoDto { IsActive = true, IsDeleted = false });
                    return result?.Data?.List ?? new List<GradeDto>();
                });

                var campuses = await _cache.GetOrCreateAsync($"{KeyCache.Campuses}", async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(PerformanceSettings.CacheDuration);
                    var result = await _campusService.GetAllAsync(1, int.MaxValue, new CampusInfoDto { IsActive = true, IsDeleted = false });
                    return result?.Data?.List ?? new List<CampusDto>();
                });

                var academicPeriods = await _cache.GetOrCreateAsync(KeyCache.Periods, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(PerformanceSettings.CacheDuration);
                    var result = await _periodService.GetAllAsync(1, int.MaxValue, new PeriodInfoDto { IsActive = true, IsDeleted = false });
                    return result?.Data?.List ?? new List<AcademicPeriodDto>();
                });

                var allAcademicPeriods = await _cache.GetOrCreateAsync(KeyCache.AllPeriods, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(PerformanceSettings.CacheDuration);
                    var result = await _periodService.GetAllAsync();
                    return result?.Data ?? new List<AcademicPeriodDto>();
                });

                //var competencyType = await _cache.GetOrCreateAsync(KeyCache.CompetencyType, async entry =>
                //{
                //    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(6);
                //    var result = _enumService.ToEnumList<CompetencyType>();
                //    return result?.Data ?? new List<LookupDto>();
                //});

                //var notificationType = await _cache.GetOrCreateAsync(KeyCache.NotificationType, async entry =>
                //{
                //    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(6);
                //    var result = _enumService.ToEnumList<NotificationType>();
                //    return result?.Data ?? new List<LookupDto>();
                //});

                var periodType = await _cache.GetOrCreateAsync(KeyCache.PeriodType, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(PerformanceSettings.CacheDuration);
                    var result = _enumService.ToEnumList<PeriodType>();
                    return result?.Data ?? new List<LookupDto>();
                });

                var planningStatus = await _cache.GetOrCreateAsync(KeyCache.PlanningStatus, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(PerformanceSettings.CacheDuration);
                    var result = _enumService.ToEnumList<PlanningStatus>();
                    return result?.Data ?? new List<LookupDto>();
                });

                var reviewStatus = await _cache.GetOrCreateAsync(KeyCache.ReviewStatus, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(PerformanceSettings.CacheDuration);
                    var result = _enumService.ToEnumList<ReviewStatus>();
                    return result?.Data ?? new List<LookupDto>();
                });

                var userRole = await _cache.GetOrCreateAsync(KeyCache.UserRole, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(PerformanceSettings.CacheDuration);
                    var result = _enumService.ToEnumList<UserRole>();
                    return result?.Data ?? new List<LookupDto>();
                });

                var years = await _cache.GetOrCreateAsync(KeyCache.Years, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(PerformanceSettings.CacheDuration);
                    var result = await GetYearListAsync(2025, 2030); // or use DateTime.Now.Year
                    return result ?? new List<LookupDto>();
                });

                var languages = await _cache.GetOrCreateAsync(KeyCache.Languages, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(PerformanceSettings.CacheDuration);
                    var result = await _languageService.GetAllAsync(1, 100);
                    return result?.Data?.List ?? new List<LanguageDto>();
                });

                var courses = await _cache.GetOrCreateAsync(KeyCache.Courses, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(PerformanceSettings.CacheDuration);
                    var result = await _courseService.GetAllAsync(1, int.MaxValue);
                    return result?.Data?.List ?? new List<CourseDto>();
                });

                var teachers = await _cache.GetOrCreateAsync(KeyCache.Teachers, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(PerformanceSettings.CacheDuration);
                    var result = await _userService.GetAllTeacherAsync();
                    return result?.Data ?? new List<UserDto>();
                });

                var roles = await _cache.GetOrCreateAsync(KeyCache.Roles, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(PerformanceSettings.CacheDuration);
                    var result = await _roleService.GetAllAsync(1, int.MaxValue, new RoleInfoDto { IsActive = true, IsDeleted = false });
                    return result?.Data.List ?? new List<RoleDto>();
                });

                var curriculumStatus = await _cache.GetOrCreateAsync(KeyCache.CurriculumStatus, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(PerformanceSettings.CacheDuration);
                    var result = _enumService.ToEnumList<CurriculumStatus>();
                    return result?.Data ?? new List<LookupDto>();
                });

                //var campusesAllAreas = await _cache.GetOrCreateAsync($"{currentUser.Data.Role.Code}_{KeyCache.CampusesAllAreas}", async entry =>
                //{
                //    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(6);
                //    var result = await _campusService.GetAllWithAreasAsync(1, int.MaxValue);
                //    return result?.Data ?? new List<CampusWithAreasDto>();
                //});
                var configurationSystem = GetConfigurationSystems();


                sr.Data = new LookupBundleDto
                {
                    AcademicAreas = academicAreas.ToList(),
                    Subjects = subjects.ToList(),
                    Grades = grades.ToList(),
                    Campuses = campuses.ToList(),
                    AcademicPeriods = academicPeriods.ToList(),
                    //CompetencyType = competencyType,
                    //NotificationType = notificationType,
                    PeriodType = periodType,
                    PlanningStatus = planningStatus,
                    ReviewStatus = reviewStatus,
                    UserRole = userRole,
                    Years = years,
                    Languages = languages.ToList(),
                    Courses = courses.ToList(),
                    ConfigurationSystem = configurationSystem,
                    Teachers = teachers.ToList(),
                    //CampusesAllAreas= campusesAllAreas.ToList()
                    Roles = roles.ToList(),
                    AllAcademicPeriods = allAcademicPeriods.ToList(),
                     CurriculumStatus= curriculumStatus
                };

            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }
            return sr;
        }

        private async Task<List<LookupDto>> GetYearListAsync(int startYear, int endYear)
        {
            int currentYear = DateTime.UtcNow.Year;
            return await Task.FromResult(
                Enumerable.Range(startYear, endYear - startYear + 1)
                          .Select(year => new LookupDto
                          {
                              Value = year,
                              Label = year == currentYear ? $"{year}*" : year.ToString()
                          })
                          .ToList()
            );
        }

        private List<ConfigurationSystemDto> GetConfigurationSystems()
                         => new List<ConfigurationSystemDto>
                         {
                            new ConfigurationSystemDto
                            {
                                Id = Guid.NewGuid(),
                                Code = "WebSite",
                                Value = GeneralSettings.WebSite,
                                Description = nameof(GeneralSettings.WebSite)
                            },
                            new ConfigurationSystemDto
                            {
                                Id = Guid.NewGuid(),
                                Code = "Environment",
                                Value = GeneralSettings.Environment,
                                Description = nameof(GeneralSettings.Environment)
                            },
                            new ConfigurationSystemDto
                            {
                                Id = Guid.NewGuid(),
                                Code = "SessionTimeout",
                                Value = GeneralSettings.SessionTimeout.ToString(),
                                Description = nameof(GeneralSettings.SessionTimeout)
                            },
                            new ConfigurationSystemDto
                            {
                                Id = Guid.NewGuid(),
                                Code = "EnabledLog",
                                Value = GeneralSettings.EnabledLog.ToString(),
                                Description = nameof(GeneralSettings.EnabledLog)
                            },
                            new ConfigurationSystemDto
                            {
                                Id = Guid.NewGuid(),
                                Code = "CacheDuration",
                                Value = PerformanceSettings.CacheDuration.ToString(),
                                Description = nameof(PerformanceSettings.CacheDuration)
                            }
                         };

    }
}