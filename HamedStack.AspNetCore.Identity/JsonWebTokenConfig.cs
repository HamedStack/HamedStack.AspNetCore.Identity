namespace HamedStack.AspNetCore.Identity
{
    public class JsonWebTokenConfig
    {
        public int RefreshTokenValidityInDays { get; set; }
        public string Secret { get; set; } = null!;
        public int TokenValidityInMinutes { get; set; }
        public string ValidAudience { get; set; } = null!;
        public string ValidIssuer { get; set; } = null!;
    }
}
