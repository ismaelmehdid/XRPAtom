namespace XRPAtom.Core.DTOs;

public class EscrowCreationResultDto
{
    public bool Success { get; set; }
    public string EscrowId { get; set; }
    public decimal Amount { get; set; }
    public string XummPayloadId { get; set; }
    public string QrCodeUrl { get; set; }
    public string DeepLink { get; set; }
    public string Condition { get; set; }
    public string Fulfillment { get; set; }
}