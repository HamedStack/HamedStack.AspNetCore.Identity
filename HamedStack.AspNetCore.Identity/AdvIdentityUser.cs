using Microsoft.AspNetCore.Identity;

namespace HamedStack.AspNetCore.Identity
{
    public class AdvIdentityUser : IdentityUser
    {
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }
    }
}
