namespace XRPAtom.Blockchain.Interfaces;

public interface IBlockchainVerificationService
{
    Task<bool> VerifyEventOnBlockchain(string eventId, string blockchainReference, string verificationProof);
    Task<string> CreateEventVerification(string eventId, string verificationProof, decimal totalEnergySaved);
    Task<bool> RecordEventParticipation(string eventId, string userId, string walletAddress);
    Task<string> GenerateVerificationProof(string eventId, string userId, decimal energySaved);
}