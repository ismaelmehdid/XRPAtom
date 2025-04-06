namespace XRPAtom.Blockchain.Interfaces;

public interface IBlockchainEventListener
{
    Task<bool> ProcessEscrowTransaction(string payloadId, string transactionHash, uint offerSequence);
    Task<bool> MonitorLedgerForEscrowEvents();
    Task<bool> NotifyEventCompletion(string eventId, string transactionHash);
}