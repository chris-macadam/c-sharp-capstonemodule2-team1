using System.Collections.Generic;
using System.Transactions;
using TenmoServer.Models;

namespace TenmoServer.DAO
{
    public interface ITransferDao
    {
        Transfer CreateTransfer(int transferType, int accountFrom, int accountTo, decimal amount);
        IList<Transfer> GetAllTransfers(int id);
        Transfer GetTransferById(int transferId);
        // Consider below if necessary
        //Transaction RequestTransfer();
        IList<Transfer> GetPendingTransfers(int id);
        Transfer SetTransferStatus(int transferId, int statusCode);
    }
}
