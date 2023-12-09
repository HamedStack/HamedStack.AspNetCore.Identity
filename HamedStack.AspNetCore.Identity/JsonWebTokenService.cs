using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace HamedStack.AspNetCore.Identity
{
    public class JsonWebTokenService
    {
        private readonly IOptions<JsonWebTokenConfig> _options;
        private readonly UserManager<AdvIdentityUser> _userManager;
        public JsonWebTokenService(UserManager<AdvIdentityUser> userManager, IOptions<JsonWebTokenConfig> options)
        {
            _userManager = userManager;
            _options = options;
        }

        public async Task<TokenModel?> Login(LoginModel model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var userRoles = await _userManager.GetRolesAsync(user);

                var authClaims = new List<Claim>
                {
                    new(ClaimTypes.Name, user.UserName!),
                    new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                var token = CreateToken(authClaims);
                var refreshToken = GenerateRefreshToken();

                _ = int.TryParse(_options.Value.RefreshTokenValidityInDays.ToString(), out int refreshTokenValidityInDays);

                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = DateTime.Now.AddDays(refreshTokenValidityInDays);

                await _userManager.UpdateAsync(user);

                return new TokenModel
                {
                    AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                    RefreshToken = refreshToken
                };
            }
            return null;
        }
        public async Task<TokenModel?> RefreshToken(TokenModel tokenModel)
        {
            if (tokenModel is null)
            {
                return null;
            }

            string? accessToken = tokenModel.AccessToken;
            string? refreshToken = tokenModel.RefreshToken;

            var principal = GetPrincipalFromExpiredToken(accessToken);

            if (principal == null)
            {
                return null;
            }
            if (principal.Identity == null)
            {
                return null;
            }
            if (principal.Identity.Name == null)
            {
                return null;
            }

            string username = principal.Identity.Name;

            var user = await _userManager.FindByNameAsync(username);

            if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
            {
                return null;
            }

            var newAccessToken = CreateToken(principal.Claims.ToList());
            var newRefreshToken = GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            await _userManager.UpdateAsync(user);

            return new TokenModel
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(newAccessToken),
                RefreshToken = newRefreshToken
            };
        }

        public async Task<IdentityResult> Register(RegisterModel model)
        {
            ArgumentNullException.ThrowIfNull(model);

            if (model.UserName is null)
            {
                throw new ArgumentNullException(nameof(model.UserName));
            }
            if (model.Password is null)
            {
                throw new ArgumentNullException(nameof(model.Password));
            }


            var userExists = await _userManager.FindByNameAsync(model.UserName);
            if (userExists != null)
            {
                return new AdvIdentityResult()
                {
                    Succeeded = false,
                    Errors = new List<IdentityError> { new() { Description = "User already exists!" } }
                };
            }

            AdvIdentityUser user = new()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.UserName
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return result;

            return new AdvIdentityResult()
            {
                Succeeded = true,
            };
        }
        public async Task<bool> Revoke(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null) return false;

            user.RefreshToken = null;
            await _userManager.UpdateAsync(user);

            return true;
        }

        public async Task RevokeAll()
        {
            var tasks = new List<Task>();

            foreach (var user in _userManager.Users.ToList())
            {
                user.RefreshToken = null;
                tasks.Add(_userManager.UpdateAsync(user));
            }

            await Task.WhenAll(tasks);
        }

        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private JwtSecurityToken CreateToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Value.Secret));
            _ = int.TryParse(_options.Value.TokenValidityInMinutes.ToString(), out int tokenValidityInMinutes);

            return new JwtSecurityToken(
                issuer: _options.Value.ValidIssuer,
                audience: _options.Value.ValidAudience,
                claims: authClaims,
                expires: DateTime.Now.AddMinutes(tokenValidityInMinutes),
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );
        }
        private ClaimsPrincipal? GetPrincipalFromExpiredToken(string? token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Value.Secret)),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }
    }
}
