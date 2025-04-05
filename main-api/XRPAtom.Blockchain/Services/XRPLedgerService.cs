using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using XRPAtom.Blockchain.Interfaces;
using XRPAtom.Core.Domain;

namespace XRPAtom.Blockchain.Services;

public class XRPLedgerService : IXRPLedgerService
{
    public Task<string> GetAccountInfo(string address)
    {
        throw new NotImplementedException();
    }

    public Task<string> GenerateWallet()
    {
        throw new NotImplementedException();
    }

    public Task<string> SubmitTransaction(string transaction, string signature)
    {
        throw new NotImplementedException();
    }

    public Task<string> CreatePayment(string sourceAddress, string destinationAddress, decimal amount, string sourceSecret)
    {
        throw new NotImplementedException();
    }

    public Task<bool> VerifyCurtailmentEvent(string eventId, string proof)
    {
        throw new NotImplementedException();
    }

    public Task<string> IssueReward(string destinationAddress, decimal amount, string eventId)
    {
        throw new NotImplementedException();
    }

    public Task<string> CreateMarketplaceOffer(string address, string offerType, decimal amount, decimal price, string secret)
    {
        throw new NotImplementedException();
    }

    public Task<List<object>> GetMarketplaceOffers(string type)
    {
        throw new NotImplementedException();
    }

    public Task<string> AcceptOffer(string offerId, string buyerAddress, string secret)
    {
        throw new NotImplementedException();
    }
}
