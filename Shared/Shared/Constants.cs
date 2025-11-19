namespace Shared
{
    public static class Constants
    {
        public static class KeySettings
        {
            public static readonly string DefaultConnection = "DefaultConnection";
        }
        public static class Environment
        {
            public static readonly string Development = "Development";
            public static readonly string Staging = "Staging";
            public static readonly string Production = "Production";
        }
        public static string AspNetCodreEnv = "ASPNETCORE_ENVIRONMENT";
    }
}