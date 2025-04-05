using System.Text.Json.Serialization;

namespace XRPAtom.Blockchain.Models
{
    public class XamanPayloadRequest
    {
        /// <summary>
        /// The transaction to be signed in JSON format
        /// </summary>
        [JsonPropertyName("txjson")]
        public object Txjson { get; set; }
        
        /// <summary>
        /// Optional user token for push notifications
        /// </summary>
        [JsonPropertyName("user_token")]
        public UserTokenOptions UserToken { get; set; }
        
        /// <summary>
        /// Options for the payload
        /// </summary>
        [JsonPropertyName("options")]
        public RequestOptions Options { get; set; } = new RequestOptions();

        public class UserTokenOptions
        {
            /// <summary>
            /// The user token for push notifications
            /// </summary>
            [JsonPropertyName("token")]
            public string Token { get; set; }
        }

        public class RequestOptions
        {
            /// <summary>
            /// Whether to include a return URL
            /// </summary>
            [JsonPropertyName("return_url")]
            public bool ReturnUrl { get; set; } = true;
            
            /// <summary>
            /// Whether Xaman should submit the signed transaction
            /// </summary>
            [JsonPropertyName("submit")]
            public bool Submit { get; set; } = true;
            
            /// <summary>
            /// Whether the payload should expire
            /// </summary>
            [JsonPropertyName("expire")]
            public bool Expire { get; set; } = true;
            
            /// <summary>
            /// The number of seconds until the payload expires
            /// </summary>
            [JsonPropertyName("expire_seconds")]
            public int? ExpireSeconds { get; set; } = 300; // 5 minutes by default
        }
    }

    public class XamanPayloadResponse
    {
        /// <summary>
        /// The unique identifier for the payload
        /// </summary>
        [JsonPropertyName("uuid")]
        public string Uuid { get; set; }
        
        /// <summary>
        /// The next URL to redirect to
        /// </summary>
        [JsonPropertyName("next")]
        public string Next { get; set; }
        
        /// <summary>
        /// References to QR codes and WebSocket
        /// </summary>
        [JsonPropertyName("refs")]
        public QrData Refs { get; set; }
        
        /// <summary>
        /// Whether the payload was pushed to the user's device
        /// </summary>
        [JsonPropertyName("pushed")]
        public bool Pushed { get; set; }

        public class QrData
        {
            /// <summary>
            /// The URL for the QR code PNG
            /// </summary>
            [JsonPropertyName("qr_png")]
            public string QrPng { get; set; }
            
            /// <summary>
            /// The URL to open the Xaman app
            /// </summary>
            [JsonPropertyName("qr_url")]
            public string QrUrl { get; set; }
            
            /// <summary>
            /// The WebSocket URL for status updates
            /// </summary>
            [JsonPropertyName("websocket_status")]
            public string WebsocketStatus { get; set; }
        }
    }

    public class XamanPayloadStatus
    {
        [JsonPropertyName("uuid")]
        public string Uuid { get; set; }
    
        [JsonPropertyName("response")]
        public ResponseData Response { get; set; }
    
        [JsonPropertyName("meta")]
        public MetaData Meta { get; set; }
    
        [JsonPropertyName("expired")]
        public bool Expired { get; set; }
    
        [JsonPropertyName("resolved")]
        public bool Resolved { get; set; }
    
        [JsonPropertyName("signed")]
        public bool Signed { get; set; }
    
        [JsonPropertyName("transaction")]
        public object Transaction { get; set; }

        public class ResponseData
        {
            [JsonPropertyName("signed")]
            public bool Signed { get; set; }

            [JsonPropertyName("txid")]
            public string Txid { get; set; }

            [JsonPropertyName("hex")]
            public string Hex { get; set; }
        }

        public class MetaData  // Separate class without inheritance
        {
            [JsonPropertyName("device")]
            public string Device { get; set; }

            [JsonPropertyName("app")]
            public string App { get; set; }
        }
    }
}