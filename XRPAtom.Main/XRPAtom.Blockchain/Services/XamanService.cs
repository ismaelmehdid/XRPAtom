using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using XRPAtom.Blockchain.Interfaces;
using XRPAtom.Blockchain.Models;

namespace XRPAtom.Blockchain.Services
{
    public class XamanService : IXamanService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<XamanService> _logger;
        private readonly string _apiKey;
        private readonly string _apiSecret;

        public XamanService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<XamanService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _apiKey = configuration["Xaman:ApiKey"];
            _apiSecret = configuration["Xaman:ApiSecret"];
            
            // Configure HttpClient for Xaman API
            _httpClient.DefaultRequestHeaders.Add("X-API-Key", _apiKey);
            _httpClient.DefaultRequestHeaders.Add("X-API-Secret", _apiSecret);
        }

        public async Task<XamanPayloadResponse> CreateSignRequest(XamanPayloadRequest request)
        {
            try
            {
                var content = new StringContent(
                    JsonSerializer.Serialize(request),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync("platform/payload", content);
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

        public async Task<XamanPayloadStatus> CheckPayloadStatus(string payloadId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"platform/payload/{payloadId}");
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

                var response = await _httpClient.PostAsync($"platform/payload/{payloadId}/subscribe", content);
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

        public async Task<string> GetUserToken(string userAddress)
        {
            try
            {
                var response = await _httpClient.GetAsync($"platform/xapp/user/{userAddress}");
                
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
    }
}