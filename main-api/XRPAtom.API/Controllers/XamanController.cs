using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using XRPAtom.Blockchain.Interfaces;
using XRPAtom.Blockchain.Models;
using System.Text.Json;

namespace XRPAtom.API.Controllers
{
    [ApiController]
    [Route("api/xaman")]
    public class XamanController : ControllerBase
    {
        private readonly IXamanService _xamanService;
        private readonly IXRPLTransactionService _transactionService;
        private readonly IUserWalletService _userWalletService;
        private readonly ILogger<XamanController> _logger;

        public XamanController(
            IXamanService xamanService,
            IXRPLTransactionService transactionService,
            IUserWalletService userWalletService,
            ILogger<XamanController> logger)
        {
            _xamanService = xamanService;
            _transactionService = transactionService;
            _userWalletService = userWalletService;
            _logger = logger;
        }

        [HttpPost("sign")]
        [Authorize]
        public async Task<IActionResult> RequestSignature([FromBody] MobileSignRequest request)
        {
            try {
                // Get user ID from claims
                var userId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(new { error = "Invalid user identifier" });
                }

                // Get user's wallet
                var wallet = await _userWalletService.GetWalletByUserIdAsync(userId);
                if (wallet == null)
                {
                    return BadRequest(new { error = "No wallet found for this user" });
                }

                // Prepare the transaction via the XRPL service
                var prepareResponse = await _transactionService.PrepareTransaction(new TransactionPrepareRequest {
                    TransactionType = request.TransactionType,
                    SourceAddress = wallet.Address,
                    DestinationAddress = request.DestinationAddress,
                    Amount = request.Amount,
                    Currency = request.Currency ?? "XRP"
                });

                if (!prepareResponse.Success)
                {
                    return BadRequest(new { error = prepareResponse.ErrorMessage });
                }

                // Create a sign request via Xaman
                var transaction = JsonSerializer.Deserialize<object>(prepareResponse.PreparedTransaction);
                var xamanRequest = new XamanPayloadRequest {
                    Txjson = transaction,
                    Options = new XamanPayloadRequest.RequestOptions {
                        ReturnUrl = true,
                        Submit = true,
                        Expire = true,
                        ExpireSeconds = 300 // 5 minutes
                    }
                };

                // If the user has a Xaman token, use it for push notification
                if (!string.IsNullOrEmpty(request.UserToken))
                {
                    xamanRequest.UserToken = new XamanPayloadRequest.UserTokenOptions {
                        Token = request.UserToken
                    };
                }

                // Create the sign request
                var xamanResponse = await _xamanService.CreateSignRequest(xamanRequest);
                
                // Return the response with QR code and deep link information
                return Ok(new {
                    payloadId = xamanResponse.Uuid,
                    qrUrl = xamanResponse.Refs.QrPng,
                    qrLink = xamanResponse.Refs.QrUrl,
                    websocketUrl = xamanResponse.Refs.WebsocketStatus,
                    pushed = xamanResponse.Pushed
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Xaman sign request");
                return StatusCode(500, new { error = "An error occurred while creating the sign request" });
            }
        }

        [HttpGet("status/{payloadId}")]
        public async Task<IActionResult> CheckStatus(string payloadId)
        {
            try {
                var status = await _xamanService.CheckPayloadStatus(payloadId);
                
                // If the transaction is signed, record it in our database
                if (status.Signed && !string.IsNullOrEmpty(status.Response?.Txid))
                {
                    // In a real implementation, this would record the transaction
                    // For now, we just log it
                    _logger.LogInformation("Transaction signed via Xaman: {TxId}", status.Response.Txid);
                }
                
                return Ok(new {
                    payloadId = status.Uuid,
                    signed = status.Signed,
                    expired = status.Expired,
                    resolved = status.Resolved,
                    transactionId = status.Response?.Txid,
                    transactionHex = status.Response?.Hex
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking Xaman payload status");
                return StatusCode(500, new { error = "An error occurred while checking the sign request status" });
            }
        }

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
    }

    public class MobileSignRequest
    {
        public string TransactionType { get; set; } // Payment, TrustSet, etc.
        public string DestinationAddress { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string UserToken { get; set; } // Optional Xaman push notification token
    }

    public class PayloadSubscribeRequest
    {
        public string PayloadId { get; set; }
        public string CallbackUrl { get; set; }
    }

    public class UserTokenRequest
    {
        public string Address { get; set; }
    }
}