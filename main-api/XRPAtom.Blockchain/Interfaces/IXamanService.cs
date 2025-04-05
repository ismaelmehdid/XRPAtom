using XRPAtom.Blockchain.Models;
using XRPAtom.Blockchain.Services;

namespace XRPAtom.Blockchain.Interfaces
{
    /// <summary>
    /// Interface for services that interact with the Xaman (XUMM) API
    /// </summary>
    public interface IXamanService
    {
        /// <summary>
        /// Creates a sign request for the Xaman (XUMM) app
        /// </summary>
        /// <param name="request">The payload request details</param>
        /// <returns>The payload response with QR and deep link information</returns>
        Task<XamanPayloadResponse> CreateSignRequest(XamanPayloadRequest request);
        
        /// <summary>
        /// Checks the status of a payload sign request
        /// </summary>
        /// <param name="payloadId">The payload UUID to check</param>
        /// <returns>The current payload status</returns>
        Task<XamanPayloadStatus> CheckPayloadStatus(string payloadId);
        
        /// <summary>
        /// Subscribes to a payload for real-time updates
        /// </summary>
        /// <param name="payloadId">The payload UUID to subscribe to</param>
        /// <param name="callbackUrl">Optional callback URL for updates</param>
        /// <returns>Success status</returns>
        Task<bool> SubscribeToPayload(string payloadId, string callbackUrl = null);
        
        /// <summary>
        /// Retrieves a user token for push notifications
        /// </summary>
        /// <param name="userAddress">The user's XRPL address</param>
        /// <returns>The user token if available</returns>
        Task<string> GetUserToken(string userAddress);
    }
}