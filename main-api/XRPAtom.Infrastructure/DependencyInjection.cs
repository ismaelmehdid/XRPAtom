using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
            services.AddScoped<ICurtailmentEventRepository, CurtailmentEventRepository>();
            //services.AddScoped<IEventParticipationRepository, EventParticipationRepository>();
            services.AddScoped<ITransactionRepository, TransactionRepository>();
            //services.AddScoped<IMarketplaceListingRepository, MarketplaceListingRepository>();
            //services.AddScoped<IMarketplaceTransactionRepository, MarketplaceTransactionRepository>();

            // Register services
            // services.AddScoped<IUserService, UserService>();
            services.AddScoped<IDeviceService, DeviceService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IPasswordService, PasswordService>();
            services.AddScoped<ICurtailmentEventService, CurtailmentEventService>();
            // services.AddScoped<IMarketplaceService, MarketplaceService>();


            // Register background services
            // services.AddHostedService<CurtailmentSchedulerService>();

            return services;
        }
    }
}