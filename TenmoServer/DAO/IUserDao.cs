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
        Account GetAccountByUserId(int userId);
        //decimal GetUserBalance(int userId);
        bool CheckAccountBalance(decimal minAmount, int userId);
        string GetUsernameByAccountId(int accountId);
    }
}
