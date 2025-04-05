using System.Threading.Tasks;
using XRPAtom.Blockchain.Models;

namespace XRPAtom.Blockchain.Interfaces
{
    public interface IXRPLTransactionService
    {
        /// <summary>
        /// Prepares a transaction by retrieving required fields from the XRPL
        /// </summary>
        /// <param name="request">The transaction preparation request details</param>
        /// <returns>A prepared transaction ready for signing</returns>
        Task<TransactionPrepareResponse> PrepareTransaction(TransactionPrepareRequest request);
        
        /// <summary>
        /// Submits a signed transaction to the XRP Ledger
        /// </summary>
        /// <param name="signedTransaction">The signed transaction blob</param>
        /// <returns>The submission response with transaction hash and status</returns>
        Task<TransactionSubmitResponse> SubmitSignedTransaction(string signedTransaction);
        
        /// <summary>
        /// Checks the status of a transaction on the XRP Ledger
        /// </summary>
        /// <param name="transactionHash">The transaction hash to check</param>
        /// <returns>The current transaction status</returns>
        Task<TransactionStatusResponse> CheckTransactionStatus(string transactionHash);
    }
}