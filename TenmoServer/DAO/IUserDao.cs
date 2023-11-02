using System.Collections.Generic;
using TenmoServer.Models;

namespace TenmoServer.DAO
{
    public interface IUserDao
    {
        User GetUserById(int id);
        User GetUserByUsername(string username);
        User CreateUser(string username, string password);
        List<User> GetUsers();
        decimal GetUserBalance(int userId);
    }
}
