using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using XRPAtom.Blockchain.Interfaces;
using XRPAtom.Blockchain.Models;
using XRPAtom.Blockchain.Services;
using XRPAtom.Core.Repositories;

namespace XRPAtom.API.Controllers
{
    [ApiController]
    [Route("api/xaman")]
    public class XamanController : ControllerBase
    {
        private readonly IXamanService _xamanService;
        private readonly IXRPLedgerService _xrplService;
        private readonly IUserWalletService _userWalletService;
        private readonly ITransactionRepository _transactionRepository;
        private readonly ILogger<XamanController> _logger;

        public XamanController(
            IXamanService xamanService,
            IXRPLedgerService xrplService,
            IUserWalletService userWalletService,
            ITransactionRepository transactionRepository,
            ILogger<XamanController> logger)
        {
            _xamanService = xamanService;
            _xrplService = xrplService;
            _userWalletService = userWalletService;
            _transactionRepository = transactionRepository;
            _logger = logger;
        }

        /// <summary>
        /// Creates a payment transaction sign request for Xaman
        /// </summary>
        [HttpPost("payment")]
        [Authorize]
        public async Task<IActionResult> CreatePayment([FromBody] PaymentRequest request)
        {
            try
            {
                // Get user's wallet address
                var userId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { error = "Invalid user identifier" });
                }

                var wallet = await _userWalletService.GetWalletByUserIdAsync(userId);
                if (wallet == null)
                {
                    return BadRequest(new { error = "No wallet found for this user" });
                }

                // Validate the destination address
                if (!IsValidXrpAddress(request.DestinationAddress))
                {
                    return BadRequest(new { error = "Invalid destination address format" });
                }

                // Prepare the transaction request
                var transactionRequest = new TransactionPrepareRequest
                {
                    TransactionType = "Payment",
                    SourceAddress = wallet.Address,
                    DestinationAddress = request.DestinationAddress,
                    Amount = request.Amount,
                    Currency = request.Currency ?? "XRP"
                };

                // For non-XRP currencies, include the issuer
                if (transactionRequest.Currency != "XRP" && !string.IsNullOrEmpty(request.Issuer))
                {
                    transactionRequest.Issuer = request.Issuer;
                }

                // Create a sign request via Xaman
                var signRequest = await ((XamanService)_xamanService).PrepareTransaction(
                    transactionRequest,
                    _xrplService,
                    request.Memo);

                // Store the transaction details for tracking
                var transaction = new XRPAtom.Core.Domain.Transaction
                {
                    SourceAddress = wallet.Address,
                    DestinationAddress = request.DestinationAddress,
                    Amount = request.Amount,
                    Currency = request.Currency ?? "XRP",
                    Type = "Payment",
                    Status = "pending_signature", // Will be updated when signed
                    Timestamp = DateTime.UtcNow,
                    Memo = request.Memo,
                    RelatedEntityId = signRequest.PayloadId,
                    RelatedEntityType = "xaman_payload"
                };

                // Save the pending transaction
                await _transactionRepository.CreateTransactionAsync(transaction);

                // Return the sign request details to the client
                return Ok(new
                {
                    payloadId = signRequest.PayloadId,
                    qrCodeUrl = signRequest.QrCodeUrl,
                    deepLink = signRequest.DeepLinkUrl,
                    websocketUrl = signRequest.WebsocketUrl,
                    pushed = signRequest.PushNotificationSent,
                    transactionId = transaction.Id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment sign request");
                return StatusCode(500, new { error = "An error occurred while creating the payment request" });
            }
        }

        /// <summary>
        /// Creates a trust line for tokens
        /// </summary>
        [HttpPost("trustset")]
        [Authorize]
        public async Task<IActionResult> CreateTrustLine([FromBody] TrustSetRequest request)
        {
            try
            {
                // Get user's wallet address
                var userId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { error = "Invalid user identifier" });
                }

                var wallet = await _userWalletService.GetWalletByUserIdAsync(userId);
                if (wallet == null)
                {
                    return BadRequest(new { error = "No wallet found for this user" });
                }

