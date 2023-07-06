using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using TenmoServer.DAO;
using TenmoServer.Exceptions;
using TenmoServer.Models;
using TenmoServer.Security;
using Microsoft.AspNetCore.Authorization;

namespace TenmoServer.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class TransferController : ControllerBase
    {
        private readonly ITransferDao transferDao;
        private readonly IAccountDao accountDao;
 
        public TransferController(ITransferDao transferDao, IAccountDao accountDao)
        {
            this.transferDao = transferDao;
            
            this.accountDao = accountDao;

        }

        [HttpGet("{accountId}")]
        public ActionResult<IList<Transfer>> GetTransfers(int accountId)
        {
            return Ok(transferDao.GetTransfers(accountId));
        }

        [HttpPost()]
        public ActionResult<Transfer> Send (Transfer transfer)
        {    
            decimal accountBalance = accountDao.GetBalanceByAccountID(transfer.AccountFrom);

            if (transfer != null && (transfer.AccountFrom != transfer.AccountTo) && (transfer.Amount > 0)
                && (accountBalance > transfer.Amount))
            {
                accountDao.IncrementBalance(transfer.AccountFrom, -transfer.Amount);
                accountDao.IncrementBalance(transfer.AccountTo, transfer.Amount);

                return Ok(transferDao.CreateTransfer(transfer));
            }

            return StatusCode(400) ;
        }
        [HttpGet("transfertype/{transferTypeId}")]
         public ActionResult<TransferType> GetTransferType(int transferTypeId)
        {
            return Ok(transferDao.GetTransferType(transferTypeId));
        }
        [HttpGet("transferstatus/{transferStatusId}")]
        public ActionResult<TransferStatus> GetTransferStatus(int transferStatusId)
        {
            return Ok(transferDao.GetTransferStatus(transferStatusId));
        }

    }
}
