namespace XRPAtom.Core.DTOs;

public class EscrowFinishResultDto
{
    public bool Success { get; set; }
    public string EscrowId { get; set; }
    public string XummPayloadId { get; set; }
    public string QrCodeUrl { get; set; }
    public string DeepLink { get; set; }
}