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
    public class AccountController : ControllerBase
    {
        public IAccountDao accountDao;
        public AccountController(IAccountDao accountDao)
        {
            this.accountDao = accountDao;
        }

        [HttpGet("{id}/balance")]
        public ActionResult<decimal> GetBalance(int id)
        {
            return Ok(accountDao.GetBalanceByUserID(id));
        }

        [HttpGet("{id}")]
        public ActionResult<decimal> GetAccountId(int id)
        {
            return Ok(accountDao.GetAccountId(id));
        }
    }
}
