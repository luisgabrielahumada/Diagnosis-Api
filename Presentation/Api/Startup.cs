using Application;
using AspNetCoreRateLimit;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Serilog;
using Serilog.Events;
using Shared;
using System.Text.Json;
namespace Web.Api
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        private readonly IHostEnvironment _hostEnvironment;
        public Startup(IConfiguration configuration, IHostEnvironment hostEnvironment)
        {
            Configuration = configuration;
            _hostEnvironment = hostEnvironment;
            var builder = new ConfigurationBuilder()
                   .SetBasePath(AppContext.BaseDirectory)
                   .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                   .AddJsonFile($"appsettings.{hostEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                   .AddEnvironmentVariables();

            Configuration = builder.Build();
            _hostEnvironment = hostEnvironment;
        }


        public void ConfigureServices(IServiceCollection services)
        {
            DbConfiguration.ConnectionString = Configuration.GetConnectionString(Constants.KeySettings.DefaultConnection);
            services.Configure<IpRateLimitOptions>(Configuration.GetSection("IpRateLimiting"));
            services.AddDirectoryBrowser();
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllCors",
                builder =>
                {
                    builder.WithOrigins("AllowAllCors")
                     .AllowAnyMethod()
                     .AllowAnyHeader()
                     .AllowCredentials();
                });
            });
            services.AddHttpContextAccessor();
            services.AddMemoryCache();
            services.AddEndpointsApiExplorer();
            services.AddMvc();
            services.AddControllers()
                      .AddJsonOptions(options =>
                      {
                          options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                          options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
                          options.JsonSerializerOptions.DefaultIgnoreCondition =
                              System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
                      });
            services.AddAuthorization();
            services.AddRazorPages();
            services.AddHttpClient();
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
            services.AddSwaggerGen();
            services.AddHttpClient();
            services.AddAuthorization();
            services.AddInfrastructureServices();
            services.AddApplicationServices();
            Log.Logger = new LoggerConfiguration()
                            .MinimumLevel.Information()
                            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                            .Enrich.WithProperty("Application", "Detección de Infección")
                            .Enrich.WithProperty("LogType", "logs")
                            .WriteTo.Console()
                            .CreateLogger();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

            }
            app.UseDefaultFiles();
            app.UseStaticFiles();
            var contentPath = Path.Combine(env.ContentRootPath, "Content");
            if (Directory.Exists(contentPath))
            {
                var provider = new PhysicalFileProvider(contentPath);
            }
            //app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors("AllowAllCors");
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Detección de Infección");
                c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
                c.DefaultModelsExpandDepth(-1);
                c.DisplayRequestDuration();
                c.EnableFilter();
                c.ShowExtensions();
                c.EnableDeepLinking();
            });
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
            });
        }
    }
}