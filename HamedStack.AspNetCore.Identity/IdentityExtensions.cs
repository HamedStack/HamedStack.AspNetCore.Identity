using Microsoft.AspNetCore.Http;

namespace HamedStack.AspNetCore.Identity
{
    public static class IdentityExtensions
    {
        public static void SetRefreshTokenInCookie(this HttpResponse response, string refreshToken, CookieOptions? cookieOptions = default)
        {
            cookieOptions ??= new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(10),
            };
            response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }
    }
}