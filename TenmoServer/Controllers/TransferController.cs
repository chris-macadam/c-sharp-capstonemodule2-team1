using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Transactions;
using TenmoServer.DAO;
using TenmoServer.Exceptions;
using TenmoServer.Models;

namespace TenmoServer.Controllers
{
    [Route("transfer")]
    [ApiController, Authorize]
    public class TransferController : ControllerBase
    {
        private ITransferDao transferDao;
        private IUserDao userDao;

        public TransferController (ITransferDao transferDao, IUserDao userDao)
        {
            this.transferDao = transferDao;
            this.userDao = userDao;
        }

        [HttpPost("{userId}")]
        public ActionResult<Transfer> CreateTransfer(Transfer transfer, int userId)
        {
            Transfer createdTransfer = transferDao.CreateTransfer(transfer, userId);
            if (createdTransfer.TransferStatus == 2)
            {
                transferDao.TransferFunds(createdTransfer);
            }
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
        public ActionResult<Transfer> GetTransferById(int transferId)
        {
            Transfer transfer = transferDao.GetTransferById(transferId);
            if(transfer == null)
            {
                return NotFound();
                
            }

            return transfer;
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
                bool HasSufficientBalance = userDao.CheckUserBalance(transfer.TransactionAmount, transfer.AccountFromId);
                if (HasSufficientBalance)
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
