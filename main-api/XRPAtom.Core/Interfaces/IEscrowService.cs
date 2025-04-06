using XRPAtom.Core.DTOs;

namespace XRPAtom.Core.Interfaces
{
    public interface IEscrowService
    {
        Task<EscrowCreationResultDto> CreateMainEventEscrow(
            string eventId, 
            string sourceAddress, 
            decimal totalAmount,
            DateTime releaseDate);
            
        Task<EscrowCreationResultDto> CreateParticipantEscrow(
            string eventId,
            string participantId,
            string sourceAddress,
            string destinationAddress,
            decimal amount,
            DateTime releaseDate);
            
        Task<EscrowFinishResultDto> FinishEscrow(string escrowId, string signerAddress);
        
        Task<EscrowCancelResultDto> CancelEscrow(string escrowId, string signerAddress);
        
        Task<bool> UpdateEscrowFromTransaction(string escrowId, string txHash);
    }
}