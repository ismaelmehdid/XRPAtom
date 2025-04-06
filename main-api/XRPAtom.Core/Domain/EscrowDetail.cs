namespace XRPAtom.Core.Domain;

public class EscrowDetail
{
    public string Id { get; set; }
    public string EventId { get; set; }
    public string ParticipantId { get; set; }
    public string EscrowType { get; set; } // "MainEvent" or "Participant"
        
    public string SourceAddress { get; set; }
    public string DestinationAddress { get; set; }
    public decimal Amount { get; set; }
        
    public string Condition { get; set; }
    public string Fulfillment { get; set; }
    public uint FinishAfter { get; set; }
        
    public string XummPayloadId { get; set; }
    public string FinishPayloadId { get; set; }
    public string CancelPayloadId { get; set; }
        
    public string TransactionHash { get; set; }
    public string OfferSequence { get; set; }
        
    public string Status { get; set; } // "Pending", "Active", "FinishPending", "CancelPending", "Finished", "Cancelled", "Failed"
        
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
        
    // Navigation properties
    public virtual CurtailmentEvent Event { get; set; }
}