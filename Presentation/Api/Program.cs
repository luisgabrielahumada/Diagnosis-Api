using Shared;

namespace Web.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
             Host.CreateDefaultBuilder(args)
                 .ConfigureWebHostDefaults(webBuilder =>
                 {
                     webBuilder.UseStartup<Startup>();
                     var environment = Environment.GetEnvironmentVariable(Constants.AspNetCodreEnv);
                     if (!string.Equals(environment, Constants.Environment.Development, StringComparison.OrdinalIgnoreCase))
                     {
                         webBuilder.UseUrls("https://0.0.0.0:80");
                     }
                 });
    }
}