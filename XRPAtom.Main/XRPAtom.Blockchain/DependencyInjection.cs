using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using XRMAtom.Infrastructure.BackgroundServices;
using XRPAtom.Blockchain.Interfaces;
using XRPAtom.Blockchain.Services;

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
                
                // Add Xaman API credentials
                client.DefaultRequestHeaders.Add("X-API-Key", configuration["Xaman:ApiKey"]);
                client.DefaultRequestHeaders.Add("X-API-Secret", configuration["Xaman:ApiSecret"]);
            });

            // Register blockchain services
            services.AddScoped<IXRPLTransactionService, XRPLTransactionService>();
            services.AddScoped<IXRPLedgerService, XRPLedgerService>();
            services.AddScoped<IXamanService, XamanService>();
            services.AddScoped<IBlockchainVerificationService, BlockchainVerificationService>();

            // Register background service for blockchain event monitoring
            //services.AddHostedService<XRPLedgerEventMonitor>();

            return services;
        }
    }
}