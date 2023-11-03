using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using TenmoServer.DAO;
using TenmoServer.Models;

namespace TenmoServer.Controllers
{
    [Route("users")]
    [ApiController, Authorize]

    public class UserController : ControllerBase
    {
        private IUserDao userDao;

        public UserController(IUserDao userDao)
        {
            this.userDao = userDao;
        }

        [HttpGet()]
        public ActionResult<List<User>> GetUsers()
        {
            List<User> users = userDao.GetUsers();
            if(users == null)
            {
                return NotFound();
            }
            
            return users; 
        }

        [HttpGet("{userId}")]
        public ActionResult<User> GetUserById(int userId)
        {
            User user = userDao.GetUserById(userId);
            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        [HttpGet("{userId}/account")]
        public Account GetAccountByUserId(int userId)
        {
            return userDao.GetAccountByUserId(userId);
        }
    }
}
