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

        public static class DiagnosisType
        {
            public static readonly string Zombie = "zombie";
            public static readonly string Covid = "covid";
            public static readonly string Influenza = "influenza";
        }

        public static class StatusFilter
        {
            public static readonly string Active = "active";
            public static readonly string InActive = "inactive";
            public static readonly string All = "all";
        }
    }
}