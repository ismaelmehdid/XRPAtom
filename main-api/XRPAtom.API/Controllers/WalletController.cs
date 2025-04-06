using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XRPAtom.Blockchain.Interfaces;
using XRPAtom.Core.DTOs;
using System.Text.Json;
using XRPAtom.Core.Interfaces;
using XUMM.NET.SDK.Clients.Interfaces;
using XUMM.NET.SDK.Models.Misc;
using XUMM.NET.SDK.Models.Payload;

namespace XRPAtom.API.Controllers
{
    [ApiController]
    [Route("api/wallet")]
    [Authorize]
    public class WalletController : ControllerBase
    {
        private readonly IUserWalletService _userWalletService;
        private readonly IXRPLedgerService _xrplService;
        private readonly ILogger<WalletController> _logger;
        private readonly IXummPayloadClient _xummPayloadClient;

        public WalletController(
            IUserWalletService userWalletService,
            IXRPLedgerService xrplService,
            IXummPayloadClient xummPayloadClient,
            ILogger<WalletController> logger)
        {
            _userWalletService = userWalletService;
            _xummPayloadClient = xummPayloadClient;
            _xrplService = xrplService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetUserWallet()
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

                // Get real-time account info from the ledger
                var accountInfoJson = await _xrplService.GetAccountInfo(wallet.Address);
                var accountInfo = JsonSerializer.Deserialize<JsonElement>(accountInfoJson);
                
                // Get ATOM token balance too (if the user has a trustline)
                // In a real implementation, this would check for the specific token
                
                return Ok(new
                {
                    wallet,
                    ledgerInfo = accountInfo
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user wallet");
                return StatusCode(500, new { error = "An error occurred while retrieving wallet information" });
            }
        }
        
        [HttpPost("connect-xumm")]
        public async Task<IActionResult> ConnectXummWallet()
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(new { error = "Invalid user identifier" });
                }

                // Check if user already has a wallet
                var existingWallet = await _userWalletService.GetWalletByUserIdAsync(userId);
                if (existingWallet != null)
                {
                    return BadRequest(new { error = "User already has a wallet" });
                }

                // Create a unique identifier for this connection attempt
                string connectionIdentifier = $"xrpatom_connect_{userId}";

                // Create a simple Sign In transaction JSON
                // Note: This is a minimal Sign In transaction that XUMM can process
                var txJson = JsonSerializer.Serialize(new
                {
                    TransactionType = "SignIn",
                    SignInFor = "XRPAtom Wallet Linking",
                    Identifier = connectionIdentifier,
                    Instruction = "Sign to access XRPAtom"
                });

                // Create the payload with the transaction JSON
                var payload = new XummPostJsonPayload(txJson)
                {
                    Options = new XummPayloadOptions
                    {
                        ReturnUrl = new XummPayloadReturnUrl
                        {
                            Web = "https://app.zunix.systems",
                            App = "https://app.zunix.systems"
                        },
                        Submit = false,
                        Expire = 300
                    }
                };

                var response = await _xummPayloadClient.CreateAsync(payload, throwError: true);

                return Ok(new
                {
                    qrUrl = response.Refs.QrPng,
                    deepLink = response.Next.Always,
                    uuid = response.Uuid
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating XUMM connection payload");
                return StatusCode(500, new { error = "An error occurred while connecting with XUMM" });
            }
        }
        
        public class VerifyXummConnectionRequest
        {
            public string PayloadId { get; set; }
        }
        
        [HttpPost("verify-xumm-connection")]
        public async Task<IActionResult> VerifyXummConnection([FromBody] VerifyXummConnectionRequest request)
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(new { error = "Invalid user identifier" });
                }

                // Retrieve the payload details
                var payloadDetails = await _xummPayloadClient.GetAsync(request.PayloadId, throwError: true);
                
                if (payloadDetails == null)
                {
                    return BadRequest(new { error = "Invalid payload" });
                }

                // Check if payload is resolved (signed)
                if (!payloadDetails.Meta.Resolved)
                {
                    return BadRequest(new { error = "Payload not yet signed" });
                }
                
                // Verify the payload's custom identifier
                string expectedConnectionIdentifier = $"xrpatom_connect_{userId}";
                
                var parsed_json_request = JsonSerializer.Deserialize<JsonElement>(payloadDetails.Payload.RequestJson);
                // Check if the custom meta identifier matches
                if (parsed_json_request.GetProperty("Identifier").GetString() != expectedConnectionIdentifier)
                {
                    return BadRequest(new { error = "Invalid connection identifier" });
                }

                // Get the wallet address from the signed payload
                var walletAddress = payloadDetails.Response.Account;

                // Create a wallet for the user with this address
                var wallet = await _userWalletService.CreateWalletAsync(userId, walletAddress);

                return Ok(new { 
                    success = true, 
                    address = walletAddress,
                    message = "Xuman wallet connected successfully" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying XUMM connection");
                return StatusCode(500, new { error = "An error occurred while verifying XUMM connection" });
            }
        }
    }
}