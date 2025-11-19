namespace Infrastructure.Dto
{
    public class UserLoggindDto 
    {
        public Guid? Id { get; set; }
        public string Login { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        //public string Campus { get; set; }
        public string Role { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public bool IsWelcome { get; set; } = false;

    }
}
