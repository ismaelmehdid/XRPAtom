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
        /// The next URL information
        /// </summary>
        [JsonPropertyName("next")]
        public NextUrlData Next { get; set; }
    
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

        public class NextUrlData
        {
            /// <summary>
            /// The URL to always use for redirection
            /// </summary>
            [JsonPropertyName("always")]
            public string Always { get; set; }
        }

        public class QrData
        {
            /// <summary>
            /// The URL for the QR code PNG
            /// </summary>
            [JsonPropertyName("qr_png")]
            public string QrPng { get; set; }
        
            /// <summary>
            /// The URL for the QR matrix data
            /// </summary>
            [JsonPropertyName("qr_matrix")]
            public string QrMatrix { get; set; }
        
            /// <summary>
            /// QR URI quality options
            /// </summary>
            [JsonPropertyName("qr_uri_quality_opts")]
            public string[] QrUriQualityOpts { get; set; }
        
            /// <summary>
            /// The WebSocket URL for status updates
            /// </summary>
            [JsonPropertyName("websocket_status")]
            public string WebsocketStatus { get; set; }
        }
    }

    public class XamanPayloadStatus
    {
        [JsonPropertyName("meta")]
        public MetaData Meta { get; set; }

        [JsonPropertyName("application")]
        public ApplicationData Application { get; set; }

        [JsonPropertyName("payload")]
        public PayloadData Payload { get; set; }
        
        [JsonPropertyName("response")]
        public ResponseData Response { get; set; }

        [JsonPropertyName("custom_meta")]
        public CustomMetaData CustomMeta { get; set; }

        // Helper properties to make code using this class easier to maintain
        [JsonIgnore]
        public bool Expired => Meta?.Expired ?? false;
        
        [JsonIgnore]
        public bool Resolved => Meta?.Resolved ?? false;
        
        [JsonIgnore]
        public bool Signed => Meta?.Signed ?? false;
        
        [JsonIgnore]
        public string Uuid => Meta?.Uuid;

        public class MetaData
        {
            [JsonPropertyName("exists")]
            public bool Exists { get; set; }

            [JsonPropertyName("uuid")]
            public string Uuid { get; set; }

            [JsonPropertyName("multisign")]
            public bool Multisign { get; set; }

            [JsonPropertyName("submit")]
            public bool Submit { get; set; }

            [JsonPropertyName("pathfinding")]
            public bool Pathfinding { get; set; }

            [JsonPropertyName("pathfinding_fallback")]
            public bool PathfindingFallback { get; set; }

            [JsonPropertyName("force_network")]
            public string ForceNetwork { get; set; }

            [JsonPropertyName("destination")]
            public string Destination { get; set; }

            [JsonPropertyName("resolved_destination")]
            public string ResolvedDestination { get; set; }

            [JsonPropertyName("resolved")]
            public bool Resolved { get; set; }

            [JsonPropertyName("signed")]
            public bool Signed { get; set; }

            [JsonPropertyName("cancelled")]
            public bool Cancelled { get; set; }

            [JsonPropertyName("expired")]
            public bool Expired { get; set; }

            [JsonPropertyName("pushed")]
            public bool Pushed { get; set; }

            [JsonPropertyName("app_opened")]
            public bool AppOpened { get; set; }

            [JsonPropertyName("opened_by_deeplink")]
            public bool OpenedByDeeplink { get; set; }

            [JsonPropertyName("return_url_app")]
            public string ReturnUrlApp { get; set; }

            [JsonPropertyName("return_url_web")]
            public string ReturnUrlWeb { get; set; }

            [JsonPropertyName("is_xapp")]
            public bool IsXapp { get; set; }

            [JsonPropertyName("signers")]
            public object Signers { get; set; }
        }

        public class ApplicationData
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("description")]
            public string Description { get; set; }

            [JsonPropertyName("disabled")]
            public int Disabled { get; set; }

            [JsonPropertyName("uuidv4")]
            public string UuidV4 { get; set; }

            [JsonPropertyName("icon_url")]
            public string IconUrl { get; set; }

            [JsonPropertyName("issued_user_token")]
            public string IssuedUserToken { get; set; }
        }

        public class PayloadData
        {
            [JsonPropertyName("tx_type")]
            public string TxType { get; set; }

            [JsonPropertyName("tx_destination")]
            public string TxDestination { get; set; }

            [JsonPropertyName("tx_destination_tag")]
            public object TxDestinationTag { get; set; }

            [JsonPropertyName("request_json")]
            public Dictionary<string, object> RequestJson { get; set; }

            [JsonPropertyName("origintype")]
            public string OriginType { get; set; }

            [JsonPropertyName("signmethod")]
            public string SignMethod { get; set; }

            [JsonPropertyName("created_at")]
            public string CreatedAt { get; set; }

            [JsonPropertyName("expires_at")]
            public string ExpiresAt { get; set; }

            [JsonPropertyName("expires_in_seconds")]
            public int ExpiresInSeconds { get; set; }
        }

        public class ResponseData
        {
            [JsonPropertyName("hex")]
            public string Hex { get; set; }

            [JsonPropertyName("txid")]
            public string Txid { get; set; }

            [JsonPropertyName("resolved_at")]
            public string ResolvedAt { get; set; }

            [JsonPropertyName("dispatched_to")]
            public string DispatchedTo { get; set; }

            [JsonPropertyName("dispatched_nodetype")]
            public string DispatchedNodetype { get; set; }

            [JsonPropertyName("dispatched_result")]
            public string DispatchedResult { get; set; }

            [JsonPropertyName("dispatched_to_node")]
            public bool DispatchedToNode { get; set; }

            [JsonPropertyName("environment_nodeuri")]
            public string EnvironmentNodeUri { get; set; }

            [JsonPropertyName("environment_nodetype")]
            public string EnvironmentNodetype { get; set; }

            [JsonPropertyName("multisign_account")]
            public string MultisignAccount { get; set; }

            [JsonPropertyName("account")]
            public string Account { get; set; }

            [JsonPropertyName("signer")]
            public string Signer { get; set; }

            [JsonPropertyName("user")]
            public string User { get; set; }

            [JsonPropertyName("environment_networkid")]
            public int EnvironmentNetworkId { get; set; }
        }

        public class CustomMetaData
        {
            [JsonPropertyName("identifier")]
            public string Identifier { get; set; }

            [JsonPropertyName("blob")]
            public string Blob { get; set; }

            [JsonPropertyName("instruction")]
            public string Instruction { get; set; }
        }
    }
}