using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Dto;
using Shared.Extensions;
using Shared.MapperModel;
using Shared.Pagination;
using Shared.Response;
using System.Linq.Expressions;
namespace Infrastructure.Services
{
    public interface IAuditLogService
    {
        Task<ServiceResponse<PaginatedList<AuditLogDto>>> GetAllAsync(Guid id, int pageIndex = 1, int pageSize = 10, string table = null);
    }

    public class AuditLogService : IAuditLogService
    {
        private readonly IReadRepository<AuditLog> _read;
        private readonly IIdentityUserService _identityUser;
        public AuditLogService(IReadRepository<AuditLog> read)
        {
            _read = read;
        }

        public async Task<ServiceResponse<PaginatedList<AuditLogDto>>> GetAllAsync(Guid id, int pageIndex = 1, int pageSize = 10, string table = null)
        {
            var sr = new ServiceResponse<PaginatedList<AuditLogDto>>();
            try
            {
                var parameters = new PagerParameters(pageIndex, pageSize);
                parameters.SortDirection = "DESC";
                parameters.SortField = "Timestamp";
                Expression<Func<AuditLog, bool>> predicate = x =>
                    (x.RecordId == id && x.TableName == table);


                var items = await _read
                                    .GetPaginationAsync<object>(parameters, includes: null, predicate, orderBy: q => q.Timestamp);
                if (!items.Status)
                {
                    return new ServiceResponse<PaginatedList<AuditLogDto>>
                    { Errors = items.Errors };
                }

                var data = new PaginatedList<AuditLogDto>
                {
                    Count = items.Data.Count,
                    List = MappingHelper
                    .MapEntityListToMapperModelList<AuditLog, AuditLogDto>
                        (items.Data.List)
                };

                sr.Data = data;

            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }
            return sr;
        }
    }
}