using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XRPAtom.Blockchain.Interfaces;
using XRPAtom.Core.DTOs;
using System.Text.Json;

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

        public WalletController(
            IUserWalletService userWalletService,
            IXRPLedgerService xrplService,
            ILogger<WalletController> logger)
        {
            _userWalletService = userWalletService;
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
        
        [HttpPost]
        public async Task<IActionResult> CreateWallet()
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

                // Generate a new wallet on the XRP Ledger
                var walletJson = await _xrplService.GenerateWallet();
                var walletData = JsonSerializer.Deserialize<dynamic>(walletJson);

                string address = walletData.GetProperty("address").GetString();
                string publicKey = walletData.GetProperty("publicKey").GetString();
                string secret = walletData.GetProperty("secret").GetString();

                // Store the wallet in our database (but NOT the secret!)
                var createdWallet = await _userWalletService.CreateWalletAsync(userId, address, publicKey);

                // Return wallet information including the secret (only at creation time)
                return Ok(new
                {
                    address,
                    publicKey,
                    secret, // Only returned during wallet creation
                    message = "Wallet created successfully. Save your secret key securely!"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating wallet");
                return StatusCode(500, new { error = "An error occurred while creating the wallet" });
            }
        }

        [HttpPost("import")]
        public async Task<IActionResult> ImportWallet([FromBody] WalletImportDto request)
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

                // Validate the address format
                if (!IsValidXrpAddress(request.Address))
                {
                    return BadRequest(new { error = "Invalid XRP address format" });
                }

                // Verify that the address exists on the ledger
                try
                {
                    await _xrplService.GetAccountInfo(request.Address);
                }
                catch
                {
                    return BadRequest(new { error = "Unable to verify the XRP address on the ledger" });
                }

                // Store the wallet in our database
                var createdWallet = await _userWalletService.CreateWalletAsync(userId, request.Address, null);

                return Ok(new { address = request.Address, message = "Wallet imported successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing wallet");
                return StatusCode(500, new { error = "An error occurred while importing the wallet" });
            }
        }

        [HttpGet("balance")]
        public async Task<IActionResult> GetWalletBalance()
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(new { error = "Invalid user identifier" });
                }

                // Refresh the wallet balances from the ledger
                var success = await _userWalletService.RefreshWalletBalancesAsync(userId);
                if (!success)
                {
                    return NotFound(new { error = "Wallet not found or error refreshing balances" });
                }

                // Get the updated wallet
                var wallet = await _userWalletService.GetWalletByUserIdAsync(userId);

                return Ok(new
                {
                    balance = wallet.Balance,
                    atomTokenBalance = wallet.AtomTokenBalance,
                    address = wallet.Address,
                    lastUpdated = wallet.LastUpdated
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving wallet balance");
                return StatusCode(500, new { error = "An error occurred while retrieving the wallet balance" });
            }
        }

        private bool IsValidXrpAddress(string address)
        {
            // This is a simplified validation - in production, use a proper XRPL library validation
            return !string.IsNullOrEmpty(address) && 
                   address.StartsWith("r") && 
                   address.Length >= 25 && 
                   address.Length <= 35;
        }
    }
}