using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XRPAtom.Blockchain.Interfaces;
using XRPAtom.Core.DTOs;
using XRPAtom.Core.Interfaces;
using XRPAtom.Core.Repositories;
using XUMM.NET.SDK.Clients.Interfaces;
using XUMM.NET.SDK.Models.Payload;

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
        private readonly IXummPayloadClient _xummPayloadClient;
        private readonly ILogger<TransactionController> _logger;

        public TransactionController(
            IUserWalletService userWalletService,
            IXRPLTransactionService xrplTransactionService,
            ITransactionRepository transactionRepository,
            IXummPayloadClient xummPayloadClient,
            ILogger<TransactionController> logger)
        {
            _userWalletService = userWalletService;
            _xrplTransactionService = xrplTransactionService;
            _xummPayloadClient = xummPayloadClient;
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

        [HttpPost("create-xumm")]
        public async Task<IActionResult> CreateXummTransaction([FromBody] CreateTransactionDto request)
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

                // Create JSON payload for XUMM
                var payloadJson = $"{{ \"TransactionType\": \"Payment\", " +
                                  $"\"Destination\": \"{request.DestinationAddress}\", " +
                                  $"\"Amount\": \"{(long)(request.Amount * 1000000)}\" }}"; // Convert to drops

                if (request.Currency != "XRP")
                {
                    // For non-XRP currencies, use different format
                    payloadJson = $"{{ \"TransactionType\": \"Payment\", " +
                                 $"\"Destination\": \"{request.DestinationAddress}\", " +
                                 $"\"Amount\": {{ " +
                                 $"\"currency\": \"{request.Currency}\", " +
                                 $"\"issuer\": \"{request.DestinationAddress}\", " +
                                 $"\"value\": \"{request.Amount}\" " +
                                 $"}} }}";
                }

                var payload = new XummPostJsonPayload(payloadJson);

                // Add optional memo if provided
                if (!string.IsNullOrEmpty(request.Memo))
                {
                    payload.CustomMeta = new XummPayloadCustomMeta
                    {
                        Instruction = request.Memo
                    };
                }

                // Record transaction in our database as "pending_signature"
                var transaction = new XRPAtom.Core.Domain.Transaction
                {
                    SourceAddress = wallet.Address,
                    DestinationAddress = request.DestinationAddress,
                    Amount = request.Amount,
                    Currency = request.Currency ?? "XRP",
                    Type = "Payment",
                    Status = "pending_signature",
                    Timestamp = DateTime.UtcNow,
                    Memo = request.Memo,
                    Issuer = wallet.Address,
                    TransactionHash = "",
                    RelatedEntityId = "",
                    RelatedEntityType = "",
                    RawResponse = ""
                };

                await _transactionRepository.CreateTransactionAsync(transaction);

                // Create XUMM payload
                var response = await _xummPayloadClient.CreateAsync(payload);

                // Update transaction with payload reference
                await _transactionRepository.UpdateTransactionHash(transaction.Id, response.Uuid);

                return Ok(new
                {
                    transactionId = transaction.Id,
                    payloadId = response.Uuid,
                    qrUrl = response.Refs.QrPng,
                    deepLink = response.Next.Always,
                    websocket = response.Refs.WebsocketStatus
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating XUMM transaction");
                return StatusCode(500, new { error = "An error occurred while creating the transaction" });
            }
        }

        [HttpGet("xumm-status/{payloadId}")]
        public async Task<IActionResult> CheckXummPayloadStatus(string payloadId)
        {
            try
            {
                var payloadDetails = await _xummPayloadClient.GetAsync(payloadId);
                
                if (payloadDetails == null)
                {
                    return NotFound(new { error = "Payload not found" });
                }

                // If the payload is signed and completed, update our transaction record
                if (payloadDetails.Meta.Resolved && payloadDetails.Response?.Txid != null)
                {
                    // Find the transaction by payload ID (stored in our TransactionHash field temporarily)
                    var transaction = await _transactionRepository.GetByPayloadId(payloadId);
                    if (transaction != null)
                    {
                        // Update the transaction status and hash
                        await _transactionRepository.UpdateStatusAsync(transaction.Id, "submitted");
                        await _transactionRepository.UpdateTransactionHash(transaction.Id, payloadDetails.Response.Txid);
                    }
                }

                return Ok(new
                {
                    resolved = payloadDetails.Meta.Resolved,
                    signed = payloadDetails.Meta.Signed,
                    expired = payloadDetails.Meta.Expired,
                    txid = payloadDetails.Response?.Txid
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking XUMM payload status");
                return StatusCode(500, new { error = "An error occurred while checking the transaction status" });
            }
        }
    }
}