using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XRPAtom.Core.DTOs;
using XRPAtom.Core.Interfaces;
using XRPAtom.Blockchain.Interfaces;

namespace XRPAtom.API.Controllers
{
    [ApiController]
    [Route("api/marketplace")]
    [Authorize]
    public class MarketplaceController : ControllerBase
    {
        private readonly IMarketplaceService _marketplaceService;
        private readonly IUserWalletService _userWalletService;
        private readonly IXRPLedgerService _xrplService;
        private readonly IXRPLTransactionService _transactionService;
        private readonly ILogger<MarketplaceController> _logger;

        public MarketplaceController(
            IMarketplaceService marketplaceService,
            IUserWalletService userWalletService,
            IXRPLedgerService xrplService,
            IXRPLTransactionService transactionService,
            ILogger<MarketplaceController> logger)
        {
            _marketplaceService = marketplaceService;
            _userWalletService = userWalletService;
            _xrplService = xrplService;
            _transactionService = transactionService;
            _logger = logger;
        }

        [HttpGet("listings")]
        public async Task<IActionResult> GetListings(
            [FromQuery] string type = "buy",
            [FromQuery] int page = 1,
            [FromQuery] int limit = 10)
        {
            try
            {
                if (type != "buy" && type != "sell")
                {
                    return BadRequest(new { error = "Type must be either 'buy' or 'sell'" });
                }

                var userId = User.FindFirst("sub")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(new { error = "Invalid user identifier" });
                }

                // Get marketplace listings from our database
                var listings = await _marketplaceService.GetListingsAsync(type, page, limit);
                
                // Get live listings from the XRP Ledger DEX
                // This would typically be done asynchronously or cached
                List<object> xrplOffers = new List<object>();
                try
                {
                    xrplOffers = await _xrplService.GetMarketplaceOffers(type);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to retrieve XRPL marketplace offers");
                }
                
                return Ok(new
                {
                    listings,
                    xrplOffers,
                    pagination = new
                    {
                        page,
                        limit
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving marketplace listings");
                return StatusCode(500, new { error = "An error occurred while retrieving marketplace listings" });
            }
        }

        [HttpGet("listings/{id}")]
        public async Task<IActionResult> GetListingById(string id)
        {
            try
            {
                var userId = User.FindFirst("sub")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(new { error = "Invalid user identifier" });
                }

                var listing = await _marketplaceService.GetListingByIdAsync(id);
                if (listing == null)
                {
                    return NotFound(new { error = "Listing not found" });
                }

                // Check if this is the user's own listing
                listing.IsOwner = await _marketplaceService.IsUserOwnedListingAsync(id, userId);

                return Ok(listing);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving marketplace listing");
                return StatusCode(500, new { error = "An error occurred while retrieving the marketplace listing" });
            }
        }

        [HttpGet("user-listings")]
        public async Task<IActionResult> GetUserListings([FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            try
            {
                var userId = User.FindFirst("sub")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(new { error = "Invalid user identifier" });
                }

                var listings = await _marketplaceService.GetUserListingsAsync(userId, page, limit);

                return Ok(new 
                { 
                    listings,
                    pagination = new
                    {
                        page,
                        limit
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user marketplace listings");
                return StatusCode(500, new { error = "An error occurred while retrieving user marketplace listings" });
            }
        }

        [HttpPost("listings")]
        public async Task<IActionResult> CreateListing([FromBody] CreateMarketplaceListingDto request)
        {
            try
            {
                var userId = User.FindFirst("sub")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(new { error = "Invalid user identifier" });
                }

                // Validate the request
                if (request.MaxKwh < request.MinKwh)
                {
                    return BadRequest(new { error = "Maximum kWh must be greater than or equal to minimum kWh" });
                }

                // Get user's wallet
                var wallet = await _userWalletService.GetWalletByUserIdAsync(userId);
                if (wallet == null)
                {
                    return BadRequest(new { error = "No wallet found for this user" });
                }

                // Create listing in the database
                var listing = await _marketplaceService.CreateListingAsync(request);

                // Create the corresponding offer on the XRP Ledger DEX
                // This would typically be done in a background job or using the Xaman signing flow
                // For demonstration purposes, we'll just simulate it
                string blockchainReference = Guid.NewGuid().ToString();

                // Update the listing with the blockchain reference
                await _marketplaceService.UpdateListingBlockchainReferenceAsync(listing.Id, blockchainReference);

                // Get the updated listing
                var updatedListing = await _marketplaceService.GetListingByIdAsync(listing.Id);

                return Ok(updatedListing);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating marketplace listing");
                return StatusCode(500, new { error = "An error occurred while creating the marketplace listing" });
            }
        }

        [HttpPut("listings/{id}")]
        public async Task<IActionResult> UpdateListing(string id, [FromBody] UpdateMarketplaceListingDto request)
        {
            try
            {
                var userId = User.FindFirst("sub")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(new { error = "Invalid user identifier" });
                }

                // Check if the listing exists
                var listing = await _marketplaceService.GetListingByIdAsync(id);
                if (listing == null)
                {
                    return NotFound(new { error = "Listing not found" });
                }

                // Check if the user owns this listing
                if (!await _marketplaceService.IsUserOwnedListingAsync(id, userId))
                {
                    return Forbid();
                }

                // Validate the request
                if (request is { MaxKwh: not null, MinKwh: not null } && request.MaxKwh.Value < request.MinKwh.Value)
                {
                    return BadRequest(new { error = "Maximum kWh must be greater than or equal to minimum kWh" });
                }

                // Update the listing
                var updatedListing = await _marketplaceService.UpdateListingAsync(id, request);

                // In a real application, you would also update the XRPL DEX offer
                // This would typically be done in a background job or using the Xaman signing flow

                return Ok(updatedListing);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating marketplace listing");
                return StatusCode(500, new { error = "An error occurred while updating the marketplace listing" });
            }
        }

        [HttpDelete("listings/{id}")]
        public async Task<IActionResult> DeleteListing(string id)
        {
            try
            {
                var userId = User.FindFirst("sub")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(new { error = "Invalid user identifier" });
                }

                // Check if the listing exists
                var listing = await _marketplaceService.GetListingByIdAsync(id);
                if (listing == null)
                {
                    return NotFound(new { error = "Listing not found" });
                }

                // Check if the user owns this listing
                if (!await _marketplaceService.IsUserOwnedListingAsync(id, userId))
                {
                    return Forbid();
                }

                // Delete the listing
                var success = await _marketplaceService.DeleteListingAsync(id);
                if (!success)
                {
                    return StatusCode(500, new { error = "Failed to delete listing" });
                }

                // In a real application, you would also cancel the XRPL DEX offer
                // This would typically be done in a background job or using the Xaman signing flow

                return Ok(new { success = true, message = "Listing deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting marketplace listing");
                return StatusCode(500, new { error = "An error occurred while deleting the marketplace listing" });
            }
        }

        [HttpPost("transactions")]
        public async Task<IActionResult> CreateTransaction([FromBody] CreateMarketplaceTransactionDto request)
        {
            try
            {
                var userId = User.FindFirst("sub")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(new { error = "Invalid user identifier" });
                }

                // Check if the listing exists
                var listing = await _marketplaceService.GetListingByIdAsync(request.ListingId);
                if (listing == null)
                {
                    return NotFound(new { error = "Listing not found" });
                }

                // Check if the listing is active
                if (listing.Status != "active")
                {
                    return BadRequest(new { error = "Listing is no longer active" });
                }

                // Validate the transaction
                if (request.Amount < listing.MinKwh || request.Amount > listing.MaxKwh)
                {
                    return BadRequest(new { error = $"Amount must be between {listing.MinKwh} and {listing.MaxKwh} kWh" });
                }

                // Check if the user can purchase this listing
                // For example, they shouldn't be able to purchase their own listing
                if (!await _marketplaceService.CanUserPurchaseListingAsync(listing.Id, userId, request.Amount))
                {
                    return BadRequest(new { error = "You cannot purchase this listing" });
                }

                // Get buyer's wallet
                var wallet = await _userWalletService.GetWalletByUserIdAsync(userId);
                if (wallet == null)
                {
                    return BadRequest(new { error = "No wallet found for this user" });
                }

                // Calculate total price
                decimal totalPrice = request.Amount * listing.PricePerKwh;

                // Create the transaction record
                var transaction = await _marketplaceService.CreateTransactionAsync(new CreateMarketplaceTransactionDto
                {
                    ListingId = listing.Id,
                    Amount = request.Amount,
                    Notes = request.Notes
                });

                // In a real application, you would now create a payment transaction on the XRPL
                // This would typically be done using the Xaman signing flow

                return Ok(transaction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating marketplace transaction");
                return StatusCode(500, new { error = "An error occurred while creating the marketplace transaction" });
            }
        }

        [HttpGet("transactions")]
        public async Task<IActionResult> GetTransactions([FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            try
            {
                var userId = User.FindFirst("sub")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(new { error = "Invalid user identifier" });
                }

                var transactions = await _marketplaceService.GetUserTransactionsAsync(userId, page, limit);

                return Ok(new
                {
                    transactions,
                    pagination = new
                    {
                        page,
                        limit
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving marketplace transactions");
                return StatusCode(500, new { error = "An error occurred while retrieving marketplace transactions" });
            }
        }

        [HttpGet("transactions/{id}")]
        public async Task<IActionResult> GetTransactionById(string id)
        {
            try
            {
                var userId = User.FindFirst("sub")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(new { error = "Invalid user identifier" });
                }

                var transaction = await _marketplaceService.GetTransactionByIdAsync(id);
                if (transaction == null)
                {
                    return NotFound(new { error = "Transaction not found" });
                }

                // Check if the user is involved in this transaction
                if (transaction.BuyerId != userId && transaction.SellerId != userId)
                {
                    return Forbid();
                }

                return Ok(transaction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving marketplace transaction");
                return StatusCode(500, new { error = "An error occurred while retrieving the marketplace transaction" });
            }
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetMarketStats()
        {
            try
            {
                var stats = new MarketplaceStatsDto
                {
                    AverageBuyPrice = await _marketplaceService.GetAveragePriceAsync("buy"),
                    AverageSellPrice = await _marketplaceService.GetAveragePriceAsync("sell"),
                    // Get other statistics
                    TotalVolumeLastDay = 0, // Placeholder
                    TotalVolumeLastWeek = 0, // Placeholder
                    TotalVolumeLastMonth = 0 // Placeholder
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving marketplace statistics");
                return StatusCode(500, new { error = "An error occurred while retrieving marketplace statistics" });
            }
        }

        [HttpPost("xrpl-prepare-offer")]
        public async Task<IActionResult> PrepareXrplOffer([FromBody] PrepareXrplOfferRequest request)
        {
            try
            {
                var userId = User.FindFirst("sub")?.Value;
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

                // Prepare the OfferCreate transaction
                var prepareResponse = await _transactionService.PrepareTransaction(new XRPAtom.Blockchain.Models.TransactionPrepareRequest
                {
                    TransactionType = "OfferCreate",
                    SourceAddress = wallet.Address,
                    
                    // For sell offers: selling ATOM tokens for XRP
                    GetsCurrency = request.OfferType == "sell" ? "XRP" : "ATOM",
                    GetsIssuer = request.OfferType == "sell" ? null : "rATOMIssuerAddress",
                    GetsAmount = request.OfferType == "sell" ? request.TotalPrice : request.Amount,
                    
                    // For buy offers: buying ATOM tokens with XRP
                    PaysCurrency = request.OfferType == "buy" ? "ATOM" : "XRP",
                    PaysIssuer = request.OfferType == "buy" ? "rATOMIssuerAddress" : null,
                    PaysAmount = request.OfferType == "buy" ? request.Amount : request.TotalPrice
                });

                if (!prepareResponse.Success)
                {
                    return BadRequest(new { error = prepareResponse.ErrorMessage });
                }

                return Ok(prepareResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error preparing XRPL offer");
                return StatusCode(500, new { error = "An error occurred while preparing the XRPL offer" });
            }
        }
    }

    public class PrepareXrplOfferRequest
    {
        public string? OfferType { get; set; } // "buy" or "sell"
        public decimal Amount { get; set; } // Amount of energy flexibility in kWh
        public decimal PricePerKwh { get; set; } // Price per kWh in XRP
        public decimal TotalPrice { get; set; } // Total price in XRP
    }
}