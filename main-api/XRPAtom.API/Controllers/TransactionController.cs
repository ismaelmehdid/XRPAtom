using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XRPAtom.Blockchain.Interfaces;
using XRPAtom.Core.DTOs;
using XRPAtom.Core.Repositories;

namespace XRPAtom.API.Controllers
{
    [ApiController]
    [Route("api/transactions")]
    [Authorize]
    public class TransactionController : ControllerBase
    {
        private readonly IUserWalletService _userWalletService;
        private readonly IXRPLTransactionService _xrplTransactionService;
        private readonly ITransactionRepository _transactionRepository;
        private readonly ILogger<TransactionController> _logger;

        public TransactionController(
            IUserWalletService userWalletService,
            IXRPLTransactionService xrplTransactionService,
            ITransactionRepository transactionRepository,
            ILogger<TransactionController> logger)
        {
            _userWalletService = userWalletService;
            _xrplTransactionService = xrplTransactionService;
            _transactionRepository = transactionRepository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetTransactions([FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(new { error = "Invalid user identifier" });
                }

                var wallet = await _userWalletService.GetWalletByUserIdAsync(userId);
                if (wallet == null)
                {
                    return NotFound(new { error = "Wallet not found for this user" });
                }

                var transactions = await _transactionRepository.GetTransactionsByAddressAsync(wallet.Address, page, limit);
                var total = await _transactionRepository.GetTransactionCountByAddressAsync(wallet.Address);

                // Convert to DTOs
                var transactionDtos = new List<WalletTransactionDto>();
                foreach (var tx in transactions)
                {
                    transactionDtos.Add(new WalletTransactionDto
                    {
                        Id = tx.Id,
                        SourceAddress = tx.SourceAddress,
                        DestinationAddress = tx.DestinationAddress,
                        Amount = tx.Amount,
                        Currency = tx.Currency,
                        Type = tx.Type,
                        Status = tx.Status,
                        TransactionHash = tx.TransactionHash,
                        Timestamp = tx.Timestamp,
                        Memo = tx.Memo
                    });
                }

                return Ok(new
                {
                    transactions = transactionDtos,
                    pagination = new
                    {
                        total,
                        page,
                        limit,
                        pages = (int)Math.Ceiling((double)total / limit)
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transactions");
                return StatusCode(500, new { error = "An error occurred while retrieving transactions" });
            }
        }

        [HttpPost("prepare")]
        public async Task<IActionResult> PrepareTransaction([FromBody] CreateTransactionDto request)
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(new { error = "Invalid user identifier" });
                }

                var wallet = await _userWalletService.GetWalletByUserIdAsync(userId);
                if (wallet == null)
                {
                    return NotFound(new { error = "Wallet not found for this user" });
                }

                // Prepare the transaction
                var prepareResponse = await _xrplTransactionService.PrepareTransaction(new XRPAtom.Blockchain.Models.TransactionPrepareRequest
                {
                    TransactionType = "Payment",
                    SourceAddress = wallet.Address,
                    DestinationAddress = request.DestinationAddress,
                    Amount = request.Amount,
                    Currency = request.Currency ?? "XRP"
                });

                if (!prepareResponse.Success)
                {
                    return BadRequest(new { error = prepareResponse.ErrorMessage });
                }

                return Ok(prepareResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error preparing transaction");
                return StatusCode(500, new { error = "An error occurred while preparing the transaction" });
            }
        }

        [HttpPost("submit")]
        public async Task<IActionResult> SubmitTransaction([FromBody] SubmitTransactionRequest request)
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(new { error = "Invalid user identifier" });
                }

                // Submit the signed transaction
                var submitResponse = await _xrplTransactionService.SubmitSignedTransaction(request.SignedTransaction);

                if (!submitResponse.Success)
                {
                    return BadRequest(new { error = submitResponse.ErrorMessage ?? submitResponse.EngineResultMessage });
                }

                // Record the transaction in our database
                // In a production app, you would extract all details from the transaction
                await _transactionRepository.CreateTransactionAsync(new XRPAtom.Core.Domain.Transaction
                {
                    SourceAddress = request.SourceAddress,
                    DestinationAddress = request.DestinationAddress,
                    Amount = request.Amount,
                    Currency = request.Currency ?? "XRP",
                    Type = "Payment",
                    Status = "submitted", // Will be updated by background job
                    TransactionHash = submitResponse.TransactionHash,
                    Timestamp = DateTime.UtcNow,
                    Memo = request.Memo
                });

                // Return the transaction hash
                return Ok(new
                {
                    success = true,
                    transactionHash = submitResponse.TransactionHash
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting transaction");
                return StatusCode(500, new { error = "An error occurred while submitting the transaction" });
            }
        }

        [HttpGet("status/{transactionHash}")]
        public async Task<IActionResult> CheckTransactionStatus(string transactionHash)
        {
            try
            {
                var response = await _xrplTransactionService.CheckTransactionStatus(transactionHash);

                if (!response.Success && response.Status == "error")
                {
                    return BadRequest(new { error = response.Message });
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking transaction status");
                return StatusCode(500, new { error = "An error occurred while checking the transaction status" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTransactionDetails(string id)
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(new { error = "Invalid user identifier" });
                }

                var wallet = await _userWalletService.GetWalletByUserIdAsync(userId);
                if (wallet == null)
                {
                    return NotFound(new { error = "Wallet not found for this user" });
                }

                var transaction = await _transactionRepository.GetTransactionByIdAsync(id);
                if (transaction == null)
                {
                    return NotFound(new { error = "Transaction not found" });
                }

                // Security check: ensure the user is authorized to view this transaction
                if (transaction.SourceAddress != wallet.Address && transaction.DestinationAddress != wallet.Address)
                {
                    return Forbid();
                }

                // Convert to DTO
                var transactionDto = new WalletTransactionDto
                {
                    Id = transaction.Id,
                    SourceAddress = transaction.SourceAddress,
                    DestinationAddress = transaction.DestinationAddress,
                    Amount = transaction.Amount,
                    Currency = transaction.Currency,
                    Type = transaction.Type,
                    Status = transaction.Status,
                    TransactionHash = transaction.TransactionHash,
                    Timestamp = transaction.Timestamp,
                    Memo = transaction.Memo
                };

                return Ok(transactionDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transaction details");
                return StatusCode(500, new { error = "An error occurred while retrieving transaction details" });
            }
        }
    }

    public class SubmitTransactionRequest
    {
        public string SignedTransaction { get; set; }
        public string SourceAddress { get; set; }
        public string DestinationAddress { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Memo { get; set; }
    }
}