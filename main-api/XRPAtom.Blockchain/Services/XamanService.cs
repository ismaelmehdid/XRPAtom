using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using XRPAtom.Blockchain.Interfaces;
using XRPAtom.Blockchain.Models;

namespace XRPAtom.Blockchain.Services
{
    /// <summary>
    /// Enhanced XamanService for handling transaction signing via the Xaman app
    /// </summary>
    public class XamanService : IXamanService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<XamanService> _logger;
        private readonly string _apiKey;
        private readonly string _apiSecret;
        private readonly string _serverUrl;

        public XamanService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<XamanService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _apiKey = configuration["Xaman:ApiKey"];
            _apiSecret = configuration["Xaman:ApiSecret"];
            _serverUrl = configuration["Xaman:ApiUrl"] ?? "https://xumm.app/api/v1/";
            
            // Configure HttpClient for Xaman API
            if (!string.IsNullOrEmpty(_apiKey))
                _httpClient.DefaultRequestHeaders.Add("X-API-Key", _apiKey);
                
            if (!string.IsNullOrEmpty(_apiSecret))
                _httpClient.DefaultRequestHeaders.Add("X-API-Secret", _apiSecret);
        }

        /// <summary>
        /// Creates a sign request for the Xaman app
        /// </summary>
        public async Task<XamanPayloadResponse> CreateSignRequest(XamanPayloadRequest request)
        {
            try
            {
                var content = new StringContent(
                    JsonSerializer.Serialize(request),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync($"{_serverUrl}platform/payload", content);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<XamanPayloadResponse>(responseBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Xaman sign request");
                throw;
            }
        }

        /// <summary>
        /// Checks the status of a Xaman payload
        /// </summary>
        public async Task<XamanPayloadStatus> CheckPayloadStatus(string payloadId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_serverUrl}platform/payload/{payloadId}");
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<XamanPayloadStatus>(responseBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking Xaman payload status for {PayloadId}", payloadId);
                throw;
            }
        }

        /// <summary>
        /// Subscribes to a Xaman payload for real-time updates
        /// </summary>
        public async Task<bool> SubscribeToPayload(string payloadId, string callbackUrl = null)
        {
            try
            {
                // If no callback URL is provided, we can't subscribe
                if (string.IsNullOrEmpty(callbackUrl))
                {
                    _logger.LogWarning("No callback URL provided for payload subscription");
                    return false;
                }

                var request = new
                {
                    url = callbackUrl
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(request),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync($"{_serverUrl}platform/payload/{payloadId}/subscribe", content);
                response.EnsureSuccessStatusCode();

                // If we get here, the subscription was successful
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error subscribing to Xaman payload {PayloadId}", payloadId);
                return false;
            }
        }

        /// <summary>
        /// Retrieves a user token for push notifications
        /// </summary>
        public async Task<string> GetUserToken(string userAddress)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_serverUrl}platform/xapp/user/{userAddress}");
                
                // If the user doesn't have a token, return null
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
                
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                var result = JsonDocument.Parse(responseBody).RootElement;

                // Extract the user token from the response
                if (result.TryGetProperty("user_token", out var userToken))
                {
                    return userToken.GetString();
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Xaman user token for {Address}", userAddress);
                return null;
            }
        }
        
        public async Task<XamanSignRequest> CreateWalletLinkingRequest(string callbackUrl = null)
        {
            // Create a custom payload for wallet linking
            var xamanPayloadRequest = new XamanPayloadRequest
            {
                // This is a customized payload, not an actual transaction
                Txjson = new
                {
                    TransactionType = "SignIn",
                    SignInFor = "XRPAtom Wallet Linking"
                },
                Options = new XamanPayloadRequest.RequestOptions
                {
                    ReturnUrl = true,
                    Submit = false, // Don't submit to the ledger
                    Expire = true,
                    ExpireSeconds = 300 // 5 minute expiration
                }
            };

            // Create the sign request
            var response = await CreateSignRequest(xamanPayloadRequest);

            // If callback URL is provided, subscribe to the payload for updates
            if (!string.IsNullOrEmpty(callbackUrl))
            {
                await SubscribeToPayload(response.Uuid, callbackUrl);
            }

            // Return a simplified sign request object
            return new XamanSignRequest
            {
                PayloadId = response.Uuid,
                QrCodeUrl = response.Refs.QrPng,
                WebsocketUrl = response.Refs.WebsocketStatus,
                PushNotificationSent = response.Pushed
            };
        }

        /// <summary>
        /// Prepares a transaction for signing via Xaman
        /// </summary>
        public async Task<XamanSignRequest> PrepareTransaction(TransactionPrepareRequest request, IXRPLedgerService xrplService, string optionalMemo = null)
        {
            try
            {
                // First, we need to get the account info and sequence
                var accountInfo = await xrplService.GetAccountInfo(request.SourceAddress);
                var accountInfoDoc = JsonDocument.Parse(accountInfo).RootElement;
                
                if (!accountInfoDoc.TryGetProperty("result", out var accountResult) || 
                    accountResult.TryGetProperty("error", out _))
                {
                    throw new Exception($"Failed to get account info: {accountInfo}");
                }
                
                var accountData = accountResult.GetProperty("account_data");
                var sequence = accountData.GetProperty("Sequence").GetUInt32();

                // Get current ledger for LastLedgerSequence
                var ledgerResponse = await xrplService.GetLedgerCurrent();
                var ledgerDoc = JsonDocument.Parse(ledgerResponse).RootElement;
                
                if (!ledgerDoc.TryGetProperty("result", out var ledgerResult) || 
                    ledgerResult.TryGetProperty("error", out _))
                {
                    throw new Exception($"Failed to get ledger info: {ledgerResponse}");
                }
                
                var currentLedger = ledgerResult.GetProperty("ledger_current_index").GetUInt32();
                var lastLedgerSequence = currentLedger + 4; // Give 4 ledgers to process

                // Build the transaction JSON based on type
                object txJson;
                
                switch (request.TransactionType.ToLower())
                {
                    case "payment":
                        txJson = BuildPaymentTransaction(request, sequence, lastLedgerSequence, optionalMemo);
                        break;
                    case "trustset":
                        txJson = BuildTrustSetTransaction(request, sequence, lastLedgerSequence);
                        break;
                    case "offercreate":
                        txJson = BuildOfferCreateTransaction(request, sequence, lastLedgerSequence);
                        break;
                    default:
                        throw new NotSupportedException($"Transaction type {request.TransactionType} not supported");
                }
                
                // Create Xaman payload request
                var xamanPayloadRequest = new XamanPayloadRequest
                {
                    Txjson = txJson,
                    Options = new XamanPayloadRequest.RequestOptions
                    {
                        ReturnUrl = true,
                        Submit = true,
                        Expire = true,
                        ExpireSeconds = 300 // 5 minutes
                    }
                };
                
                // Create the sign request in Xaman
                var response = await CreateSignRequest(xamanPayloadRequest);
                
                // Return a simplified sign request object
                return new XamanSignRequest
                {
                    PayloadId = response.Uuid,
                    QrCodeUrl = response.Refs.QrPng,
                    WebsocketUrl = response.Refs.WebsocketStatus,
                    PushNotificationSent = response.Pushed
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error preparing transaction for Xaman signing");
                throw;
            }
        }

        #region Transaction Building Helpers

        private object BuildPaymentTransaction(TransactionPrepareRequest request, uint sequence, uint lastLedgerSequence, string memo = null)
        {
            // Check if this is an XRP payment or an issued currency payment
            object payment;
            
            if (request.Currency == "XRP")
            {
                // XRP payment
                payment = new
                {
                    TransactionType = "Payment",
                    Account = request.SourceAddress,
                    Destination = request.DestinationAddress,
                    Amount = Convert.ToString(Convert.ToUInt64(request.Amount * 1000000)), // Convert to drops (1 XRP = 1,000,000 drops)
                    Sequence = sequence,
                    Fee = "12", // Standard fee in drops
                    LastLedgerSequence = lastLedgerSequence,
                    Flags = request.Flags ?? 0
                };
            }
            else
            {
                // Issued currency payment
                payment = new
                {
                    TransactionType = "Payment",
                    Account = request.SourceAddress,
                    Destination = request.DestinationAddress,
                    Amount = new
                    {
                        currency = request.Currency,
                        issuer = request.Issuer,
                        value = request.Amount.ToString()
                    },
                    Sequence = sequence,
                    Fee = "12",
                    LastLedgerSequence = lastLedgerSequence,
                    Flags = request.Flags ?? 0
                };
            }
            
            // If memo is provided, add it to the transaction
            if (!string.IsNullOrEmpty(memo))
            {
                // Convert memo to hex
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(memo);
                string hexMemo = BitConverter.ToString(bytes).Replace("-", "").ToLower();
                
                // Add the memo to the payment
                var paymentWithMemo = new Dictionary<string, object>(
                    JsonSerializer.Deserialize<Dictionary<string, object>>(
                        JsonSerializer.Serialize(payment)
                    )
                );
                
                paymentWithMemo["Memos"] = new[]
                {
                    new
                    {
                        Memo = new
                        {
                            MemoData = hexMemo
                        }
                    }
                };
                
                return paymentWithMemo;
            }
            
            return payment;
        }

        private object BuildTrustSetTransaction(TransactionPrepareRequest request, uint sequence, uint lastLedgerSequence)
        {
            return new
            {
                TransactionType = "TrustSet",
                Account = request.SourceAddress,
                LimitAmount = new
                {
                    currency = request.Currency,
                    issuer = request.Issuer,
                    value = request.Amount.ToString()
                },
                Sequence = sequence,
                Fee = "12",
                LastLedgerSequence = lastLedgerSequence,
                Flags = request.Flags ?? 0
            };
        }

        private object BuildOfferCreateTransaction(TransactionPrepareRequest request, uint sequence, uint lastLedgerSequence)
        {
            // OfferCreate for buying or selling XRP or issued currencies
            object takerGets;
            object takerPays;

            if (request.GetsCurrency == "XRP")
            {
                takerGets = Convert.ToString(Convert.ToUInt64(request.GetsAmount * 1000000));
            }
            else
            {
                takerGets = new
                {
                    currency = request.GetsCurrency,
                    issuer = request.GetsIssuer,
                    value = request.GetsAmount.ToString()
                };
            }

            if (request.PaysCurrency == "XRP")
            {
                takerPays = Convert.ToString(Convert.ToUInt64(request.PaysAmount * 1000000));
            }
            else
            {
                takerPays = new
                {
                    currency = request.PaysCurrency,
                    issuer = request.PaysIssuer,
                    value = request.PaysAmount.ToString()
                };
            }

            return new
            {
                TransactionType = "OfferCreate",
                Account = request.SourceAddress,
                TakerGets = takerGets,
                TakerPays = takerPays,
                Sequence = sequence,
                Fee = "12",
                LastLedgerSequence = lastLedgerSequence,
                Flags = request.Flags ?? 0
            };
        }

        #endregion
    }
    
    

    /// <summary>
    /// Simplified response for Xaman sign requests
    /// </summary>
    public class XamanSignRequest
    {
        public string PayloadId { get; set; }
        public string QrCodeUrl { get; set; }
        public string DeepLinkUrl { get; set; }
        public string WebsocketUrl { get; set; }
        public bool PushNotificationSent { get; set; }
    }
}