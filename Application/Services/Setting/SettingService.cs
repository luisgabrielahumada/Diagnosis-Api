using Infrastructure.Dto;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Shared.Extensions;
using Shared.Response;
using System.Reflection;

namespace Infrastructure.Services
{
    public interface ISettingService
    {
        Task<ServiceResponse<ConfigurationDto>> GetAllAsync();
        Task<ServiceResponse<ParameterDto>> GetAsync(string code);
        Task<ServiceResponse<ParameterDto>> GetByIdAsync(int id);
        Task<ServiceResponse> SaveAsync(int id, ParameterDto data);
        Task<ServiceResponse> LoadParametersAsync();
    }

    public class SettingService : ISettingService
    {
        private readonly IReadRepository<Setting> _read;
        private readonly IWriteRepository<Setting> _write;
        private readonly IMemoryCache _cache;
        private readonly IIdentityUserService _identityUser;
        private readonly ILoggingService _log;
        private const string AllSettingsKey = "GlobalConfig";
        private const string ParamKeyPrefix = "ConfigParam_";

        public SettingService(
            IReadRepository<Setting> read,
            IWriteRepository<Setting> write,
            IMemoryCache cache,
            IIdentityUserService identityUser,
            ILoggingService log)
        {
            _read = read;
            _write = write;
            _cache = cache;
            _identityUser = identityUser;
            _log = log;
        }

        public async Task<ServiceResponse<ConfigurationDto>> GetAllAsync()
        {
            var sr = new ServiceResponse<ConfigurationDto>();
            try
            {
                var items = await _read.GetAllAsync();
                if (!items.Status)
                {
                    sr.AddErrors(items.Errors);
                    return sr;
                }

                sr.Data = ConvertToConfiguration(items.Data.ToList());
            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }
            return sr;
        }

        public async Task<ServiceResponse> SaveAsync(int id, ParameterDto data)
        {
            var sr = new ServiceResponse();
            try
            {
                var result = await _read.GetAllAsync(
                    predicate: t => t.Id == id
                );

                if (!result.Status)
                {
                    sr.AddErrors(result.Errors);
                    return sr;
                }

                var setting = result.Data.FirstOrDefault();
                if (setting != null)
                {
                    setting.Value = data.Value.ToString();
                    await _write.UpdateAsync(_identityUser.UserEmail, setting);
                    // Invalidar caché de este parámetro y de todo el conjunto
                    _cache.Remove($"{ParamKeyPrefix}{setting.Code}");
                    _cache.Remove(AllSettingsKey);
                }
            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }
            return sr;
        }

        public async Task<ServiceResponse> LoadParametersAsync()
        {
            var sr = new ServiceResponse();
            try
            {
                // 1) Leer de caché o BD
                if (!_cache.TryGetValue(AllSettingsKey, out List<Setting> parameters))
                {
                    var dbResult = await _read.GetAllAsync();
                    if (!dbResult.Status || dbResult.Data == null)
                    {
                        sr.AddError("Error", "No configuration data found.");
                        return sr;
                    }

                    parameters = dbResult.Data.ToList();

                    var cacheOptions = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(3)
                    };
                    _cache.Set(AllSettingsKey, parameters, cacheOptions);
                }

                // 2) Asignar a propiedades estáticas
                foreach (var cfg in parameters)
                {
                    var typeName = $"Shared.Configuration.{cfg.Category.Trim()}Settings, Shared";
                    var groupType = Type.GetType(typeName);
                    if (groupType == null)
                    {
                        await _log.LogErrorAsync($"Categoria no encontrada: {cfg.Category}");
                        continue;
                    }

                    var prop = groupType.GetProperty(cfg.Code.Trim());
                    if (prop == null)
                    {
                        await _log.LogErrorAsync($"Propiedad no encontrada: {cfg.Code} en {groupType.Name}");
                        continue;
                    }

                    SetPropertyValue(prop, cfg.Value);
                }
            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }
            return sr;
        }

        public async Task<ServiceResponse<ParameterDto>> GetAsync(string code)
        {
            var sr = new ServiceResponse<ParameterDto>();
            try
            {
                var key = $"{ParamKeyPrefix}{code.Trim()}";

                if (!_cache.TryGetValue(key, out ParameterDto dto))
                {
                    var result = await _read.GetAllAsync(
                        predicate: t => t.Code == code.Trim()
                    );

                    if (!result.Status || result.Data == null || !result.Data.Any())
                    {
                        sr.AddError("Error","Configuration not found.");
                        return sr;
                    }


                    dto = new ParameterDto(result.Data.First());

                    var opts = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(6)
                    };
                    _cache.Set(key, dto, opts);
                }

                sr.Data = dto;
            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }
            return sr;
        }

        public async Task<ServiceResponse<ParameterDto>> GetByIdAsync(int id)
        {
            var sr = new ServiceResponse<ParameterDto>();
            try
            {
                var item = await _read.GetByIdAsync(id);
                if (!item.Status)
                {
                    sr.AddErrors(item.Errors);
                    return sr;
                }
                    

                sr.Data = new ParameterDto(item.Data);
            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }
            return sr;
        }

        // --- Helpers ---

        private ConfigurationDto ConvertToConfiguration(List<Setting> settings)
        {
            var categories = settings
                .GroupBy(s => s.Category)
                .Select(g => new Category
                {
                    Name = g.Key,
                    Parameters = g.Select(cp => new ParameterDto
                    {
                        Id = cp.Id,
                        Code = cp.Code,
                        ControlTitle = cp.ControlTitle,
                        Description = cp.Description,
                        Value = cp.Value,
                        ControlType = cp.ControlType,
                        InputType = cp.InputType,
                        Options = cp.Options != null
                            ? JsonConvert.DeserializeObject<List<ParameterOption>>(cp.Options)
                            : null
                    }).ToList()
                })
                .ToList();

            return new ConfigurationDto { Categories = categories };
        }

        private static void SetPropertyValue(PropertyInfo property, string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return;
            object val = property.PropertyType.Name.ToLower() switch
            {
                "int32" => value.ToInt(),
                "int64" => value.ToInt(),
                "boolean" => value.ToBool(),
                "datetime" => DateTime.Parse(value),
                _ => value.Trim()
            };
            property.SetValue(null, val);
        }
    }
}
