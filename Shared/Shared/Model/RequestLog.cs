namespace Shared.Model
{
    public class RequestLog
    {
        public string Method { get; set; }
        public string Path { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public string Body { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
