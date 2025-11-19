using Application.Dto.Common;
using Application.Factory.Interface;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Infrastructure.Dto;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Shared;
using Shared.Crypto;
using Shared.Extensions;
using Shared.Response;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace Infrastructure.Services
{
    public interface IAuthService
    {
        Task<ServiceResponse<LoginResponseDto>> GetUserReNewalTokenAsync(string? userId);
        //Task<ServiceResponse<bool>> RegisterUserAsync(UsersDto userDto);
        Task<ServiceResponse<LoginResponseDto>> LoginUserAsync(LoginDto loginDto);
        Task<ServiceResponse<LoginResponseDto>> LoginMicrosoftAsync(MicrosoftLoginDto loginDto);
        Task<ServiceResponse> ReNewToken(SessionDto token);
    }

    public class AuthService : IAuthService
    {
        private readonly IReadRepository<User> _read;
        private readonly IReadRepository<Role> _readRole;
        private readonly IWriteRepository<User> _write;
        private readonly IMemoryCache _cache;
        private readonly ILoggingService _log;
        private readonly IJobScheduler _scheduler;

        public AuthService(IReadRepository<User> read, IWriteRepository<User> write,
                           IMemoryCache cache, ILoggingService log, IReadRepository<Role> readRole,
                           IJobScheduler scheduler)
        {
            _read = read;
            _cache = cache;
            _log = log;
            _write = write;
            _readRole = readRole;
            _scheduler = scheduler;
        }

        //public async Task<ServiceResponse<bool>> RegisterUserAsync(UsersDto userDto)
        //{
        //    var response = new ServiceResponse<bool>();
        //    try
        //    {
        //        var user = userDto.ToEntity();
        //        user.PasswordHash = userDto.Password.AesEncrypt(Constants.AesKey);
        //        user.IsActive = true;
        //        await _write.AddAsync("0", user);
        //        response.Data = true;
        //    }
        //    catch (Exception ex)
        //    {
        //        response.AddError(ex);
        //    }
        //    return response;
        //}

        public async Task<ServiceResponse<LoginResponseDto>> LoginUserAsync(LoginDto loginDto)
        {
            var response = new ServiceResponse<LoginResponseDto>();

            try
            {
                var includes = new[] {
                    nameof(Role)
                };
                var passwordHash = loginDto.Password.AesEncrypt(Constants.AesKey);

                var usersResult = await _read.GetAllAsync(includes,
                    predicate: t => t.Login == loginDto.Login && t.IsActive == true
                );

                if (!usersResult.Status)
                {
                    response.AddErrors(usersResult.Errors);
                    return response;
                }

                var userDB = usersResult.Data.FirstOrDefault();
                if (userDB is null)
                {
                    response.AddError(Errors.LOGIN_INVALID_CREDENTIALS, Errors.LOGIN_INVALID_CREDENTIALS_MESSAGE);
                    return response;
                }

                if (!VerifyPassword(userDB.PasswordHash, passwordHash))
                {
                    response.AddError(Errors.LOGIN_INVALID_CREDENTIALS, Errors.LOGIN_INVALID_CREDENTIALS_MESSAGE);
                    return response;
                }

                var user = new UserDto(userDB);

                var token = GenerateJwtToken(user);

                response.Data = new LoginResponseDto
                {
                    Token = token,
                    User = new UserLoggindDto
                    {
                        //Campus = user?.Campus.Name,
                        LastLoginAt = user.CreatedAt,
                        Email = user.Email,
                        Id = user.Id,
                        Login = user.Login,
                        Name = user.Name,
                        Phone = user.Phone,
                        Role = user.Role.Code.ToString()
                    }
                };
            }
            catch (Exception ex)
            {
                response.AddError(ex);
            }

            return response;
        }

        public async Task<ServiceResponse<LoginResponseDto>> GetUserReNewalTokenAsync(string userId)
        {
            var response = new ServiceResponse<LoginResponseDto>();
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    response.AddError(Errors.AUTHORIZATION_INVALID_TOKEN, Errors.AUTHORIZATION_INVALID_TOKEN_MESSAGE);
                    return response;
                }
                var includes = new[]
                {
                    nameof(Role)
                };
                var userDB = await _read
                                    .GetByIdAsync(Guid.Parse(userId), includes);
                if (!userDB.Status)
                {
                    response.AddErrors(userDB.Errors);
                    return response;
                }

                if (userDB.Data is not User)
                {
                    response.AddError(Errors.AUTHORIZATION_INVALID_TOKEN, Errors.AUTHORIZATION_INVALID_TOKEN_MESSAGE);
                    return response;
                }
                var user = new UserDto(userDB.Data);
                // 2️⃣ ✅ Generate JWT Token
                var token = GenerateJwtToken(user);

                response.Data = new LoginResponseDto
                {
                    Token = token,
                    User = new UserLoggindDto
                    {
                        //Campus = user?.Campus.Name,
                        LastLoginAt = user.CreatedAt,
                        Email = user.Email,
                        Id = user.Id,
                        Login = user.Login,
                        Name = user.Name,
                        Phone = user.Phone,
                        Role = user.Role.Code.ToString()
                    }
                };
            }
            catch (Exception ex)
            {
                response.AddError(ex);
            }
            return response;
        }

        // ----- En tu clase de generación de token -----
        private string GenerateJwtToken(UserDto user)
        {
            var handler = new JwtSecurityTokenHandler();
            var keyBytes = Convert.FromBase64String(JwtConfiguration.Key);
            var signingKey = new SymmetricSecurityKey(keyBytes);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Name,  user.Name),
                new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString())
            };

            var now = DateTime.UtcNow;
            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                NotBefore = now,
                Expires = now.AddMinutes(30),
                SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256),
                Issuer = JwtConfiguration.Issuer
                // Audience = JwtConfiguration.Audience  // si lo necesitas
            };

            var token = handler.CreateToken(descriptor);
            return handler.WriteToken(token);
        }

        private bool VerifyPassword(string enteredPassword, string storedHash)
        {
            return enteredPassword.SequenceEqual(storedHash);
        }

        public async Task<ServiceResponse> ReNewToken(SessionDto token)
        {
            var response = new ServiceResponse();
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(JwtConfiguration.Key);

                // Validate the old token
                var principal = tokenHandler.ValidateToken(token.OldToken, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = false, // ✅ Allow expired token validation
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;


                // Extract claims from the old token
                var userId = jwtToken.Claims.First(x => x.Type == JwtRegisteredClaimNames.NameId).Value;
                var userEmail = jwtToken.Claims.First(x => x.Type == JwtRegisteredClaimNames.Email).Value;
                var role = jwtToken.Claims.First(x => x.Type == JwtRegisteredClaimNames.Sid).Value;
                var userFirstName = jwtToken.Claims.First(x => x.Type == JwtRegisteredClaimNames.Name).Value;
                var userLastName = jwtToken.Claims.First(x => x.Type == JwtRegisteredClaimNames.FamilyName).Value;

                var roleDB = await _readRole.GetByIdAsync(Guid.Parse(role));

                response.Data = GenerateJwtToken(new UserDto
                {
                    Id = Guid.Parse(userId),
                    Email = userEmail,
                    //Role = new Role { Id = int.Parse(role) },
                    RoleId = Guid.Parse(role),
                    Role = new RoleDto(roleDB.Data),
                    Name = userFirstName,
                });
            }
            catch (Exception ex)
            {
                response.AddError(ex);
            }
            return response;
        }

        public async Task<ServiceResponse<LoginResponseDto>> LoginMicrosoftAsync(MicrosoftLoginDto request)
        {
            var response = new ServiceResponse<LoginResponseDto>();

            try
            {
                // 1. Validar token y extraer email
                //var principal = await ValidateMicrosoftTokenAsync(request.Password);
                //var email = principal
                //    .FindFirst("preferred_username")?.Value?
                //    .Trim()
                //    .ToLowerInvariant()
                //    ?? throw new SecurityException("Token inválido");
                var email = request.Login.Trim().ToLowerInvariant();

                // 2. Buscar usuario (trae solo uno y rol incluido)
                var usersResult = await _read.GetAllAsync(
                    includes: new[] { nameof(Role) },
                    predicate: x =>
                        x.Login.ToLower().Trim() == email
                );
                var isWelcome = false;
                var userEntity = usersResult.Data?.FirstOrDefault();
                UserDto userDto;
                var isNewUser = userEntity == null;

                if (isNewUser)
                {
                    // 3a. Crear usuario nuevo
                    var pwd = StringExtensions.GenerateSecurePassword();
                    userDto = new UserDto
                    {
                        Email = email,
                        Login = email,
                        Name = email,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        PasswordHash = pwd.AesEncrypt(Constants.AesKey),
                        RoleId = (await _readRole.GetAllAsync(
                                          null,
                                          t => t.Code == UserRole.Pending.ToString()))
                                       .Data
                                       .First().Id

                    };

                    userEntity = userDto.ToEntity();
                    await _write.AddAsync(request.Login, userEntity);


                    //_backgroundJobClient.Enqueue<INotificationService>(job =>
                    //                job.(emailTemplateId, destinatarios, datosPlantilla));
                    //await _notificationService.SendUserCreatedEmailAsync(
                    //    to: userDto.Email,
                    //    subject: "¡Bienvenido!",
                    //    body: $"Hola {userDto.Name}, tu usuario ha sido creado. Por favor, completa tu perfil."
                    //);
                }
                else
                {

                    if (userEntity.IsActive != true || userEntity.IsDeleted == true)
                    {
                        response.AddError(Errors.LOGIN_USER_INACTIVE, "El usuario no está activo o ha sido eliminado.");
                        return response;
                    }

                    // 3b. Mapear DTO desde la entidad existente
                    userDto = new UserDto
                    {
                        Id = userEntity.Id,
                        Email = userEntity.Email,
                        Login = userEntity.Login,
                        Name = userEntity.Name,
                        Phone = userEntity.Phone,
                        CreatedAt = userEntity.CreatedAt,
                        RoleId = userEntity.RoleId,
                        IsActive = userEntity.IsActive,
                        IsDeleted = userEntity.IsDeleted,
                        RoleCode = userEntity.Role.Code.ToLower()
                    };

                    if (userEntity.Role.Code.Equals(UserRole.Admin.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        isWelcome = false;
                        userEntity.UpdatedAt = DateTime.UtcNow;
                        await _write.UpdateAsync(request.Login, userEntity);
                    }
                    if (!userEntity.UpdatedAt.HasValue)
                    {
                        isWelcome = true;
                        var message = new NotificationMessage
                        {
                            To = userDto.Email,
                            Subject = "Mensaje de Bienvenida:",
                            Body = $"¡Bienvenido(a) al módulo de planeación de clase!",
                            TemplateId = Constants.NotificationTemplate.Welcome,
                            Keys = new Dictionary<string, string>()
                            {
                                {"name", userDto.Name },
                                {"login", userDto.Login },
                                {"email", userDto.Email },
                                {"phone", userDto.Phone ?? string.Empty }
                            }
                        };
                        _scheduler.Enqueue<INotificationJob>(job =>
                               job.SendNotificationAsync(Constants.NotificationType.Email, message)
                        );
                    }
                }

                // 4. Generar JWT e actualizar fecha de login
                var token = GenerateJwtToken(userDto);


                // 5. Respuesta
                response.Data = new LoginResponseDto
                {
                    Token = token,
                    User = new UserLoggindDto
                    {
                        Id = userDto.Id,
                        Email = userDto.Email,
                        Login = userDto.Login,
                        Name = userDto.Name,
                        Phone = userDto.Phone,
                        Role = userDto.RoleCode,
                        LastLoginAt = userEntity.UpdatedAt,
                        IsWelcome = isWelcome
                    }
                };
            }
            catch (Exception ex)
            {
                response.AddError(ex);
            }

            return response;
        }
        public async Task<ClaimsPrincipal> ValidateMicrosoftTokenAsync(string token)
        {
            IdentityModelEventSource.ShowPII = true;

            var authority = $"https://login.microsoftonline.com/{AppMicrosoftProvider.TenantID}/v2.0";
            var validAudience = AppMicrosoftProvider.ClientID;

            var httpClient = new HttpClient(); // 👈 NECESARIO
            var retriever = new HttpDocumentRetriever(httpClient)  // 👈 aquí el truco
            {
                RequireHttps = true
            };

            var configManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                $"{authority}/.well-known/openid-configuration",
                new OpenIdConnectConfigurationRetriever(),
                retriever
            );

            var openIdConfig = await configManager.GetConfigurationAsync(); // 👈 esto debe darte signing keys

            if (openIdConfig.SigningKeys == null || openIdConfig.SigningKeys.Count == 0)
                throw new Exception("❌ No se pudieron obtener claves públicas desde Azure AD.");

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = openIdConfig.SigningKeys,

                ValidateIssuer = true,
                ValidIssuer = authority,

                ValidateAudience = true,
                ValidAudience = validAudience,

                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(5)
            };

            var handler = new JwtSecurityTokenHandler();
            return handler.ValidateToken(token, validationParameters, out SecurityToken security);
        }

    }
}