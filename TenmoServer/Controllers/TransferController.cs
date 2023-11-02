using Microsoft.AspNetCore.Mvc;
using TenmoServer.DAO;
using TenmoServer.Models;

namespace TenmoServer.Controllers
{
    [Route("transfer")]
    [ApiController]
    public class TransferController : ControllerBase
    {
        private ITransferDao transferDao;

        public TransferController (ITransferDao transferDao)
        {
            this.transferDao = transferDao;
        }
    }
}
