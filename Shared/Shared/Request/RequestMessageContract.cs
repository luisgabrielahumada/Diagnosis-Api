namespace Shared.Request
{
    public class RequestMessageContract
    {
        public Guid RequestId { get; set; } = Guid.NewGuid();
        public string UserId { get; set; } = string.Empty;
        public DateTime Date { get; set; } = DateTime.UtcNow;
    }
}
