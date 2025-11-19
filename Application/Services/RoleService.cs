using DocumentFormat.OpenXml.Bibliography;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Infrastructure.Dto;
using Infrastructure.Dto.Filters;
using Microsoft.Extensions.Caching.Memory;
using Shared.MapperModel;
using Shared.Pagination;
using Shared.Response;
using System.Linq.Expressions;
namespace Infrastructure.Services
{
    public interface IRoleService
    {
        Task<ServiceResponse<List<UserDto>>> GetUsersByRole(UserRole role);
        Task<ServiceResponse<List<UserDto>>> GetUsersByRoles(params UserRole[] roles);
        Task<ServiceResponse<PaginatedList<RoleDto>>> GetAllAsync(int pageIndex, int pageSize, RoleInfoDto filter);
        Task<ServiceResponse<RoleDto>> GetByIdAsync(Guid id);
        Task<ServiceResponse> SaveAsync(Guid id, RoleDto data);
        Task<ServiceResponse> DeleteAsync(Guid id);
    }

    public class RoleService : IRoleService
    {
        private readonly IReadRepository<Role> _read;
        private readonly IWriteRepository<Role> _write;
        private readonly IMemoryCache _cache;
        private readonly IIdentityUserService _identityUser;
        private readonly ILoggingService _log;
        public RoleService(IReadRepository<Role> read, IWriteRepository<Role> write, IIdentityUserService identityUserService)
        {
            _read = read;
            _write = write;
            _identityUser = identityUserService;
        }

        public async Task<ServiceResponse<List<UserDto>>> GetUsersByRole(UserRole role)
        {
            var sr = new ServiceResponse<List<UserDto>>();
            try
            {
                var includes = new[]
                {
                    "Users"
                };
                Expression<Func<Role, bool>> predicate = x => (x.Code == role.ToString());
                var itemDB = await _read
                                    .GetAllAsync(includes, predicate);
                if (!itemDB.Status)
                {
                    sr.AddErrors(itemDB.Errors);
                    return sr;
                }

                sr.Data = itemDB.Data.SelectMany(r => r.Users)
                                    .Where(u => (u.IsActive.HasValue && u.IsActive.Value))
                                    .Select(u => new UserDto
                                    {
                                        Id = u.Id,
                                        Email = u.Email,
                                        Name = u.Name,
                                        //Campus = new CampusDto(u.Campus)
                                    })
                                    .ToList();

            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }
            return sr;
        }
        public async Task<ServiceResponse<List<UserDto>>> GetUsersByRoles(params UserRole[] roles)
        {
            var sr = new ServiceResponse<List<UserDto>>();

            try
            {
                var includes = new[]
                {
                            "Users"
                };

                var roleCodes = roles.Select(r => r.ToString()).ToList();
                Expression<Func<Role, bool>> predicate = r => roleCodes.Contains(r.Code);

                var itemDB = await _read.GetAllAsync(includes, predicate);

                if (!itemDB.Status)
                {
                    sr.AddErrors(itemDB.Errors);
                    return sr;
                }

                sr.Data = itemDB.Data
                    .SelectMany(r => r.Users)
                    .Where(u => u.IsActive == true)
                    .Select(u => new UserDto
                    {
                        Id = u.Id,
                        Email = u.Email,
                        Name = u.Name,
                        // Descomenta si necesitas el campus mapeado
                        // Campus = new CampusDto(u.Campus)
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }

            return sr;
        }
        public async Task<ServiceResponse<PaginatedList<RoleDto>>> GetAllAsync(int pageIndex, int pageSize, RoleInfoDto filter)
        {
            var sr = new ServiceResponse<PaginatedList<RoleDto>>();
            try
            {
                var parameters = new PagerParameters(pageIndex, pageSize);
                parameters.SortDirection = "ASC";
                parameters.SortField = "Name";
                var items = await _read.GetPaginationAsync<object>(parameters, null, filter: x=> x.IsDeleted ==  filter.IsDeleted, orderBy: q => q.CreatedAt);

                if (!items.Status)
                {
                    return new ServiceResponse<PaginatedList<RoleDto>>
                    {
                        Errors = items.Errors
                    };
                }

                // 4) Mapear a DTO y devolver
                var data = new PaginatedList<RoleDto>
                {
                    Count = items.Data.Count,
                    List = MappingHelper
                                .MapEntityListToMapperModelList<Role, RoleDto>(items.Data.List)
                };
                sr.Data = data;
            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }

            return sr;
        }
        public async Task<ServiceResponse<RoleDto>> GetByIdAsync(Guid id)
        {
            var sr = new ServiceResponse<RoleDto>();
            try
            {

                var item = await _read
                                    .GetByIdAsync(id);
                if (!item.Status)
                {
                    sr.AddErrors(item.Errors);
                    return sr;
                }

                sr.Data = new RoleDto(item.Data);

            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }
            return sr;
        }
        public async Task<ServiceResponse> SaveAsync(Guid id, RoleDto data)
        {
            var sr = new ServiceResponse();
            try
            {
                if (id == Guid.Empty)
                {
                    var entity = data.ToEntity();
                    entity.Code = data.Code.ToString();
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
                    entity.Code = data.Code.ToString();
                    entity.IsEnabledSetting = data.IsEnabledSetting;
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