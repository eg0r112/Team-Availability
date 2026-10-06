using System.Text;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using TeamAvailability.Application.Common.Interfaces;
using TeamAvailability.Infrastructure.BackgroundServices;
using TeamAvailability.Infrastructure.Caching;
using TeamAvailability.Infrastructure.Integrations;
using TeamAvailability.Infrastructure.Messaging.Consumers;
using TeamAvailability.Infrastructure.Options;
using TeamAvailability.Infrastructure.Persistence;
using TeamAvailability.Infrastructure.Persistence.Seed;
using TeamAvailability.Infrastructure.Services;

namespace TeamAvailability.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment? environment = null)
    {
        var isTesting = environment?.IsEnvironment("Testing") == true;
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<TelegramOptions>(configuration.GetSection(TelegramOptions.SectionName));
        services.Configure<JiraOptions>(configuration.GetSection(JiraOptions.SectionName));
        services.Configure<LocalDevOptions>(configuration.GetSection(LocalDevOptions.SectionName));

        var localDev = configuration.GetSection(LocalDevOptions.SectionName).Get<LocalDevOptions>() ?? new LocalDevOptions();

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ICorrelationIdProvider, CorrelationIdProvider>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IApiKeyValidator, ApiKeyValidator>();
        services.AddSingleton<IJiraIntegrationSettings, JiraIntegrationSettings>();
        if (localDev.UseMemoryCache)
        {
            services.AddDistributedMemoryCache();
            services.AddScoped<IAvailabilityCache, MemoryAvailabilityCache>();
            services.AddScoped<IIdempotencyStore, MemoryIdempotencyStore>();
        }
        else
        {
            services.AddScoped<IAvailabilityCache, RedisAvailabilityCache>();
            services.AddScoped<IIdempotencyStore, RedisIdempotencyStore>();
            var redisConnection = configuration.GetConnectionString("Redis") ?? "localhost:6379";
            services.AddStackExchangeRedisCache(options => options.Configuration = redisConnection);
        }

        services.AddHttpClient<ITelegramNotificationService, TelegramNotificationService>();
        services.AddHttpClient<IJiraClient, JiraCloudClient>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var jwt = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret))
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy("AdminOnly", policy => policy.RequireRole(nameof(Domain.Enums.SystemRole.Admin)))
            .AddPolicy("TeamLeadOrAdmin", policy => policy.RequireRole(
                nameof(Domain.Enums.SystemRole.Admin),
                nameof(Domain.Enums.SystemRole.TeamLead)));

        services.AddMassTransit(x =>
        {
            x.AddConsumer<AbsenceCreatedConsumer>();

            if (localDev.UseInMemoryBus)
            {
                x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
            }
            else
            {
                var rabbitHost = configuration.GetConnectionString("RabbitMq") ?? "amqp://guest:guest@localhost:5672";
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(new Uri(rabbitHost));
                    cfg.ConfigureEndpoints(context);
                });
            }
        });

        services.AddHostedService<DatabaseSeeder>();

        if (!isTesting)
        {
            services.AddHostedService<OutboxDispatcherService>();
            services.AddHostedService<AvailabilityRecalculationService>();
            services.AddHostedService<OnCallReminderService>();
            services.AddHostedService<TelegramPollingService>();
            services.AddHttpClient(nameof(TelegramPollingService));
        }

        return services;
    }
}
