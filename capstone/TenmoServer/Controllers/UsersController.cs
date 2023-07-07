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
    public class UsersController:ControllerBase
    {
        private readonly IUserDao userDao;
        public UsersController(IUserDao userDao)
        {       
            this.userDao = userDao;
        }
 
        [HttpGet()]
        public ActionResult<IList<User>> ListUsers()
        {
            return Ok(userDao.GetUsers());
        }
        [HttpGet("username/{accountId}")]
        public ActionResult<User> GetUserNameByAccountId(int accountId)
        {
            string name = userDao.GetUserNameByAccountId(accountId);
            if (name == "")
            {
                return StatusCode(404);
            }
            return Ok(name);
        }
    }
}