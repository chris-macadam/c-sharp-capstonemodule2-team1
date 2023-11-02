using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Transactions;
using TenmoServer.DAO;
using TenmoServer.Exceptions;
using TenmoServer.Models;

namespace TenmoServer.Controllers
{
    [Route("transfer")]
    [ApiController]
    public class TransferController : ControllerBase
    {
        private ITransferDao transferDao;
        private IUserDao userDao;

        public TransferController (ITransferDao transferDao)
        {
            this.transferDao = transferDao;
        }

        [HttpPost("{userId}")]
        public ActionResult<Transfer> CreateTransfer(Transfer transfer, int userId)
        {
            Transfer createdTransfer = transferDao.CreateTransfer(transfer, userId);
            return Created($"/reservations/{createdTransfer.TransferId}", createdTransfer);
        }

        [HttpGet("all/{userId}")]
        public ActionResult<List<Transfer>> GetTransfers(int userId) 
        {
            List<Transfer> transfers = transferDao.GetAllTransfers(userId);
            if(transfers == null)
            {
                return NotFound();
            }

            return transfers;
        }

        [HttpGet("{transferId}")]
        public ActionResult<List<Transfer>> GetTransfersById(int transferId)
        {
            List<Transfer> transfers = transferDao.GetAllTransfers(transferId);
            if(transfers == null)
            {
                return NotFound();
                
            }

            return transfers;
        }

        [HttpGet("pending/{userId}")]
        public ActionResult<List<Transfer>> GetPendingTransfers(int userId)
        {
            List<Transfer> transfers = transferDao.GetPendingTransfers(userId);
            if (transfers == null)
            {
                return NotFound();

            }

            return transfers;
        }

        [HttpPut("{transferId}")]
        public ActionResult<Transfer> UpdateTransferStatus(int transferId, Transfer transfer)
        {
            transfer.TransferId = transferId;

            try
            {
                if(userDao.CheckUserBalance(transfer.TransactionAmount, transfer.AccountFromId))
                {


                    Transfer result = transferDao.SetTransferStatus(transferId, transfer.TransferStatus);
                    return Ok(result);
                }
                else
                {
                    return UnprocessableEntity();
                }
            }
            catch (DaoException)
            {
                return NotFound();
            }
        }
    }
}
