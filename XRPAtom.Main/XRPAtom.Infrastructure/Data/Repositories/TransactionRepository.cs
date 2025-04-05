using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using XRPAtom.Core.Domain;
using XRPAtom.Core.Repositories;
using XRPAtom.Infrastructure.Data;

namespace XRPAtom.Infrastructure.Data.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly ApplicationDbContext _context;

        public TransactionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Transaction> GetTransactionByIdAsync(string id)
        {
            return await _context.Transactions.FindAsync(id);
        }

        public async Task<Transaction> GetByTransactionHashAsync(string hash)
        {
            return await _context.Transactions
                .FirstOrDefaultAsync(t => t.TransactionHash == hash);
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsByAddressAsync(
            string address, 
            int page = 1, 
            int pageSize = 10)
        {
            return await _context.Transactions
                .Where(t => t.SourceAddress == address || t.DestinationAddress == address)
                .OrderByDescending(t => t.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTransactionCountByAddressAsync(string address)
        {
            return await _context.Transactions
                .CountAsync(t => t.SourceAddress == address || t.DestinationAddress == address);
        }

        public async Task<Transaction> CreateTransactionAsync(Transaction transaction)
        {
            // Ensure ID is generated if not provided
            if (string.IsNullOrEmpty(transaction.Id))
            {
                transaction.Id = Guid.NewGuid().ToString();
            }

            // Set timestamp if not set
            if (transaction.Timestamp == default)
            {
                transaction.Timestamp = DateTime.UtcNow;
            }

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
            return transaction;
        }

        public async Task<bool> UpdateStatusAsync(string id, string status)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            
            if (transaction == null)
            {
                return false;
            }

            transaction.Status = status;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateTransactionHashAsync(string id, string hash)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            
            if (transaction == null)
            {
                return false;
            }

            transaction.TransactionHash = hash;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Transaction>> GetPendingTransactionsAsync(int maxRetryCount = 3)
        {
            return await _context.Transactions
                .Where(t => 
                    (t.Status == "pending" || t.Status == "submitted") && 
                    t.RetryCount < maxRetryCount)
                .OrderBy(t => t.Timestamp)
                .ToListAsync();
        }

        public async Task<bool> IncrementRetryCountAsync(string id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            
            if (transaction == null)
            {
                return false;
            }

            transaction.RetryCount++;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Transaction>> GetTransactionsByEntityAsync(
            string entityId, 
            string entityType, 
            int page = 1, 
            int pageSize = 10)
        {
            return await _context.Transactions
                .Where(t => 
                    t.RelatedEntityId == entityId && 
                    t.RelatedEntityType == entityType)
                .OrderByDescending(t => t.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}