                // Validate the issuer address
                if (!IsValidXrpAddress(request.Issuer))
                {
                    return BadRequest(new { error = "Invalid issuer address format" });
                }

                // Prepare the transaction request
                var transactionRequest = new TransactionPrepareRequest
                {
                    TransactionType = "TrustSet",
                    SourceAddress = wallet.Address,
                    Currency = request.Currency,
                    Issuer = request.Issuer,
                    Amount = request.Limit
                };

                // Create a sign request via Xaman
                var signRequest = await ((XamanService)_xamanService).PrepareTransaction(
                    transactionRequest,
                    _xrplService);

                // Store the transaction details for tracking
                var transaction = new XRPAtom.Core.Domain.Transaction
                {
                    SourceAddress = wallet.Address,
                    DestinationAddress = request.Issuer, // For TrustSet, destination is the issuer
                    Amount = request.Limit,
                    Currency = request.Currency,
                    Issuer = request.Issuer,
                    Type = "TrustSet",
                    Status = "pending_signature", // Will be updated when signed
                    Timestamp = DateTime.UtcNow,
                    RelatedEntityId = signRequest.PayloadId,
                    RelatedEntityType = "xaman_payload"
                };

                // Save the pending transaction
                await _transactionRepository.CreateTransactionAsync(transaction);

                // Return the sign request details to the client
                return Ok(new
                {
                    payloadId = signRequest.PayloadId,
                    qrCodeUrl = signRequest.QrCodeUrl,
                    deepLink = signRequest.DeepLinkUrl,
                    websocketUrl = signRequest.WebsocketUrl,
                    pushed = signRequest.PushNotificationSent,
                    transactionId = transaction.Id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating trust line sign request");
                return StatusCode(500, new { error = "An error occurred while creating the trust line request" });
            }
        }

        /// <summary>
        /// Creates a marketplace offer
        /// </summary>
        [HttpPost("offer")]
        [Authorize]
        public async Task<IActionResult> CreateOffer([FromBody] OfferRequest request)
        {
            try
            {
                // Get user's wallet address
                var userId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { error = "Invalid user identifier" });
                }

                var wallet = await _userWalletService.GetWalletByUserIdAsync(userId);
                if (wallet == null)
                {
                    return BadRequest(new { error = "No wallet found for this user" });
                }

                // Validate the issuer address if token is involved
                if (request.OfferType == "buy" && request.TokenIssuer != null && !IsValidXrpAddress(request.TokenIssuer))
                {
                    return BadRequest(new { error = "Invalid token issuer address format" });
                }

                // Prepare the transaction request
                var transactionRequest = new TransactionPrepareRequest
                {
                    TransactionType = "OfferCreate",
                    SourceAddress = wallet.Address,
                };

                // Set up the offer based on type (buy or sell)
                if (request.OfferType.ToLower() == "buy")
                {
                    // Buying tokens with XRP
                    transactionRequest.GetsCurrency = request.TokenCurrency;
                    transactionRequest.GetsIssuer = request.TokenIssuer;
                    transactionRequest.GetsAmount = request.Amount;
                    
                    transactionRequest.PaysCurrency = "XRP";
                    transactionRequest.PaysAmount = request.TotalPrice;
                }
                else // sell
                {
                    // Selling tokens for XRP
                    transactionRequest.GetsCurrency = "XRP";
                    transactionRequest.GetsAmount = request.TotalPrice;
                    
                    transactionRequest.PaysCurrency = request.TokenCurrency;
                    transactionRequest.PaysIssuer = request.TokenIssuer;
                    transactionRequest.PaysAmount = request.Amount;
                }

                // Create a sign request via Xaman
                var signRequest = await ((XamanService)_xamanService).PrepareTransaction(
                    transactionRequest,
                    _xrplService);

                // Store the transaction details for tracking
                var transaction = new XRPAtom.Core.Domain.Transaction
                {
                    SourceAddress = wallet.Address,
                    Amount = request.Amount,
                    Currency = request.TokenCurrency,
                    Issuer = request.TokenIssuer,
                    Type = "OfferCreate",
                    Status = "pending_signature", // Will be updated when signed
                    Timestamp = DateTime.UtcNow,
                    RelatedEntityId = signRequest.PayloadId,
                    RelatedEntityType = "xaman_payload"
                };

