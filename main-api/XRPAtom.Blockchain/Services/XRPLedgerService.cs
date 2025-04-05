using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using XRPAtom.Blockchain.Interfaces;

namespace XRPAtom.Blockchain.Services
{
    public class XRPLedgerService : IXRPLedgerService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<XRPLedgerService> _logger;
        private readonly string _serverUrl;

        public XRPLedgerService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<XRPLedgerService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _serverUrl = configuration["XRPLedger:ServerUrl"] ?? "https://s.altnet.rippletest.net:51234/";
        }

        public async Task<string> GetAccountInfo(string address)
        {
            try
            {
                var request = new
                {
                    method = "account_info",
                    @params = new[]
                    {
                        new
                        {
                            account = address,
                            strict = true,
                            ledger_index = "current"
                        }
                    }
                };

                var response = await SendJsonRpcRequest(request);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving account info for {Address}", address);
                throw;
            }
        }

        public async Task<string> GenerateWallet()
        {
            try
            {
                var request = new
                {
                    method = "wallet_propose",
                    @params = new object[] { }
                };

                var response = await SendJsonRpcRequest(request);
                var result = JsonDocument.Parse(response).RootElement;

                if (!result.TryGetProperty("result", out var walletResult) || walletResult.TryGetProperty("error", out _))
                {
                    throw new Exception($"Failed to generate wallet: {response}");
                }

                // Format the result as a simplified object
                var wallet = new
                {
                    address = walletResult.GetProperty("account_id").GetString(),
                    publicKey = walletResult.GetProperty("public_key").GetString(),
                    secret = walletResult.GetProperty("master_seed").GetString()
                };

                return JsonSerializer.Serialize(wallet);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating wallet");
                throw;
            }
        }

        public async Task<string> SubmitTransaction(string transaction, string signature)
        {
            try
            {
                var request = new
                {
                    method = "submit",
                    @params = new[]
                    {
                        new
                        {
                            tx_blob = signature, // In XRPL, the signature is actually the signed transaction blob
                            fail_hard = false
                        }
                    }
                };

                var response = await SendJsonRpcRequest(request);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting transaction");
                throw;
            }
        }

        public async Task<string> CreatePayment(string sourceAddress, string destinationAddress, decimal amount, string sourceSecret)
        {
            try
            {
                // First, get the account sequence
                var accountInfo = await GetAccountInfo(sourceAddress);
                var accountInfoDoc = JsonDocument.Parse(accountInfo).RootElement;
                var accountData = accountInfoDoc.GetProperty("result").GetProperty("account_data");
                var sequence = accountData.GetProperty("Sequence").GetUInt32();

                // Get current ledger for LastLedgerSequence calculation
                var ledgerRequest = new
                {
                    method = "ledger_current",
                    @params = new object[] { }
                };

                var ledgerResponse = await SendJsonRpcRequest(ledgerRequest);
                var ledgerResult = JsonDocument.Parse(ledgerResponse).RootElement;
                var currentLedger = ledgerResult.GetProperty("result").GetProperty("ledger_current_index").GetUInt32();
                var lastLedgerSequence = currentLedger + 4; // Give 4 ledgers to process the tx

                // Build the transaction object
                var payment = new
                {
                    TransactionType = "Payment",
                    Account = sourceAddress,
                    Destination = destinationAddress,
                    Amount = Convert.ToString(Convert.ToUInt64(amount * 1000000)), // Convert to drops (1 XRP = 1,000,000 drops)
                    Sequence = sequence,
                    Fee = "12", // Standard fee in drops
                    LastLedgerSequence = lastLedgerSequence
                };

                // Sign the transaction
                var signingRequest = new
                {
                    method = "sign",
                    @params = new[]
                    {
                        new
                        {
                            tx_json = payment,
                            secret = sourceSecret
                        }
                    }
                };

                var signingResponse = await SendJsonRpcRequest(signingRequest);
                var signingResult = JsonDocument.Parse(signingResponse).RootElement;

                if (!signingResult.TryGetProperty("result", out var signResult) || signResult.TryGetProperty("error", out _))
                {
                    throw new Exception($"Failed to sign transaction: {signingResponse}");
                }

                var signedBlob = signResult.GetProperty("tx_blob").GetString();
                
                // Submit the signed transaction
                var submitRequest = new
                {
                    method = "submit",
                    @params = new[]
                    {
                        new
                        {
                            tx_blob = signedBlob,
                            fail_hard = false
                        }
                    }
                };

                var submitResponse = await SendJsonRpcRequest(submitRequest);
                return submitResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment from {Source} to {Destination}", sourceAddress, destinationAddress);
                throw;
            }
        }

        public async Task<bool> VerifyCurtailmentEvent(string eventId, string proof)
        {
            try
            {
                // In a real implementation, you would:
                // 1. Look up an XRPL transaction or NFT representing the event
                // 2. Verify the content matches the expected values
                // 3. Check cryptographic signatures if applicable
                
                // For demo purposes, just simulate verification
                await Task.Delay(100); // Simulate processing
                
                // We'll consider any non-empty proof as valid for demo
                return !string.IsNullOrEmpty(proof);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying curtailment event {EventId}", eventId);
                return false;
            }
        }

        public async Task<string> IssueReward(string destinationAddress, decimal amount, string eventId)
        {
            try
            {
                // In a real application, this would use a funded account to issue rewards
                // For demo purposes, we'll use a test account
                
                // Generate a test wallet for demo purposes
                var walletJson = await GenerateWallet();
                var wallet = JsonDocument.Parse(walletJson).RootElement;
                
                var sourceAddress = wallet.GetProperty("address").GetString();
                var sourceSecret = wallet.GetProperty("secret").GetString();
                
                // For a demo, we would need to fund this address first
                // In production, you would use a pre-funded treasury account
                
                // Create the payment transaction
                var paymentResult = await CreatePayment(sourceAddress, destinationAddress, amount, sourceSecret);
                var paymentResponse = JsonDocument.Parse(paymentResult).RootElement;
                
                if (!paymentResponse.GetProperty("result").TryGetProperty("tx_json", out var txJson))
                {
                    throw new Exception("Failed to issue reward payment");
                }
                
                // Return the transaction hash
                return txJson.GetProperty("hash").GetString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error issuing reward to {Address} for event {EventId}", destinationAddress, eventId);
                throw;
            }
        }

        public async Task<string> CreateMarketplaceOffer(string address, string offerType, decimal amount, decimal price, string secret)
        {
            try
            {
                // For simplicity, we'll focus on XRP/ATOM token offers
                // First, get the account sequence
                var accountInfo = await GetAccountInfo(address);
                var accountInfoDoc = JsonDocument.Parse(accountInfo).RootElement;
                var accountData = accountInfoDoc.GetProperty("result").GetProperty("account_data");
                var sequence = accountData.GetProperty("Sequence").GetUInt32();

                // Get current ledger for LastLedgerSequence calculation
                var ledgerRequest = new
                {
                    method = "ledger_current",
                    @params = new object[] { }
                };

                var ledgerResponse = await SendJsonRpcRequest(ledgerRequest);
                var ledgerResult = JsonDocument.Parse(ledgerResponse).RootElement;
                var currentLedger = ledgerResult.GetProperty("result").GetProperty("ledger_current_index").GetUInt32();
                var lastLedgerSequence = currentLedger + 4; // Give 4 ledgers to process the tx

                // Define the ATOM token issuer (in a production app, this would be your token issuer)
                string atomIssuer = "rAtomIssuerAddressHere";

                // Build the offer based on type
                object offer;
                if (offerType.ToLower() == "sell")
                {
                    // Selling ATOM for XRP
                    offer = new
                    {
                        TransactionType = "OfferCreate",
                        Account = address,
                        TakerGets = Convert.ToString(Convert.ToUInt64(price * 1000000)), // XRP amount in drops
                        TakerPays = new
                        {
                            currency = "ATOM",
                            issuer = atomIssuer,
                            value = amount.ToString()
                        },
                        Sequence = sequence,
                        Fee = "12",
                        LastLedgerSequence = lastLedgerSequence
                    };
                }
                else
                {
                    // Buying ATOM with XRP
                    offer = new
                    {
                        TransactionType = "OfferCreate",
                        Account = address,
                        TakerGets = new
                        {
                            currency = "ATOM",
                            issuer = atomIssuer,
                            value = amount.ToString()
                        },
                        TakerPays = Convert.ToString(Convert.ToUInt64(price * 1000000)), // XRP amount in drops
                        Sequence = sequence,
                        Fee = "12",
                        LastLedgerSequence = lastLedgerSequence
                    };
                }

                // Sign the transaction
                var signingRequest = new
                {
                    method = "sign",
                    @params = new[]
                    {
                        new
                        {
                            tx_json = offer,
                            secret = secret
                        }
                    }
                };

                var signingResponse = await SendJsonRpcRequest(signingRequest);
                var signingResult = JsonDocument.Parse(signingResponse).RootElement;

                if (!signingResult.TryGetProperty("result", out var signResult) || signResult.TryGetProperty("error", out _))
                {
                    throw new Exception($"Failed to sign offer: {signingResponse}");
                }

                var signedBlob = signResult.GetProperty("tx_blob").GetString();
                
                // Submit the signed transaction
                var submitRequest = new
                {
                    method = "submit",
                    @params = new[]
                    {
                        new
                        {
                            tx_blob = signedBlob,
                            fail_hard = false
                        }
                    }
                };

                var submitResponse = await SendJsonRpcRequest(submitRequest);
                var submitResult = JsonDocument.Parse(submitResponse).RootElement;
                
                // In a real implementation, you'd extract the offer sequence from the transaction
                return submitResult.GetProperty("result").GetProperty("tx_json").GetProperty("hash").GetString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating marketplace offer");
                throw;
            }
        }

        public async Task<List<object>> GetMarketplaceOffers(string type)
        {
            try
            {
                // For a simple implementation, we'll use book_offers to get the order book
                // Define the ATOM token issuer (in a production app, this would be your token issuer)
                string atomIssuer = "rAtomIssuerAddressHere";

                object request;
                if (type.ToLower() == "buy")
                {
                    // Get offers of XRP for ATOM
                    request = new
                    {
                        method = "book_offers",
                        @params = new[]
                        {
                            new
                            {
                                taker_gets = new
                                {
                                    currency = "ATOM",
                                    issuer = atomIssuer
                                },
                                taker_pays = new
                                {
                                    currency = "XRP"
                                },
                                limit = 100
                            }
                        }
                    };
                }
                else
                {
                    // Get offers of ATOM for XRP
                    request = new
                    {
                        method = "book_offers",
                        @params = new[]
                        {
                            new
                            {
                                taker_gets = new
                                {
                                    currency = "XRP"
                                },
                                taker_pays = new
                                {
                                    currency = "ATOM",
                                    issuer = atomIssuer
                                },
                                limit = 100
                            }
                        }
                    };
                }

                var response = await SendJsonRpcRequest(request);
                var result = JsonDocument.Parse(response).RootElement;

                if (!result.TryGetProperty("result", out var bookResult) || bookResult.TryGetProperty("error", out _))
                {
                    throw new Exception($"Failed to get order book: {response}");
                }

                // Format offers to a more user-friendly structure
                var offers = new List<object>();
                if (bookResult.TryGetProperty("offers", out var offersElement))
                {
                    foreach (var offer in offersElement.EnumerateArray())
                    {
                        // Extract key offer details
                        var account = offer.GetProperty("Account").GetString();
                        var sequence = offer.GetProperty("Sequence").GetUInt32();
                        
                        if (type.ToLower() == "buy")
                        {
                            // Buying ATOM with XRP
                            decimal takerPaysValue = offer.GetProperty("TakerPays").ValueKind == JsonValueKind.String
                                ? Convert.ToDecimal(offer.GetProperty("TakerPays").GetString()) / 1000000m // Convert drops to XRP
                                : decimal.Parse(offer.GetProperty("TakerPays").GetProperty("value").GetString());
                                
                            decimal takerGetsValue = decimal.Parse(offer.GetProperty("TakerGets").GetProperty("value").GetString());
                            
                            offers.Add(new
                            {
                                OfferId = $"{account}-{sequence}",
                                OfferType = "BUY",
                                Account = account,
                                Amount = takerGetsValue,
                                Price = takerPaysValue / takerGetsValue,
                                Total = takerPaysValue
                            });
                        }
                        else
                        {
                            // Selling ATOM for XRP
                            decimal takerGetsValue = offer.GetProperty("TakerGets").ValueKind == JsonValueKind.String
                                ? Convert.ToDecimal(offer.GetProperty("TakerGets").GetString()) / 1000000m // Convert drops to XRP
                                : decimal.Parse(offer.GetProperty("TakerGets").GetProperty("value").GetString());
                                
                            decimal takerPaysValue = decimal.Parse(offer.GetProperty("TakerPays").GetProperty("value").GetString());
                            
                            offers.Add(new
                            {
                                OfferId = $"{account}-{sequence}",
                                OfferType = "SELL",
                                Account = account,
                                Amount = takerPaysValue,
                                Price = takerGetsValue / takerPaysValue,
                                Total = takerGetsValue
                            });
                        }
                    }
                }

                return offers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting marketplace offers for type {Type}", type);
                throw;
            }
        }

        public async Task<string> AcceptOffer(string offerId, string buyerAddress, string secret)
        {
            try
            {
                // Parse the offerId to get account and sequence
                var parts = offerId.Split('-');
                if (parts.Length != 2)
                {
                    throw new ArgumentException("Invalid offer ID format");
                }
                
                string sellerAccount = parts[0];
                uint offerSequence = uint.Parse(parts[1]);
                
                // Get buyer's account info for sequence
                var accountInfo = await GetAccountInfo(buyerAddress);
                var accountInfoDoc = JsonDocument.Parse(accountInfo).RootElement;
                var accountData = accountInfoDoc.GetProperty("result").GetProperty("account_data");
                var sequence = accountData.GetProperty("Sequence").GetUInt32();

                // Get current ledger for LastLedgerSequence calculation
                var ledgerRequest = new
                {
                    method = "ledger_current",
                    @params = new object[] { }
                };

                var ledgerResponse = await SendJsonRpcRequest(ledgerRequest);
                var ledgerResult = JsonDocument.Parse(ledgerResponse).RootElement;
                var currentLedger = ledgerResult.GetProperty("result").GetProperty("ledger_current_index").GetUInt32();
                var lastLedgerSequence = currentLedger + 4; // Give 4 ledgers to process the tx

                // Create an OfferCreate that fully consumes the target offer
                var acceptOfferTx = new
                {
                    TransactionType = "OfferCreate",
                    Account = buyerAddress,
                    Sequence = sequence,
                    Fee = "12",
                    LastLedgerSequence = lastLedgerSequence,
                    Flags = 131072, // tfImmediateOrCancel - only match existing offers
                    OfferSequence = offerSequence // This is how we target a specific offer
                    // TakerGets and TakerPays would be set based on the offer details
                    // For simplicity, we're omitting this part which would require looking up the offer first
                };

                // Sign the transaction
                var signingRequest = new
                {
                    method = "sign",
                    @params = new[]
                    {
                        new
                        {
                            tx_json = acceptOfferTx,
                            secret = secret
                        }
                    }
                };

                var signingResponse = await SendJsonRpcRequest(signingRequest);
                var signingResult = JsonDocument.Parse(signingResponse).RootElement;

                if (!signingResult.TryGetProperty("result", out var signResult) || signResult.TryGetProperty("error", out _))
                {
                    throw new Exception($"Failed to sign offer acceptance: {signingResponse}");
                }

                var signedBlob = signResult.GetProperty("tx_blob").GetString();
                
                // Submit the signed transaction
                var submitRequest = new
                {
                    method = "submit",
                    @params = new[]
                    {
                        new
                        {
                            tx_blob = signedBlob,
                            fail_hard = false
                        }
                    }
                };

                var submitResponse = await SendJsonRpcRequest(submitRequest);
                var submitResult = JsonDocument.Parse(submitResponse).RootElement;
                
                return submitResult.GetProperty("result").GetProperty("tx_json").GetProperty("hash").GetString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accepting offer {OfferId}", offerId);
                throw;
            }
        }

        private async Task<string> SendJsonRpcRequest(object request)
        {
            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(_serverUrl, content);
            var responseBody = await response.Content.ReadAsStringAsync();
            
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"XRPL API error: {response.StatusCode}, {responseBody}");
            }
            
            return responseBody;
        }
        
        public async Task<string> GetLedgerCurrent()
        {
            try
            {
                var request = new
                {
                    method = "ledger_current",
                    @params = new object[] { }
                };

                var response = await SendJsonRpcRequest(request);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current ledger");
                throw;
            }
        }
    }
}