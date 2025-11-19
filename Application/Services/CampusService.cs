using Application.Dto.Common;
using Application.Services;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Dto;
using Infrastructure.Dto.Filters;
using Microsoft.Extensions.Caching.Memory;
using Shared.Configuration;
using Shared.MapperModel;
using Shared.Pagination;
using Shared.Response;
using System.Linq.Expressions;
using static Shared.Constants;

namespace Infrastructure.Services
{
    public interface ICampusService
    {
        Task<ServiceResponse<PaginatedList<CampusDto>>> GetAllAsync(int pageIndex, int pageSize, CampusInfoDto filter = null);
        Task<ServiceResponse<CampusDto>> GetByIdAsync(Guid id);
        Task<ServiceResponse> SaveAsync(Guid id, CampusDto data);
        Task<ServiceResponse> DeleteAsync(Guid id);
        Task<ServiceResponse<CampusDto>> GetConfigurationAsync(Guid id);
        Task<ServiceResponse<List<LookupDto>>> GetTeachersAsync(Guid id, Guid subjectId, Guid academicAreaId);
    }

    public class CampusService : ICampusService
    {
        private readonly IReadRepository<Campus> _read;
        private readonly IReadRepository<CampusAcademicAreaReviewer> _readCampusAcademicAreaReviewer;
        private readonly IWriteRepository<Campus> _write;
        private readonly IIdentityUserService _identityUser;
        private readonly IMemoryCache _cache;
        private readonly IConfigurationsRepository _queryConfiguration;
        private readonly IUserService _userService;
        private readonly IAppNotifierService _noty;
        public CampusService(
            IReadRepository<Campus> read,
            IWriteRepository<Campus> write,
            IIdentityUserService identityUser,
            IMemoryCache cache,
            IConfigurationsRepository queryConfiguration,
            IUserService userService,
            IAppNotifierService notificationService)
        {
            _read = read;
            _write = write;
            _identityUser = identityUser;
            _cache = cache;
            _queryConfiguration = queryConfiguration;
            _userService = userService;
            _noty = notificationService;
        }

        public async Task<ServiceResponse<PaginatedList<CampusDto>>> GetAllAsync(int pageIndex, int pageSize, CampusInfoDto filter = null)
        {
            var sr = new ServiceResponse<PaginatedList<CampusDto>>();

            try
            {
                var parameters = new PagerParameters(pageIndex, pageSize);
                parameters.SortDirection = "ASC";
                parameters.SortField = "Name";  // Default sorting by CreatedAt
                var text = filter?.Text?.Trim().ToLower();
                //Func<IQueryable<Campus>, IOrderedQueryable<Campus>> orderBy =
                //  q => q.OrderBy(al => al.Name);
                Expression<Func<Campus, bool>> predicate = x =>
                    // texto
                    (string.IsNullOrWhiteSpace(text)
                        || (x.Name != null && x.Name.ToLower().Contains(text))
                        || (x.Code != null && x.Code.ToLower().Contains(text))
                        || (x.Address != null && x.Address.ToLower().Contains(text))
                        || (x.Phone != null && x.Phone.ToLower().Contains(text))
                        || (x.Email != null && x.Email.ToLower().Contains(text)))
                    // fechas
                    && (!filter.StartDate.HasValue || x.CreatedAt.Date >= filter.StartDate.Value.Date)
                    && (!filter.EndDate.HasValue || x.CreatedAt.Date <= filter.EndDate.Value.Date)
                    // flags
                    && (!filter.IsActive.HasValue || (x.IsActive.HasValue && x.IsActive == filter.IsActive.Value))
                    && (!filter.IsDeleted.HasValue || (x.IsDeleted.HasValue && x.IsDeleted == filter.IsDeleted.Value));
                //// solo campus con al menos un usuario
                //&& x.Users.Any();

                //Expression<Func<Campus, object>> orderBy = x => x.CreatedAt;

                var itemsDB = await _read.GetPaginationAsync<object>(
                    parameters: parameters,
                    includes: null,
                    filter: predicate,
                    orderBy: q => q.CreatedAt
                );

                if (!itemsDB.Status)
                {
                    sr.AddErrors(itemsDB.Errors);
                    return sr;
                }

                //var dtos = itemsDB.Data.List
                //    .Select(CampusDto.FromEntity)
                //    .ToList();

                sr.Data = new PaginatedList<CampusDto>
                {
                    Count = itemsDB.Data.Count,
                    List = MappingHelper.MapEntityListToMapperModelList<Campus, CampusDto>(itemsDB.Data.List)
                };
            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }

            return sr;
        }
        public async Task<ServiceResponse<CampusDto>> GetConfigurationAsync(Guid id)
        {
            var sr = new ServiceResponse<CampusDto>();
            try
            {
                //var campus = await _cache.GetOrCreateAsync<Campus>(KeyCache.CampusConfiguration, async entry =>
                //{
                //    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(PerformanceSettings.CacheDuration);

                    var includes = new[]
                    {
                        $"{nameof(Campus.CampusLanguages)}.{nameof(CampusLanguage.Language)}",
                        $"{nameof(Campus.CampusAcademicAreas)}.{nameof(CampusAcademicArea.AcademicArea)}",
                        $"{nameof(Campus.CampusSubjects)}.{nameof(CampusSubject.Subject)}",
                        $"{nameof(Campus.CampusGrades)}.{nameof(CampusGrade.Grade)}",
                        $"{nameof(Campus.CampusCourses)}.{nameof(CampusCourse.Course)}",
                        $"{nameof(Campus.CampusAcademicPeriods)}.{nameof(CampusAcademicPeriod.AcademicPeriod)}",
                    };

                    var resp = await _read.GetByIdAsync(id, includes, asNoTracking: true);
                    if (!resp.Status)
                    {
                        sr.AddErrors(resp.Errors);
                        return sr;
                    }

                //    return resp?.Data ?? new Campus();
                //});

                var dto = new CampusDto(resp.Data);

                // Ordenar y asignar los datos a los DTOs
                dto.Languages = resp.Data.CampusLanguages
                    .OrderBy(cl => cl.Language.Name)
                    .Select(cl => new LookupDto
                    {
                        Id = cl.LanguageId,
                        Label = cl.Language.Name
                    })
                    .ToList();

                dto.AcademicAreas = resp.Data.CampusAcademicAreas
                    .OrderBy(cl => cl.AcademicArea.Name)
                    .Select(ca => new LookupDto
                    {
                        Id = ca.AcademicAreaId,
                        Label = ca.AcademicArea.Name,
                        Description = ca.AcademicArea.Color
                    })
                    .ToList();

                dto.Subjects = resp.Data.CampusSubjects
                    .OrderBy(cl => cl.Subject.Name)
                    .Select(ca => new LookupDto
                    {
                        Id = ca.SubjectId,
                        Label = $"{ca.Subject.Name} {ca.Subject.Alias}",
                        ReferenceId = ca.Subject.AcademicAreaId
                    })
                    .ToList();

                dto.Courses = resp.Data.CampusCourses
                    .OrderBy(cl => cl.Course.Name)
                    .Select(cc => new LookupDto
                    {
                        Id = cc.CourseId,
                        Label = cc.Course.Name
                    })
                    .ToList();

                dto.Grades = resp.Data.CampusGrades
                    .OrderBy(cl => cl.Grade.Name)
                    .Select(cg => new LookupDto
                    {
                        Id = cg.GradeId,
                        Label = cg.Grade.Name
                    })
                    .ToList();

                dto.AcademicPeriods = resp.Data.CampusAcademicPeriods
                    .OrderBy(cp => cp.AcademicPeriod.Name)
                    .Select(cp => new LookupDto
                    {
                        Id = cp.AcademicPeriodId,
                        Label = cp.AcademicPeriod.Name,
                        IsActive = cp.AcademicPeriod.IsActive.GetValueOrDefault()
                    })
                    .ToList();

                sr.Data = dto;
            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }

            return sr;
        }


