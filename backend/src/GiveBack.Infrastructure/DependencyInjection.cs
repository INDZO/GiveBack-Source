using GiveBack.Application.Common.Interfaces;
using GiveBack.Infrastructure.Authentication;
using GiveBack.Infrastructure.BackgroundServices;
using GiveBack.Infrastructure.Persistence;
using GiveBack.Infrastructure.Repositories;
using GiveBack.Infrastructure.Services;
using GiveBack.Infrastructure.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GiveBack.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<GiveBackDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IOpportunityRepository, OpportunityRepository>();
        services.AddScoped<IApplicationRepository, ApplicationRepository>();
        services.AddScoped<IVolunteerRepository, VolunteerRepository>();
        services.AddScoped<IOrganizationRepository, OrganizationRepository>();
        services.AddScoped<IHourLogRepository, HourLogRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();

        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<ICertificatePdfService, CertificatePdfService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IBadgeService, BadgeService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddHostedService<ReminderNotificationBackgroundService>();
        return services;
    }
}
