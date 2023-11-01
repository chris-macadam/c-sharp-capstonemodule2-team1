using System.Transactions;

namespace TenmoServer.DAO
{
    public interface ITransferDao
    {
        Transaction CreateTransfer();
        Transaction GetAllTransfers();
        Transaction GetTransferById();
        // Consider below if necessary
        //Transaction RequestTransfer();
        Transaction GetPendingTransfers();
        Transaction SetTransferStatus();
    }
}
