using Shared.MapperModel;
using Domain.Entities;

namespace Infrastructure.Dto
{
    public class LoginResponseDto  
    {
        public string Token { get; set; }
        public UserLoggindDto User { get; set; }
 
    }
}
