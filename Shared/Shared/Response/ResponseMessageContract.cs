namespace Shared.Response
{
    public class ResponseMessageContract<T>
    {
        public bool Success { get; set; }
        public T Data { get; set; }
        public List<string> Errors { get; set; } = new();
        public int TotalMessage => Errors.Count;
        public int Version { get; set; } = 1;
        public Guid TransactionId { get; set; } = Guid.NewGuid();
    }

    public class ResponseMessageContract : ResponseMessageContract<object> { }

}
