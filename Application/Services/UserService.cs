using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Infrastructure.Dto;
using Infrastructure.Dto.Filters;
using Microsoft.Extensions.Caching.Memory;
using Shared;
using Shared.Crypto;
using Shared.Extensions;
using Shared.MapperModel;
using Shared.Pagination;
using Shared.Response;
using System.Linq.Expressions;
namespace Infrastructure.Services
{
    public interface IUserService
    {
        Task<ServiceResponse<PaginatedList<UserDto>>> GetCurrentUserAllAsync(int pageIndex, int pageSize, UserInfoDto filter);
        Task<ServiceResponse<PaginatedList<UserDto>>> GetAllAsync(int pageIndex, int pageSize, UserInfoDto filter);
        Task<ServiceResponse<UserDto>> GetByIdAsync(int id);
        Task<ServiceResponse<UserDto>> GetByIdAsync(string login);
        Task<ServiceResponse> InitialConfigurationRoleAsync(Guid id, CampusRoleDto data);
        Task<ServiceResponse<UserDto>> GetByIdAsync(Guid id);
        Task<ServiceResponse<List<UserDto>>> GetAllTeacherAsync();
        Task<ServiceResponse> SaveAsync(Guid id, UserDto data);
        Task<ServiceResponse> DeleteAsync(Guid id);
    }

    public class UserService : IUserService
    {
        private readonly IReadRepository<User> _read;
        private readonly IWriteRepository<User> _write;
        private readonly IWriteRepository<CampusAcademicAreaReviewer> _writeCampusAcademicAreaReviewer;
        private readonly IWriteRepository<CampusSubjectTeacher> _writeCampusSubjectTeacher;
        private readonly IMemoryCache _cache;
        private readonly IIdentityUserService _identityUser;
        private readonly ILoggingService _log;
        public  readonly IUserRepository _userRepository;

