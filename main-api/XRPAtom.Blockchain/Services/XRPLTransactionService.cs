using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using XRPAtom.Blockchain.Interfaces;
using XRPAtom.Blockchain.Models;

namespace XRPAtom.Blockchain.Services
{
    public class XRPLTransactionService : IXRPLTransactionService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<XRPLTransactionService> _logger;
        private readonly string _xrplServer;

        public XRPLTransactionService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<XRPLTransactionService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _xrplServer = configuration["XRPLedger:ServerUrl"] ?? "https://s.altnet.rippletest.net:51234/";
        }

        public async Task<TransactionPrepareResponse> PrepareTransaction(TransactionPrepareRequest request)
        {
            try
            {
                // First, get account info to obtain the sequence number
                var accountInfoRequest = new
                {
                    method = "account_info",
                    @params = new[]
                    {
                        new
                        {
                            account = request.SourceAddress,
                            strict = true,
                            ledger_index = "current"
                        }
                    }
                };

                var accountInfoResponse = await SendJsonRpcRequest(accountInfoRequest);
                var accountInfoResult = JsonDocument.Parse(accountInfoResponse).RootElement;
                
                if (!accountInfoResult.TryGetProperty("result", out var result) || 
                    result.TryGetProperty("error", out _))
                {
                    throw new Exception($"Failed to get account info: {accountInfoResponse}");
                }

                var accountData = result.GetProperty("account_data");
                var sequence = accountData.GetProperty("Sequence").GetUInt32();

                // Get current ledger info to calculate LastLedgerSequence
                var ledgerRequest = new
                {
                    method = "ledger_current",
                    @params = new object[] { }
                };

                var ledgerResponse = await SendJsonRpcRequest(ledgerRequest);
                var ledgerResult = JsonDocument.Parse(ledgerResponse).RootElement;
                
                if (!ledgerResult.TryGetProperty("result", out var ledgerResultObj))
                {
                    throw new Exception($"Failed to get ledger info: {ledgerResponse}");
                }

                var currentLedger = ledgerResultObj.GetProperty("ledger_current_index").GetUInt32();
                var lastLedgerSequence = currentLedger + 4; // Give 4 ledgers to process the tx

                // Build the transaction object based on the request type
                object transaction;
                switch (request.TransactionType.ToLower())
                {
                    case "payment":
                        transaction = BuildPaymentTransaction(request, sequence, lastLedgerSequence);
                        break;
                    case "trustset":
                        transaction = BuildTrustSetTransaction(request, sequence, lastLedgerSequence);
                        break;
                    case "offercreate":
                        transaction = BuildOfferCreateTransaction(request, sequence, lastLedgerSequence);
                        break;
                    default:
                        throw new NotImplementedException($"Transaction type {request.TransactionType} not implemented");
                }

                // Prepare the transaction using XRPL's APIs
                var txJsonStr = JsonSerializer.Serialize(transaction);
                _logger.LogInformation("Prepared transaction: {TxJson}", txJsonStr);

                return new TransactionPrepareResponse
                {
                    Success = true,
                    PreparedTransaction = txJsonStr,
                    TransactionType = request.TransactionType,
                    SourceAddress = request.SourceAddress,
                    Sequence = sequence,
                    LastLedgerSequence = lastLedgerSequence
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error preparing transaction of type {Type}", request.TransactionType);
                return new TransactionPrepareResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<TransactionSubmitResponse> SubmitSignedTransaction(string signedTransaction)
        {
            try
            {
                var submitRequest = new
                {
                    method = "submit",
                    @params = new[]
                    {
                        new
                        {
                            tx_blob = signedTransaction,
                            fail_hard = false
                        }
                    }
                };

                var submitResponse = await SendJsonRpcRequest(submitRequest);
                var submitResult = JsonDocument.Parse(submitResponse).RootElement;

                if (!submitResult.TryGetProperty("result", out var result))
                {
                    throw new Exception($"Failed to submit transaction: {submitResponse}");
                }

                var engineResult = result.GetProperty("engine_result").GetString();
                var engineResultCode = result.GetProperty("engine_result_code").GetInt32();
                var engineResultMessage = result.GetProperty("engine_result_message").GetString();
                var txHash = result.GetProperty("tx_json").GetProperty("hash").GetString();

                var success = engineResult == "tesSUCCESS" || engineResult.StartsWith("tes");

                return new TransactionSubmitResponse
                {
                    Success = success,
                    TransactionHash = txHash,
                    EngineResult = engineResult,
                    EngineResultCode = engineResultCode,
                    EngineResultMessage = engineResultMessage,
                    RawResponse = submitResponse
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting signed transaction");
                return new TransactionSubmitResponse
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<TransactionStatusResponse> CheckTransactionStatus(string transactionHash)
        {
            try
            {
                var txRequest = new
                {
                    method = "tx",
                    @params = new[]
                    {
                        new
                        {
                            transaction = transactionHash,
                            binary = false
                        }
                    }
                };

                var txResponse = await SendJsonRpcRequest(txRequest);
                var txResult = JsonDocument.Parse(txResponse).RootElement;

                if (!txResult.TryGetProperty("result", out var result))
                {
                    throw new Exception($"Failed to get transaction info: {txResponse}");
                }

                if (result.TryGetProperty("error", out var error))
                {
                    var errorMessage = error.GetString();
                    // If "txnNotFound", transaction is still pending
                    if (errorMessage == "txnNotFound")
                    {
                        return new TransactionStatusResponse
                        {
                            Success = true,
                            Status = "pending",
                            Message = "Transaction not found in ledger yet - still pending"
                        };
                    }
                    else
                    {
                        return new TransactionStatusResponse
                        {
                            Success = false,
                            Status = "error",
                            Message = errorMessage
                        };
                    }
                }

                // If we found it, check its status
                var validated = result.GetProperty("validated").GetBoolean();
                var meta = result.GetProperty("meta");
                var transactionResult = meta.GetProperty("TransactionResult").GetString();

                if (validated && transactionResult == "tesSUCCESS")
                {
                    return new TransactionStatusResponse
                    {
                        Success = true,
                        Status = "confirmed",
                        Message = "Transaction confirmed and successful",
                        LedgerIndex = result.GetProperty("ledger_index").GetUInt32(),
                        Date = UnixTimeToDateTime(result.GetProperty("date").GetUInt32())
                    };
                }
                else if (validated)
                {
                    return new TransactionStatusResponse
                    {
                        Success = false,
                        Status = "failed",
                        Message = $"Transaction was included in ledger but failed with result: {transactionResult}"
                    };
                }
                else
                {
                    return new TransactionStatusResponse
                    {
                        Success = true,
                        Status = "pending",
                        Message = "Transaction found but not yet validated"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking transaction status for hash {Hash}", transactionHash);
                return new TransactionStatusResponse
                {
                    Success = false,
                    Status = "error",
                    Message = ex.Message
                };
            }
        }

        #region Helper Methods

        private object BuildPaymentTransaction(TransactionPrepareRequest request, uint sequence, uint lastLedgerSequence)
        {
            // Check if this is an XRP payment or an issued currency payment
            if (request.Currency == "XRP")
            {
                // XRP payment
                return new
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
                return new
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
            // This implementation assumes request contains proper TakerGets and TakerPays objects
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

        private async Task<string> SendJsonRpcRequest(object request)
        {
            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(_xrplServer, content);
            var responseBody = await response.Content.ReadAsStringAsync();
            
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"XRPL API error: {response.StatusCode}, {responseBody}");
            }
            
            return responseBody;
        }

        private DateTime UnixTimeToDateTime(uint unixTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddSeconds(unixTime);
        }

        #endregion
    }
}