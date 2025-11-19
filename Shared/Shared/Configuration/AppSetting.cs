namespace Shared
{

    public class SwaggerSettings
    {
        public string RoutePrefix { get; set; }
    }

    public class JwtSettings
    {
        public string Key { get; set; }
        public string Issuer { get; set; }
        public string Scheme { get; set; }
        public string Description { get; set; }
    }

    public class Settings
    {
        public string AllowAllCors { get; set; }
        public string Version { get; set; }
        public string Proxy { get; set; }
        public string IpRateLimiting { get; set; }
        public List<string> ExcludePath { get; set; }
        public string[] WithOrigins { get; set; }
        public JwtSettings JwtSettings { get; set; }
        public SwaggerSettings SwaggerSettings { get; set; }
    }
}