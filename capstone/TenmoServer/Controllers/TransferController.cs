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
        private ITransferDao transferDao;
        private readonly IUserDao userDao;
        public TransferController(ITransferDao transferDao, IUserDao userDao)
        {
            this.transferDao = transferDao;
            this.userDao = userDao;
        }

        [HttpGet()]
        public ActionResult<IList<User>> ListUsers()
        {
            return Ok(userDao.GetUsers());
        }


    }
}
