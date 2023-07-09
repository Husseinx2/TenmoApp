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
        
        [HttpGet("account/{accountId}")]
        public ActionResult<IList<Transfer>> GetTransfersByAccountID(int accountId)
        {
            return Ok(transferDao.GetTransfersByAccountID(accountId));
        }

        [HttpGet("{transferId}")] 
        public ActionResult<Transfer> GetTransferByTransferId(int transferId)
        {
            Transfer transfer = transferDao.GetTransferByTransferId(transferId);
            if (transfer == null)
            {
                return StatusCode(404);
            }
            return Ok(transfer);
        }
        [HttpPost("send")]
        public ActionResult<Transfer> Send(Transfer transfer)
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

        [HttpPost("request")]
        public ActionResult<Transfer> TransferRequest(Transfer transfer)
        {
            if (transfer != null && (transfer.AccountFrom != transfer.AccountTo))
            {
                return Ok(transferDao.CreateTransfer(transfer));
            }

            return StatusCode(400);
        }

        [HttpPut("request")]
        public ActionResult<Transfer> UpdateRequest(Transfer transfer)
        {
            decimal accountBalance = accountDao.GetBalanceByAccountID(transfer.AccountFrom);

            if (transfer != null && (transfer.AccountFrom != transfer.AccountTo) && (transfer.Amount > 0)
                && (accountBalance > transfer.Amount) && (transfer.TransferStatusId == 2))
            {
                accountDao.IncrementBalance(transfer.AccountFrom, -transfer.Amount);
                accountDao.IncrementBalance(transfer.AccountTo, transfer.Amount);
                return Ok(transferDao.UpdateTransfer(transfer));
            }
           else if (transfer.TransferStatusId == 3)
            {
                return Ok(transferDao.UpdateTransfer(transfer));
            }

            return StatusCode(404);         
        } 

        [HttpGet("transfertype/{transferTypeId}")]
         public ActionResult<TransferType> GetTransferType(int transferTypeId)
        {
            return Ok(transferDao.GetTransferType(transferTypeId).Desc);
        }
        [HttpGet("transferstatus/{transferStatusId}")]
        public ActionResult<TransferStatus> GetTransferStatus(int transferStatusId)
        {
            return Ok(transferDao.GetTransferStatus(transferStatusId).Desc);
        }
    }
}