        public async Task<ServiceResponse<List<LookupDto>>> GetTeachersAsync(Guid id, Guid subjectId, Guid academicAreaId)
        {
            var sr = new ServiceResponse<List<LookupDto>>();
            try
            {

                var currentUserDto = await _userService.GetByIdAsync(_identityUser.UserEmail);
                if (!currentUserDto.Status)
                {
                    sr.AddErrors(currentUserDto.Errors);
                    return sr;
                }
                var currentUser = currentUserDto.Data;
                var user = new User
                {
                    Id = currentUser.Id,
                    Name = currentUser.Name,
                    Email = currentUser.Email,
                    Role = new Role
                    {
                        Code = currentUser.Role.Code.ToString(),
                        Name = currentUser.Role.Name
                    }
                };

                var usersDB = await _queryConfiguration.GetConfigurationAsync(id, subjectId, academicAreaId, user);
                if (!usersDB.Status)
                {
                    sr.AddErrors(usersDB.Errors);
                    return sr;
                };

                sr.Data = usersDB.Data
                    .OrderBy(cl => cl.Name)
                    .Select(cl => new LookupDto
                    {
                        Id = cl.Id,
                        Label = cl.Name
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }
            return sr;
        }

        public async Task<ServiceResponse<CampusDto>> GetByIdAsync(Guid id)
        {
            var sr = new ServiceResponse<CampusDto>();
            try
            {
                var resp = await _read.GetByIdAsync(id);
                if (!resp.Status)
                {
                    sr.AddErrors(resp.Errors);
                    return sr;
                }

                sr.Data = new CampusDto(resp.Data);
            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }
            return sr;
        }

        public async Task<ServiceResponse> SaveAsync(Guid id, CampusDto data)
        {
            var sr = new ServiceResponse();
            try
            {
                if (id == Guid.Empty)
                {
                    // Crear nuevo campus
                    var entity = data.ToEntity();
                    entity.CreatedAt = DateTime.UtcNow;
                    await _write.AddAsync(_identityUser.UserEmail, entity);
                }
                else
                {
                    // Actualizar existente
                    var getResp = await _read.GetByIdAsync(id);
                    if (!getResp.Status)
                    {
                        sr.AddErrors(getResp.Errors);
                        return sr;
                    }

                    var entity = getResp.Data;
                    entity.Name = data.Name;
                    entity.Code = data.Code;
                    entity.Address = data.Address;
                    entity.Phone = data.Phone;
                    entity.Email = data.Email;
                    entity.IsActive = data.IsActive;
                    entity.IsDeleted = data.IsDeleted;
                    entity.UpdatedAt = DateTime.UtcNow;

                    await _write.UpdateAsync(_identityUser.UserEmail, entity);
                }

                _cache.Remove(KeyCache.Campuses);
            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }
            return sr;
        }

        public async Task<ServiceResponse> DeleteAsync(Guid id)
        {
            var sr = new ServiceResponse();
            try
            {
                var getResp = await _read.GetByIdAsync(id);
                if (!getResp.Status)
                {
                    sr.AddErrors(getResp.Errors);
                    return sr;
                }

                var entity = getResp.Data;
                entity.IsDeleted = !entity.IsDeleted;
                entity.UpdatedAt = DateTime.UtcNow;

                await _write.UpdateAsync(_identityUser.UserEmail, entity);
                _cache.Remove(KeyCache.Campuses);
            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }
            return sr;
        }
    }
}
