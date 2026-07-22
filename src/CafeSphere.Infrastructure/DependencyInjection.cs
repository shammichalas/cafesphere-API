using System.Text;
using CafeSphere.Application.Interfaces;
using CafeSphere.Infrastructure.Authentication;
using CafeSphere.Infrastructure.Caching;
using CafeSphere.Infrastructure.Email;
using CafeSphere.Infrastructure.Identity;
using CafeSphere.Infrastructure.Security;
using CafeSphere.Infrastructure.Storage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace CafeSphere.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddSingleton<IJwtService, JwtService>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddTransient<IEmailService, EmailService>();
        services.AddTransient<IFileStorageService, FileStorageService>();

        // Caching
        var redisConn = configuration["RedisSettings:ConnectionString"] 
            ?? configuration.GetConnectionString("Redis") 
            ?? Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING")
            ?? "localhost:6379";

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConn;
            options.InstanceName = "CafeSphere_";
        });
        services.AddSingleton<ICacheService, RedisCacheService>();

        // JWT Authentication Configuration
        var jwtSettings = new JwtSettings();
        configuration.GetSection(JwtSettings.SectionName).Bind(jwtSettings);
        var key = Encoding.ASCII.GetBytes(jwtSettings.Secret);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddAuthorization();

        return services;
    }
}
