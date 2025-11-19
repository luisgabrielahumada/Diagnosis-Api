using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Shared;
using Shared.Extensions;
using Shared.Response;


namespace Infrastructure.Services
{
    public interface IWebHookNotificacionService
    {
        Task<ServiceResponse> GePayloadAsync(string payload);
    }

    public class WebHookNotificacionService : IWebHookNotificacionService
    {
        private readonly IMemoryCache _cache;
        private readonly ILoggingService _log;
        private readonly IWriteRepository<WebHookNotificacion> _write;
        public WebHookNotificacionService(IMemoryCache cache, ILoggingService log, IWriteRepository<WebHookNotificacion> write)
        {
            _cache = cache;
            _log = log;
            _write = write;
        }
      

        public async Task<ServiceResponse> GePayloadAsync(string payload)
        {
            var sr = new ServiceResponse();
            try
            {
                if (!payload.IsNotNullOrWhiteSpace())
                {
                    sr.AddError(Errors.ENTITY_NOT_FOUND,Errors.ENTITY_NOT_FOUND_MESSAGE);
                    return sr;
                }
                await _write.AddAsync("WebHookNotificacion", new WebHookNotificacion { Payload = payload });
            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }
            return sr;
        }
    }
}