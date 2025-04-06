using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using XRPAtom.Blockchain.Interfaces;
using XRPAtom.Blockchain.Services;
using XRPAtom.Core.Interfaces;
using XRPAtom.Infrastructure.BackgroundServices;

namespace XRPAtom.Blockchain
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddBlockchainServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register HttpClients for blockchain services
            // XRP Ledger service
            services.AddHttpClient<IXRPLedgerService, XRPLedgerService>(client =>
            {
                client.BaseAddress = new Uri(configuration["XRPLedger:ServerUrl"] ?? "https://s.altnet.rippletest.net:51234/");
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            // Xaman service
            services.AddHttpClient<IXamanService, XamanService>(client =>
            {
                client.BaseAddress = new Uri("https://xumm.app/api/v1/");
                client.Timeout = TimeSpan.FromSeconds(30);
                
                // Add Xaman API credentials if available
                if (!string.IsNullOrEmpty(configuration["Xaman:ApiKey"]))
                {
                    client.DefaultRequestHeaders.Add("X-API-Key", configuration["Xaman:ApiKey"]);
                }
                
                if (!string.IsNullOrEmpty(configuration["Xaman:ApiSecret"]))
                {
                    client.DefaultRequestHeaders.Add("X-API-Secret", configuration["Xaman:ApiSecret"]);
                }
            });

            // Register blockchain services
            services.AddScoped<IXRPLTransactionService, XRPLTransactionService>();
            services.AddScoped<IXRPLedgerService, XRPLedgerService>();
            services.AddScoped<IXamanService, XamanService>();
            services.AddScoped<IBlockchainVerificationService, BlockchainVerificationService>();
            services.AddScoped<IBlockchainEventListener, BlockchainEventListener>();
            
            // Add the UserWalletService
            services.AddScoped<IUserWalletService, UserWalletService>();
            services.AddScoped<IEscrowService, EscrowService>();
            
            // Register background services
            // Register other background services as needed
            // services.AddHostedService<XRPLedgerEventMonitor>();

            return services;
        }
    }
}