        public UserService(IReadRepository<User> read, IMemoryCache cache,
                           IIdentityUserService identityUser, ILoggingService log,
                           IWriteRepository<CampusAcademicAreaReviewer> writeCampusAcademicAreaReviewer,
                           IWriteRepository<CampusSubjectTeacher> writeCampusSubjectTeacher,
                           IWriteRepository<User> write,
                           IUserRepository userRepository
                           )
        {
            _read = read;
            _cache = cache;
            _identityUser = identityUser;
            _log = log;
            _write = write;
            _writeCampusAcademicAreaReviewer = writeCampusAcademicAreaReviewer;
            _writeCampusSubjectTeacher = writeCampusSubjectTeacher;
            _userRepository = userRepository;
        }
        public async Task<ServiceResponse<PaginatedList<UserDto>>> GetCurrentUserAllAsync(int pageIndex, int pageSize, UserInfoDto filter)
        {
            var sr = new ServiceResponse<PaginatedList<UserDto>>();
            try
            {
                var parameters = new PagerParameters(pageIndex, pageSize);
                var includes = new[]
                {
                    nameof(Role)
                };

                bool isAdminUser = false;
                if (!string.IsNullOrWhiteSpace(filter.Text))
                {
                    var currentUser = await GetByIdAsync(filter.Text);
                    if (!currentUser.Status)
                    {
                        sr.AddErrors(currentUser.Errors);
                        return sr;
                    }
                    isAdminUser = currentUser.Data.Role.Code == UserRole.Admin;
                }

                Expression<Func<User, bool>> predicate = x =>
                (!filter.IsActive.HasValue || x.IsActive == filter.IsActive.Value)
             && (!filter.IsDeleted.HasValue || x.IsDeleted == filter.IsDeleted.Value)


             && (isAdminUser
                    ? (
                        filter.UserRole == null
                        || filter.UserRole.Length == 0
                        || filter.UserRole
                            .Select(r => r.ToString())
                            .Contains(x.Role.Code)
                      )
                    : x.Login == filter.Text
                );
                var items = await _read.GetPaginationAsync<object>(parameters, includes, predicate, orderBy: q => q.CreatedAt);

                if (!items.Status)
                {
                    return new ServiceResponse<PaginatedList<UserDto>>
                    {
                        Errors = items.Errors
                    };
                }

                // 4) Mapear a DTO y devolver
                var data = new PaginatedList<UserDto>
                {
                    Count = items.Data.Count,
                    List = MappingHelper
                                .MapEntityListToMapperModelList<User, UserDto>(items.Data.List)
                };
                sr.Data = data;
            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }

            return sr;
        }
        public async Task<ServiceResponse<PaginatedList<UserDto>>> GetAllAsync(int pageIndex, int pageSize, UserInfoDto filter)
        {
            var sr = new ServiceResponse<PaginatedList<UserDto>>();
            try
            {
                var parameters = new PagerParameters(pageIndex, pageSize);
                parameters.SortDirection = "ASC";
                parameters.SortField = "NAME";
                var includes = new[]
                {
                    nameof(Role)
                };


                Expression<Func<User, bool>> predicate = x =>
                (!filter.IsDeleted.HasValue || x.IsDeleted == filter.IsDeleted.Value)
                && (string.IsNullOrEmpty(filter.Text) || x.Name.Contains(filter.Text))
                && (string.IsNullOrEmpty(filter.Text) || x.Email.Contains(filter.Text))
                && (string.IsNullOrEmpty(filter.Text) || x.Login.Contains(filter.Text));


                var items = await _read.GetPaginationAsync<object>(parameters, includes, predicate, orderBy: q => q.CreatedAt);

                if (!items.Status)
                {
                    return new ServiceResponse<PaginatedList<UserDto>>
                    {
                        Errors = items.Errors
                    };
                }

                // 4) Mapear a DTO y devolver
                var data = new PaginatedList<UserDto>
                {
                    Count = items.Data.Count,
                    List = MappingHelper
                                .MapEntityListToMapperModelList<User, UserDto>(items.Data.List)
                };
                sr.Data = data;
            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }

            return sr;
        }
        public async Task<ServiceResponse<UserDto>> GetByIdAsync(int id)
        {
            var sr = new ServiceResponse<UserDto>();
            try
            {

                var item = await _read
                                    .GetByIdAsync(id, new[] { nameof(User.Role) });
                if (!item.Status)
                {
                    sr.AddErrors(item.Errors);
                    return sr;
                }

                sr.Data = new UserDto(item.Data);

            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }
            return sr;
        }
        public async Task<ServiceResponse<UserDto>> GetByIdAsync(Guid id)
        {
            var sr = new ServiceResponse<UserDto>();
            try
            {

                var item = await _read
                                    .GetByIdAsync(id, new[] { nameof(User.Role) });
                if (!item.Status)
                {
                    sr.AddErrors(item.Errors);
                    return sr;
                }

                sr.Data = new UserDto(item.Data);

            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }
            return sr;
        }
        public async Task<ServiceResponse<UserDto>> GetByIdAsync(string login)
        {
            var sr = new ServiceResponse<UserDto>();
            try
            {
                var includes = new[]
                {
                    nameof(User.Role)
                };

                var itemDB = await _read.GetAllAsync(includes, predicate: t => t.Login == login
                                                            && t.IsActive.Value == true);
                if (!itemDB.Status)
                {
                    sr.AddErrors(itemDB.Errors);
                    return sr;
                }

                if (itemDB.Data is not List<User>)
                {
                    sr.AddError("Error", $"El usuario {login} no existe");
                    return sr;
                }

                sr.Data = new UserDto(itemDB.Data.FirstOrDefault());

            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }
            return sr;
        }
        public async Task<ServiceResponse> InitialConfigurationRoleAsync(Guid id, CampusRoleDto data)
        {
            var sr = new ServiceResponse();
            try
            {
                var userId = id;
                var userResp = await _read.GetByIdAsync(userId, new[]
                {
                    nameof(User.Role)
                });
                if (!userResp.Status)
                {
                    sr.AddErrors(userResp.Errors);
                    return sr;
                }
                var user = userResp.Data!;

                //if (data.CampusId == Guid.Empty ||
                //    (user.Role.Code == UserRole.Planning_Advisor.ToString() && data.AcademicAreaId == Guid.Empty) ||
                //    (user.Role.Code == UserRole.Teacher.ToString() && data.SubjectId == Guid.Empty))
                //{
                //    sr.AddError("Los datos de configuración inicial están incompletos.");
                //    return sr;
                //}

                var applied = false;

                if (user.Role.Code == UserRole.Planning_Advisor.ToString())
                {
                    foreach (var item in data.AcademicAreas ?? Array.Empty<Guid>())
                    {
                        await _writeCampusAcademicAreaReviewer.AddAsync(new CampusAcademicAreaReviewer
                        {
                            CampusId = data.CampusId,
                            AcademicAreaId = item,
                            ReviewerId = userId
                        });
                    }


                    applied = true;
                }

                if (user.Role.Code == UserRole.Teacher.ToString())
                {
                    foreach (var item in data.Subjects ?? Array.Empty<Guid>())
                    {
                        await _writeCampusSubjectTeacher.AddAsync(new CampusSubjectTeacher
                        {
                            CampusId = data.CampusId,
                            SubjectId = item,
                            TeacherId = userId
                        });
                    }

                    applied = true;
                }

                if (applied)
                {
                    user.UpdatedAt = DateTime.UtcNow;
                    await _write.UpdateAsync(_identityUser.UserEmail, user);
                }
            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }
            return sr;
        }
        public async Task<ServiceResponse<List<UserDto>>> GetAllTeacherAsync()
        {
            var sr = new ServiceResponse<List<UserDto>>();
            try
            {
                var currentUser = await GetByIdAsync(_identityUser.UserId.Value);
                if (!currentUser.Status)
                {
                    sr.AddErrors(currentUser.Errors);
                    return sr;
                }
                var isAdminUser = currentUser.Data.Role.Code == UserRole.Admin;
                
                var items = await _userRepository.GetUsersByReviewerIdOrAllIfAdmin(isAdminUser, currentUser.Data.Id.Value);
                if (!items.Status)
                {
                    sr.AddErrors(items.Errors);
                    return sr;
                }

                // 4) Mapear a DTO y devolver
                var data = MappingHelper
                            .MapEntityListToMapperModelList<User, UserDto>(items.Data);
                sr.Data = data;
            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }

            return sr;
        }
        public async Task<ServiceResponse> SaveAsync(Guid id, UserDto data)
        {
            var sr = new ServiceResponse();
            try
            {
                if (id == Guid.Empty)
                {
                    var entity = data.ToEntity();
                    entity.PasswordHash = data.PasswordHash.AesEncrypt(Constants.AesKey);
                    entity.Login = data.Email;
                    await _write.AddAsync(_identityUser.UserEmail, entity);
                }
                else
                {
                    var getResp = await _read.GetByIdAsync(id);
                    if (!getResp.Status)
                    {
                        sr.AddErrors(getResp.Errors);
                        return sr;
                    }
                   
                    var entity = getResp.Data;
                   
                    entity.Name = data.Name;
                    entity.Email = data.Email;
                    entity.PasswordHash = data.PasswordHash ?? entity.PasswordHash;
                    entity.RoleId = data.RoleId;
                    //entity.Avatar = data.Avatar;
                    entity.Phone = data.Phone;
                    entity.IsActive = data.IsActive;
                    entity.UpdatedAt = DateTime.UtcNow;
                    await _write.UpdateAsync(_identityUser.UserEmail, entity);
                }
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

                var item = await _read.GetByIdAsync(id);
                if (!item.Status)
                {
                    sr.AddErrors(item.Errors);
                    return sr;
                }
                item.Data.IsDeleted = !item.Data.IsDeleted;
                await _write.UpdateAsync(_identityUser.UserEmail, item.Data);
            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }
            return sr;
        }

    }
}