using XRPAtom.Core.Domain;

namespace XRPAtom.Core.Repositories
{
    /// <summary>
    /// Defines the contract for transaction-related database operations
    /// </summary>
    public interface ITransactionRepository
    {
        /// <summary>
        /// Retrieves a transaction by its unique identifier
        /// </summary>
        /// <param name="id">The unique transaction identifier</param>
        /// <returns>The transaction or null if not found</returns>
        Task<Transaction> GetTransactionByIdAsync(string id);

        /// <summary>
        /// Retrieves transactions associated with a specific wallet address
        /// </summary>
        /// <param name="address">The wallet address to retrieve transactions for</param>
        /// <param name="page">The page number for pagination</param>
        /// <param name="pageSize">Number of transactions per page</param>
        /// <returns>A collection of transactions</returns>
        Task<IEnumerable<Transaction>> GetTransactionsByAddressAsync(
            string address, 
            int page = 1, 
            int pageSize = 10);

        /// <summary>
        /// Counts the total number of transactions for a specific wallet address
        /// </summary>
        /// <param name="address">The wallet address to count transactions for</param>
        /// <returns>Total number of transactions</returns>
        Task<int> GetTransactionCountByAddressAsync(string address);

        /// <summary>
        /// Creates a new transaction in the database
        /// </summary>
        /// <param name="transaction">The transaction to create</param>
        /// <returns>The created transaction with any generated identifiers</returns>
        Task<Transaction> CreateTransactionAsync(Transaction transaction);

        /// <summary>
        /// Retrieves a transaction by its transaction hash
        /// </summary>
        /// <param name="hash">The unique transaction hash</param>
        /// <returns>The transaction or null if not found</returns>
        Task<Transaction> GetByTransactionHashAsync(string hash);

        /// <summary>
        /// Updates the status of an existing transaction
        /// </summary>
        /// <param name="id">The transaction identifier</param>
        /// <param name="status">The new status to set</param>
        /// <returns>True if update was successful, false otherwise</returns>
        Task<bool> UpdateStatusAsync(string id, string status);

        /// <summary>
        /// Retrieves transactions associated with a specific entity
        /// </summary>
        /// <param name="entityId">The unique identifier of the related entity</param>
        /// <param name="entityType">The type of the related entity</param>
        /// <param name="page">The page number for pagination</param>
        /// <param name="pageSize">Number of transactions per page</param>
        /// <returns>A collection of transactions</returns>
        Task<IEnumerable<Transaction>> GetTransactionsByEntityAsync(
            string entityId, 
            string entityType, 
            int page = 1, 
            int pageSize = 10);

        /// <summary>
        /// Retrieves pending transactions that haven't been fully processed
        /// </summary>
        /// <param name="maxRetryCount">Maximum number of retry attempts</param>
        /// <returns>A collection of pending transactions</returns>
        Task<IEnumerable<Transaction>> GetPendingTransactionsAsync(int maxRetryCount = 3);

        /// <summary>
        /// Increments the retry count for a specific transaction
        /// </summary>
        /// <param name="id">The transaction identifier</param>
        /// <returns>True if increment was successful, false otherwise</returns>
        Task<bool> IncrementRetryCountAsync(string id);

        /// <summary>
        /// Updates the transaction hash for a specific transaction
        /// </summary>
        /// <param name="id"></param>
        /// <param name="transactionHash"></param>
        /// <returns></returns>
        Task<bool> UpdateTransactionHash(string id, string transactionHash);
    }
}