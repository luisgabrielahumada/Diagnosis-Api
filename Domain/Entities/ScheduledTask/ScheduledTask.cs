namespace Domain.Entities
{
    public class ScheduledTask
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string CronExpression { get; set; } = default!;
        public bool IsActive { get; set; }
        public string MethodName { get; set; } = default!;
        public string Module { get; set; } = default!;
        public string QueueName { get; set; } = "slow";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
