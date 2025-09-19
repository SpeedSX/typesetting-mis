using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TypesettingMIS.Application.Common.Interfaces;
using TypesettingMIS.Core.Entities;
using TypesettingMIS.Core.Services;
using TypesettingMIS.Infrastructure.Data;
using TypesettingMIS.Infrastructure.Services;

namespace TypesettingMIS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContextPool<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
            {
                // Retry on transient network errors/timeouts
                npgsql.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorCodesToAdd: null);
            }));

        // Register the IApplicationDbContext interface
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        // Add Identity services
        services.AddIdentityCore<User>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;
            options.User.RequireUniqueEmail = true;
        })
        .AddRoles<Role>()
        .AddEntityFrameworkStores<ApplicationDbContext>();

        // Add custom services
        services.AddScoped<IJwtConfigurationService, JwtConfigurationService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IInvitationService, InvitationService>();
        services.AddScoped<ITenantContext, TenantContext>();
        services.AddScoped<ITenantAwareService, TenantAwareService>();

        return services;
    }
}
