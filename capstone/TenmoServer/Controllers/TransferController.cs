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
        private readonly IUserDao userDao;
        public TransferController(ITransferDao transferDao, IUserDao userDao, IAccountDao accountDao)
        {
            this.transferDao = transferDao;
            this.userDao = userDao;
            this.accountDao = accountDao;

        }

        [HttpGet()]
        public ActionResult<IList<User>> ListUsers()
        {
            return Ok(userDao.GetUsers());
        }

        [HttpGet("{id}")]
        public ActionResult<IList<Transfer>> GetTransfers(int id)
        {
            return Ok(transferDao.GetTransfers(id));
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
        [HttpGet("{id}/transfertype")]
         public ActionResult<TransferType> GetTransferType(int id)
        {
            return Ok(transferDao.GetTransferType(id));
        }
        [HttpGet("{id}/transferstatus")]
        public ActionResult<TransferStatus> GetTransferStatus(int id)
        {
            return Ok(transferDao.GetTransferStatus(id));
        }


    }
}