                // Save the pending transaction
                await _transactionRepository.CreateTransactionAsync(transaction);

                // Return the sign request details to the client
                return Ok(new
                {
                    payloadId = signRequest.PayloadId,
                    qrCodeUrl = signRequest.QrCodeUrl,
                    deepLink = signRequest.DeepLinkUrl,
                    websocketUrl = signRequest.WebsocketUrl,
                    pushed = signRequest.PushNotificationSent,
                    transactionId = transaction.Id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating offer sign request");
                return StatusCode(500, new { error = "An error occurred while creating the offer request" });
            }
        }

        /// <summary>
        /// Checks the status of a Xaman sign request
        /// </summary>
        [HttpGet("status/{payloadId}")]
        public async Task<IActionResult> CheckStatus(string payloadId)
        {
            try
            {
                var status = await _xamanService.CheckPayloadStatus(payloadId);
                
                // If the transaction is signed, update our database record
                if (status.Signed && !string.IsNullOrEmpty(status.Response?.Txid))
                {
                    // Find the transaction by payload ID
                    var transactions = await _transactionRepository.GetTransactionsByEntityAsync(
                        payloadId, "xaman_payload");
        
                    var transaction = transactions.FirstOrDefault();
    
                    if (transaction != null)
                    {
                        // Update transaction status and hash
                        await _transactionRepository.UpdateStatusAsync(transaction.Id, "submitted");
                        await _transactionRepository.UpdateTransactionHash(transaction.Id, status.Response.Txid);
                    }
                }
                
                return Ok(new
                {
                    payloadId = status.Uuid,
                    signed = status.Signed,
                    expired = status.Expired,
                    resolved = status.Resolved,
                    transactionId = status.Response?.Txid,
                    transactionBlob = status.Response?.Hex
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking Xaman payload status");
                return StatusCode(500, new { error = "An error occurred while checking the sign request status" });
            }
        }

        /// <summary>
        /// Subscribes to a payload for real-time updates
        /// </summary>
        [HttpPost("subscribe")]
        [Authorize]
        public async Task<IActionResult> SubscribeToPayload([FromBody] PayloadSubscribeRequest request)
        {
            try
            {
                bool success = await _xamanService.SubscribeToPayload(request.PayloadId, request.CallbackUrl);
                return Ok(new { success });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error subscribing to payload");
                return StatusCode(500, new { error = "An error occurred while subscribing to the payload" });
            }
        }

        /// <summary>
        /// Gets a user token for push notifications
        /// </summary>
        [HttpPost("user-token")]
        [Authorize]
        public async Task<IActionResult> GetUserToken([FromBody] UserTokenRequest request)
        {
            try
            {
                string token = await _xamanService.GetUserToken(request.Address);
                return Ok(new { token });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user token");
                return StatusCode(500, new { error = "An error occurred while getting the user token" });
            }
        }

        /// <summary>
        /// Create a request to link a user's Xaman wallet
        /// </summary>
        [HttpPost("link-request")]
        [Authorize]
        public async Task<IActionResult> CreateLinkRequest()
        {
            try
            {
                // Get the user ID from the JWT token
                var userId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { error = "Invalid user identifier" });
                }

                // Check if user already has a wallet
                var existingWallet = await _userWalletService.GetWalletByUserIdAsync(userId);
                if (existingWallet != null)
                {
                    return BadRequest(new { error = "User already has a wallet" });
                }

                // Create a simple payload asking the user to sign to verify wallet ownership
                // This is a simple custom payload that just asks the user to sign a message
                var payloadRequest = new XamanPayloadRequest
                {
                    Txjson = new
                    {
                        TransactionType = "SignIn", // Custom transaction type for identification only
                        SignInFor = "XRPAtom Wallet Linking"
                    },
                    Options = new XamanPayloadRequest.RequestOptions
                    {
                        ReturnUrl = true,
                        Submit = false, // We don't actually submit this to the ledger
                        Expire = true,
                        ExpireSeconds = 300 // 5 minute expiration
                    }
                };

                // Create the sign request in Xaman
                var response = await _xamanService.CreateSignRequest(payloadRequest);

                // Return the necessary info for QR code display
                return Ok(new
                {
                    payloadId = response.Uuid,
                    qrCodeUrl = response.Refs.QrPng,
                    websocketUrl = response.Refs.WebsocketStatus
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Xaman linking request");
                return StatusCode(500, new { error = "An error occurred while creating the linking request" });
            }
        }

        /// <summary>
        /// Finalizes the wallet linking after a user has signed with Xaman
        /// </summary>
        [HttpPost("finalize-link")]
        [Authorize]
        public async Task<IActionResult> FinalizeLinking([FromBody] FinalizeLinkRequest request)
        {
            try
            {
                // Get the user ID from the JWT token
                var userId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { error = "Invalid user identifier" });
                }

                // Check if user already has a wallet
                var existingWallet = await _userWalletService.GetWalletByUserIdAsync(userId);
                if (existingWallet != null)
                {
                    return BadRequest(new { error = "User already has a wallet" });
                }

                // Get the payload status to verify
                var status = await _xamanService.CheckPayloadStatus(request.PayloadId);
                
                // Verify the payload was signed
                if (!status.Signed)
                {
                    return BadRequest(new { error = "Xaman request was not signed" });
                }

                // Extract the account from the payload
                string accountAddress;
                if (status.Response != null)
                {
                    // The actual account would be available in the response
                    // For a better implementation, you would verify this from the signed blob
                    accountAddress = request.Address; // In a real implementation, extract this from the signed payload
                }
                else
                {
                    return BadRequest(new { error = "Could not extract account address from payload" });
                }

                // Check if the address is valid
                if (!IsValidXrpAddress(accountAddress))
                {
                    return BadRequest(new { error = "Invalid XRP address format" });
                }

                // Verify the account exists on the ledger
                try
                {
                    await _xrplService.GetAccountInfo(accountAddress);
                }
                catch
                {
                    return BadRequest(new { error = "Unable to verify the XRP address on the ledger" });
                }

                // Create the wallet for the user
                var createdWallet = await _userWalletService.CreateWalletAsync(userId, accountAddress); 

                // Return the wallet information
                return Ok(new 
                { 
                    wallet = createdWallet,
                    message = "Wallet linked successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finalizing Xaman wallet linking");
                return StatusCode(500, new { error = "An error occurred while linking the wallet" });
            }
        }
        
        /// <summary>
        /// Validates an XRP address format
        /// </summary>
        private bool IsValidXrpAddress(string address)
        {
            // Basic validation - addresses start with r and are 25-35 characters
            return !string.IsNullOrEmpty(address) && 
                   address.StartsWith("r") && 
                   address.Length >= 25 && 
                   address.Length <= 35;
        }
    }

    #region Request Models

    public class PaymentRequest
    {
        public string DestinationAddress { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "XRP";
        public string Issuer { get; set; } // Only for non-XRP currencies
        public string Memo { get; set; }
        public string UserToken { get; set; } // Optional Xaman push notification token
    }

    public class TrustSetRequest
    {
        public string Currency { get; set; }
        public string Issuer { get; set; }
        public decimal Limit { get; set; }
        public string UserToken { get; set; } // Optional Xaman push notification token
    }

    public class OfferRequest
    {
        public string OfferType { get; set; } // "buy" or "sell"
        public string TokenCurrency { get; set; }
        public string TokenIssuer { get; set; }
        public decimal Amount { get; set; } // Token amount
        public decimal Price { get; set; } // Price per token in XRP
        public decimal TotalPrice { get; set; } // Total XRP amount
        public string UserToken { get; set; } // Optional Xaman push notification token
    }

    public class PayloadSubscribeRequest
    {
        public string PayloadId { get; set; }
        public string CallbackUrl { get; set; }
    }

    public class FinalizeLinkRequest
    {
        public string PayloadId { get; set; }
        public string Address { get; set; }
    }
    
    public class UserTokenRequest
    {
        public string Address { get; set; }
    }

    #endregion
}