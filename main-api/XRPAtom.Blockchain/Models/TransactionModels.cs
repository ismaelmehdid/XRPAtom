namespace XRPAtom.Blockchain.Models
{
    #region Transaction Preparation Models

    public class TransactionPrepareRequest
    {
        /// <summary>
        /// The type of transaction (Payment, TrustSet, OfferCreate, etc.)
        /// </summary>
        public string TransactionType { get; set; }
        
        /// <summary>
        /// The source address that will sign the transaction
        /// </summary>
        public string SourceAddress { get; set; }
        
        /// <summary>
        /// The destination address for payments
        /// </summary>
        public string DestinationAddress { get; set; }
        
        /// <summary>
        /// The amount to transfer or the limit amount for trust lines
        /// </summary>
        public decimal Amount { get; set; }
        
        /// <summary>
        /// The currency code (defaults to "XRP")
        /// </summary>
        public string Currency { get; set; } = "XRP";
        
        /// <summary>
        /// The issuer address for non-XRP currencies
        /// </summary>
        public string Issuer { get; set; }
        
        /// <summary>
        /// Transaction flags
        /// </summary>
        public uint? Flags { get; set; }
        
        // For OfferCreate transactions
        
        /// <summary>
        /// The currency that the offer creator wants to get
        /// </summary>
        public string GetsCurrency { get; set; }
        
        /// <summary>
        /// The issuer of the currency that the offer creator wants to get
        /// </summary>
        public string? GetsIssuer { get; set; }
        
        /// <summary>
        /// The amount that the offer creator wants to get
        /// </summary>
        public decimal GetsAmount { get; set; }
        
        /// <summary>
        /// The currency that the offer creator pays
        /// </summary>
        public string PaysCurrency { get; set; }
        
        /// <summary>
        /// The issuer of the currency that the offer creator pays
        /// </summary>
        public string? PaysIssuer { get; set; }
        
        /// <summary>
        /// The amount that the offer creator pays
        /// </summary>
        public decimal PaysAmount { get; set; }
    }

    public class TransactionPrepareResponse
    {
        /// <summary>
        /// Whether the preparation was successful
        /// </summary>
        public bool Success { get; set; }
        
        /// <summary>
        /// Error message if the preparation failed
        /// </summary>
        public string ErrorMessage { get; set; }
        
        /// <summary>
        /// The prepared transaction in JSON format
        /// </summary>
        public string PreparedTransaction { get; set; }
        
        /// <summary>
        /// The transaction type
        /// </summary>
        public string TransactionType { get; set; }
        
        /// <summary>
        /// The source address
        /// </summary>
        public string SourceAddress { get; set; }
        
        /// <summary>
        /// The sequence number for the transaction
        /// </summary>
        public uint Sequence { get; set; }
        
        /// <summary>
        /// The last ledger sequence after which the transaction is expired
        /// </summary>
        public uint LastLedgerSequence { get; set; }
    }

    #endregion

    #region Transaction Submission Models

    public class TransactionSubmitResponse
    {
        /// <summary>
        /// Whether the submission was successful
        /// </summary>
        public bool Success { get; set; }
        
        /// <summary>
        /// The transaction hash
        /// </summary>
        public string TransactionHash { get; set; }
        
        /// <summary>
        /// The engine result code from the XRPL
        /// </summary>
        public string EngineResult { get; set; }
        
        /// <summary>
        /// The numeric result code
        /// </summary>
        public int EngineResultCode { get; set; }
        
        /// <summary>
        /// A human-readable explanation of the result
        /// </summary>
        public string EngineResultMessage { get; set; }
        
        /// <summary>
        /// Error message if the submission failed
        /// </summary>
        public string ErrorMessage { get; set; }
        
        /// <summary>
        /// The raw response from the XRPL
        /// </summary>
        public string RawResponse { get; set; }
    }

    public class TransactionStatusResponse
    {
        /// <summary>
        /// Whether the status check was successful
        /// </summary>
        public bool Success { get; set; }
        
        /// <summary>
        /// The current status of the transaction (pending, confirmed, failed, error)
        /// </summary>
        public string Status { get; set; }
        
        /// <summary>
        /// A message describing the current status
        /// </summary>
        public string Message { get; set; }
        
        /// <summary>
        /// The ledger index in which the transaction was included (if confirmed)
        /// </summary>
        public uint? LedgerIndex { get; set; }
        
        /// <summary>
        /// The timestamp of the transaction (if confirmed)
        /// </summary>
        public DateTime? Date { get; set; }
    }

    #endregion
}