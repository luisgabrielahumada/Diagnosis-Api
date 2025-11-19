namespace Shared.Model
{
    public class ResponseLog
    {
        public int StatusCode { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public string Body { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
