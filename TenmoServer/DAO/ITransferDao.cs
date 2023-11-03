using System.Collections.Generic;
using System.Transactions;
using TenmoServer.Models;

namespace TenmoServer.DAO
{
    public interface ITransferDao
    {
        Transfer CreateTransfer(Transfer transfer, int userId);
        List<Transfer> GetAllTransfers(int id);
        Transfer GetTransferById(int transferId);
        // Consider below if necessary
        //Transaction RequestTransfer();
        List<Transfer> GetPendingTransfers(int id);
        bool TransferFunds(Transfer transfer);
        Transfer SetTransferStatus(int transferId, int statusCode);
        
    }
}
