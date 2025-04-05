using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;
using XRMAtom.Infrastructure.Services;
using XRPAtom.Core.Interfaces;
using XRPAtom.Core.Repositories;
using XRPAtom.Infrastructure.Data.Repositories;
using XRPAtom.Infrastructure.Services;

namespace XRPAtom.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IDeviceRepository, DeviceRepository>();
            //services.AddScoped<ICurtailmentEventRepository, CurtailmentEventRepository>();
            //services.AddScoped<IEventParticipationRepository, EventParticipationRepository>();
            services.AddScoped<ITransactionRepository, TransactionRepository>();
            //services.AddScoped<IMarketplaceListingRepository, MarketplaceListingRepository>();
            //services.AddScoped<IMarketplaceTransactionRepository, MarketplaceTransactionRepository>();

            // Register services
            // services.AddScoped<IUserService, UserService>();
            // services.AddScoped<IDeviceService, DeviceService>();
            // services.AddScoped<ICurtailmentEventService, CurtailmentEventService>();
            // services.AddScoped<IMarketplaceService, MarketplaceService>();

            // JWT Authentication
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["Jwt:Key"]))
                };
            });

            // Register background services
            // services.AddHostedService<CurtailmentSchedulerService>();

            return services;
        }
    }
}