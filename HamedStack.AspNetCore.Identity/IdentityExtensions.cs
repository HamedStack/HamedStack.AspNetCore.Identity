﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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

        public static void AddAdvIdentity(this IServiceCollection service, ConfigurationManager configurationManager, string jwtConfig, string connStrConfig, bool requireHttpsMetadata = false)
        {
            service.Configure<JsonWebTokenConfig>(configurationManager.GetSection(jwtConfig));
            service.AddDbContext<AdvIdentityDbContext>(options => options.UseSqlServer(configurationManager.GetConnectionString(connStrConfig)));

            service.AddIdentity<AdvIdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<AdvIdentityDbContext>()
                .AddDefaultTokenProviders();

            service.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })

            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = requireHttpsMetadata;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.Zero,

                    ValidAudience = configurationManager[$"{jwtConfig}:ValidAudience"],
                    ValidIssuer = configurationManager[$"{jwtConfig}:ValidIssuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configurationManager[$"{jwtConfig}:Secret"]!))
                };
            });

            service.AddScoped<JsonWebTokenService>();
        }
    }
}