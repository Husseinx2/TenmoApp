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

        [HttpGet("{userId}/balance")]
        public ActionResult<decimal> GetBalance(int userId)
        {
            return Ok(accountDao.GetBalanceByUserID(userId));
        }

        [HttpGet("{userId}")]
        public ActionResult<decimal> GetAccountId(int userId)
        {
            return Ok(accountDao.GetAccountId(userId));
        }
    }
}
