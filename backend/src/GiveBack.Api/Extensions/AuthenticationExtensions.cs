using System.Text;
using GiveBack.Api.Options;
using GiveBack.Domain.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace GiveBack.Api.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddGiveBackAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        var jwt = configuration.GetSection("Jwt").Get<JwtOptions>()
            ?? throw new InvalidOperationException("Jwt configuration section is missing.");
        if (string.IsNullOrWhiteSpace(jwt.Key) || Encoding.UTF8.GetByteCount(jwt.Key) < 32)
        {
            throw new InvalidOperationException("Set Jwt__Key to a locally generated signing key of at least 32 bytes.");
        }

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(nameof(UserRole.Volunteer), policy => policy.RequireRole(nameof(UserRole.Volunteer)));
            options.AddPolicy(nameof(UserRole.Organization), policy => policy.RequireRole(nameof(UserRole.Organization)));
            options.AddPolicy(nameof(UserRole.Admin), policy => policy.RequireRole(nameof(UserRole.Admin)));
        });
        return services;
    }
}
