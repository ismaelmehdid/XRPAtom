namespace XRPAtom.Blockchain.Interfaces
{
    public interface IXRPLedgerService
    {
        /// <summary>
        /// Gets account information from the XRP Ledger
        /// </summary>
        /// <param name="address">The account address</param>
        /// <returns>Account information in JSON format</returns>
        Task<string> GetAccountInfo(string address);
        
        /// <summary>
        /// Generates a new XRPL wallet (address and secret)
        /// </summary>
        /// <returns>Wallet information in JSON format</returns>
        Task<string> GenerateWallet();
        
        /// <summary>
        /// Submits a signed transaction to the XRP Ledger
        /// </summary>
        /// <param name="transaction">The transaction to submit</param>
        /// <param name="signature">The signature for the transaction</param>
        /// <returns>The submission result</returns>
        Task<string> SubmitTransaction(string transaction, string signature);
        
        /// <summary>
        /// Creates a payment transaction
        /// </summary>
        /// <param name="sourceAddress">The source account address</param>
        /// <param name="destinationAddress">The destination account address</param>
        /// <param name="amount">The payment amount</param>
        /// <param name="sourceSecret">The source account secret key</param>
        /// <returns>The transaction result</returns>
        Task<string> CreatePayment(string sourceAddress, string destinationAddress, decimal amount, string sourceSecret);
        
        /// <summary>
        /// Verifies a curtailment event on the blockchain
        /// </summary>
        /// <param name="eventId">The event ID</param>
        /// <param name="proof">The verification proof</param>
        /// <returns>True if verified, false otherwise</returns>
        Task<bool> VerifyCurtailmentEvent(string eventId, string proof);
        
        /// <summary>
        /// Issues a reward to a user address
        /// </summary>
        /// <param name="destinationAddress">The recipient address</param>
        /// <param name="amount">The reward amount</param>
        /// <param name="eventId">The related event ID</param>
        /// <returns>The transaction ID</returns>
        Task<string> IssueReward(string destinationAddress, decimal amount, string eventId);
        
        /// <summary>
        /// Creates a marketplace offer on the XRP Ledger DEX
        /// </summary>
        /// <param name="address">The creator address</param>
        /// <param name="offerType">The offer type (buy/sell)</param>
        /// <param name="amount">The amount offered</param>
        /// <param name="price">The price per unit</param>
        /// <param name="secret">The creator's secret key</param>
        /// <returns>The offer ID</returns>
        Task<string> CreateMarketplaceOffer(string address, string offerType, decimal amount, decimal price, string secret);
        
        /// <summary>
        /// Gets marketplace offers from the XRP Ledger DEX
        /// </summary>
        /// <param name="type">he offer type (buy/sell)</param>
        /// <returns>A list of offers</returns>
        Task<List<object>> GetMarketplaceOffers(string type);
        
        /// <summary>
        /// Accepts an offer on the XRP Ledger DEX
        /// </summary>
        /// <param name="offerId">The offer ID</param>
        /// <param name="buyerAddress">The buyer address</param>
        /// <param name="secret">The buyer's secret key</param>
        /// <returns>The transaction result</returns>
        Task<string> AcceptOffer(string offerId, string buyerAddress, string secret);
    }
}